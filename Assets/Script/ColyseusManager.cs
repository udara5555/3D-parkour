using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;

public class ColyseusManager : MonoBehaviour
{
    [Header("Server")]
    public string serverUrl = "ws://localhost:2567";
    public string roomName = "my_room";

    [Header("Scene refs")]
    public Transform localPlayer;              // drag your Player transform
    public GameObject remotePlayerPrefab;      // prefab for other players

    [Header("Send rate")]
    public float sendInterval = 0.05f;         // 20/sec

    [Header("Smoothing")]
    public float positionLerp = 12f;
    public float rotationSlerp = 12f;

    private ColyseusClient client;
    private ColyseusRoom<MyState> room;

    private float sendTimer;

    private class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public Animator anim;
    }

    private readonly Dictionary<string, RemoteData> remotes = new();

    async void Start()
    {
        client = new ColyseusClient(serverUrl);
        room = await client.JoinOrCreate<MyState>(roomName);

        Debug.Log("Connected. SessionId: " + room.SessionId);

        room.OnStateChange += (state, first) =>
        {
            SyncRemotes(state);
        };
    }

    void Update()
    {
        if (room == null || localPlayer == null) return;

        sendTimer += Time.deltaTime;
        if (sendTimer < sendInterval) return;
        sendTimer = 0f;

        Vector3 pos = localPlayer.position;
        float rotY = localPlayer.eulerAngles.y;

        bool isWalking =
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0;

        bool isSitting = Input.GetKey(KeyCode.C);

        string animState = isSitting ? "sit" : (isWalking ? "walk" : "idle");

        room.Send("move", new Dictionary<string, object>
        {
            { "x", pos.x },
            { "y", pos.y },
            { "z", pos.z },
            { "rotY", rotY },
            { "anim", animState }
        });
    }

    void LateUpdate()
    {
        // smooth remote movement every frame
        foreach (var rd in remotes.Values)
        {
            if (rd.go == null) continue;

            rd.go.transform.position = Vector3.Lerp(
                rd.go.transform.position,
                rd.targetPos,
                Time.deltaTime * positionLerp
            );

            rd.go.transform.rotation = Quaternion.Slerp(
                rd.go.transform.rotation,
                rd.targetRot,
                Time.deltaTime * rotationSlerp
            );
        }
    }

    private void SyncRemotes(MyState state)
    {
        // update / add
        state.players.ForEach((sessionId, obj) =>
        {
            if (room == null) return;
            if (sessionId == room.SessionId) return;

            // IMPORTANT: MapSchema ForEach gives object, cast to Player
            Player p = (Player)obj;

            if (!remotes.TryGetValue(sessionId, out var rd) || rd.go == null)
            {
                GameObject go = Instantiate(remotePlayerPrefab);
                rd = new RemoteData
                {
                    go = go,
                    targetPos = go.transform.position,
                    targetRot = go.transform.rotation,
                    anim = go.GetComponentInChildren<Animator>(true)
                };
                remotes[sessionId] = rd;
            }

            rd.targetPos = new Vector3(p.x, p.y, p.z);
            rd.targetRot = Quaternion.Euler(0f, p.rotY, 0f);

            // remote animation
            if (rd.anim != null)
            {
                rd.anim.SetBool("IsWalking", p.anim == "walk");
                rd.anim.SetBool("Sit", p.anim == "sit");
            }
        });

        // remove players that left
        var toRemove = new List<string>();
        foreach (var id in remotes.Keys)
        {
            if (!state.players.ContainsKey(id))
                toRemove.Add(id);
        }

        foreach (var id in toRemove)
        {
            if (remotes[id].go != null) Destroy(remotes[id].go);
            remotes.Remove(id);
        }
    }
}

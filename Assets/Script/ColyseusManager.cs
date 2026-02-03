using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;

public class ColyseusManager : MonoBehaviour
{
    public string serverUrl = "ws://localhost:2567";
    public string roomName = "my_room";

    public Transform localPlayer;
    public GameObject remotePlayerPrefab;

    ColyseusClient client;
    ColyseusRoom<MyRoomState> room;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
    }

    Dictionary<string, RemoteData> remotes = new();

    float sendTimer;
    public float sendInterval = 0.05f; // 20 updates/sec

    async void Start()
    {
        client = new ColyseusClient(serverUrl);
        room = await client.JoinOrCreate<MyRoomState>(roomName);

        Debug.Log("Connected to room: " + room.Id);

        room.OnStateChange += (state, first) =>
        {
            SyncRemotes(state);
        };
    }

    void SyncRemotes(MyRoomState state)
    {
        // ADD / UPDATE players
        state.players.ForEach((sessionId, p) =>
        {
            if (sessionId == room.SessionId) return;

            if (!remotes.ContainsKey(sessionId))
            {
                var go = Instantiate(remotePlayerPrefab);
                remotes[sessionId] = new RemoteData
                {
                    go = go,
                    targetPos = go.transform.position,
                    targetRot = go.transform.rotation
                };
            }

            var rd = remotes[sessionId];
            rd.targetPos = new Vector3(p.x, p.y, p.z);
            rd.targetRot = Quaternion.Euler(0f, p.rotY, 0f);

            // ADD THIS PART
            Animator a = rd.go.GetComponentInChildren<Animator>(true);
            if (a != null)
            {
                a.SetBool("IsWalking", p.anim == "walk");
                a.SetBool("Sit", p.anim == "sit");
            }
        });

        // REMOVE players that left
        var toRemove = new List<string>();
        foreach (var id in remotes.Keys)
            if (!state.players.ContainsKey(id))
                toRemove.Add(id);

        foreach (var id in toRemove)
        {
            Destroy(remotes[id].go);
            remotes.Remove(id);
        }
    }


    void LateUpdate()
    {
        // SMOOTH interpolation
        foreach (var rd in remotes.Values)
        {
            rd.go.transform.position = Vector3.Lerp(
                rd.go.transform.position,
                rd.targetPos,
                Time.deltaTime * 10f
            );

            rd.go.transform.rotation = Quaternion.Slerp(
                rd.go.transform.rotation,
                rd.targetRot,
                Time.deltaTime * 10f
            );
        }
    }

    void Update()
    {
        if (room == null || localPlayer == null) return;

        sendTimer += Time.deltaTime;
        if (sendTimer < sendInterval) return;
        sendTimer = 0f;

        Vector3 pos = localPlayer.position;
        float rotY = localPlayer.eulerAngles.y;

        // animation state
        bool isWalking =
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0;

        bool isSitting = Input.GetKey(KeyCode.C);

        string animState =
            isSitting ? "sit" :
            isWalking ? "walk" : "idle";

        room.Send("move", new Dictionary<string, object>
    {
        { "x", pos.x },
        { "y", pos.y },
        { "z", pos.z },
        { "rotY", rotY },
        { "anim", animState }   // ADD THIS
    });
    }

}

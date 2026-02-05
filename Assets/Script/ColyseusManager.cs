using System;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;

public class ColyseusManager : MonoBehaviour
{
    public string serverUrl = "http://127.0.0.1:2567";
    public string roomName = "my_room";

    public Transform localPlayer;
    public GameObject remotePlayerPrefab;

    public float sendInterval = 0.05f;
    public float positionLerp = 12f;
    public float rotationSlerp = 12f;

    private Client client;              //  new in 0.17
    private Room<MyState> room;         //  new in 0.17
    private float sendTimer;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public Animator anim;
    }

    readonly Dictionary<string, RemoteData> remotes = new();

    async void Start()
    {
        try
        {
            client = new Client(serverUrl);
            room = await client.JoinOrCreate<MyState>(roomName);

            Debug.Log("Connected: " + room.SessionId);

            var cb = Callbacks.Get(room);
            cb.OnAdd(state => state.players, (sessionId, player) =>
            {
                if (sessionId == room.SessionId) return;

                var go = Instantiate(remotePlayerPrefab);
                remotes[sessionId] = new RemoteData
                {
                    go = go,
                    targetPos = go.transform.position,
                    targetRot = go.transform.rotation,
                    anim = go.GetComponentInChildren<Animator>(true)
                };
            });

            cb.OnRemove(state => state.players, (sessionId, player) =>
            {
                if (remotes.TryGetValue(sessionId, out var rd) && rd.go) Destroy(rd.go);
                remotes.Remove(sessionId);
            });

            cb.OnChange(state => state.players, (sessionId, player) =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;
                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0f, player.rotY, 0f);

                if (rd.anim != null)
                {
                    rd.anim.SetBool("IsWalking", player.anim == "walk");
                    rd.anim.SetBool("Sit", player.anim == "sit");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Join failed: " + e);
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

        bool isWalking = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool isSitting = Input.GetKey(KeyCode.C);
        string animState = isSitting ? "sit" : (isWalking ? "walk" : "idle");

        room.Send("move", new Dictionary<string, object> {
            { "x", pos.x }, { "y", pos.y }, { "z", pos.z },
            { "rotY", rotY }, { "anim", animState }
        });
    }

    void LateUpdate()
    {
        foreach (var rd in remotes.Values)
        {
            if (!rd.go) continue;
            rd.go.transform.position = Vector3.Lerp(rd.go.transform.position, rd.targetPos, Time.deltaTime * positionLerp);
            rd.go.transform.rotation = Quaternion.Slerp(rd.go.transform.rotation, rd.targetRot, Time.deltaTime * rotationSlerp);
        }
    }
}

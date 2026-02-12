using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Colyseus;
using Colyseus.Schema;

public class ColyseusManager : MonoBehaviour
{
    [Header("Server")]
    public string serverUrl = "ws://127.0.0.1:2567";
    public string roomName = "my_room";

    [Header("UI")]
    public TMP_InputField roomCodeInput;   // drag TMP input
    public TMP_Text roomCodeText;          // optional: show created code

    [Header("Scene refs")]
    public Transform localPlayer;
    public GameObject remotePlayerPrefab;

    public float sendInterval = 0.05f;
    public float positionLerp = 12f;
    public float rotationSlerp = 12f;

    private Client client;
    private Room<MyState> room;
    private float sendTimer;

    public bool IsInRoom => room != null;


    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public Animator anim;
    }
    readonly Dictionary<string, RemoteData> remotes = new();

    void Awake()
    {
        client = new Client(serverUrl);
    }

    // Button: Create Room
    public async void CreateRoom()
    {
        await LeaveIfAny();

        room = await client.Create<MyState>(roomName);
        Debug.Log("CREATED roomId: " + room.RoomId);

        if (roomCodeText) roomCodeText.text = "Code: " + room.RoomId;
        HookStateCallbacks();
    }

    // Button: Join With Code
    public async void JoinWithCode()
    {
        await LeaveIfAny();

        string code = roomCodeInput ? roomCodeInput.text.Trim() : "";
        if (string.IsNullOrEmpty(code)) { Debug.LogError("Room code empty"); return; }

        room = await client.JoinById<MyState>(code);
        Debug.Log("JOINED roomId: " + room.RoomId);

        HookStateCallbacks();
    }

    async System.Threading.Tasks.Task LeaveIfAny()
    {
        if (room != null)
        {
            try { await room.Leave(); } catch { }
            room = null;
        }

        foreach (var kv in remotes)
            if (kv.Value.go) Destroy(kv.Value.go);
        remotes.Clear();
    }

    void HookStateCallbacks()
    {
        var cb = Callbacks.Get(room);

        cb.OnAdd(state => state.players, (sessionId, player) =>
        {
            // me
            if (sessionId == room.SessionId) return;

            // spawn remote once
            var go = Instantiate(remotePlayerPrefab);
            go.transform.position = new Vector3(player.x, player.y, player.z);
            go.transform.rotation = Quaternion.Euler(0f, player.rotY, 0f);

            var pm = go.GetComponent<PlayerMovement>(); if (pm) pm.enabled = false;
            var cc = go.GetComponent<CharacterController>(); if (cc) cc.enabled = false;

            remotes[sessionId] = new RemoteData
            {
                go = go,
                targetPos = go.transform.position,
                targetRot = go.transform.rotation,
                anim = go.GetComponentInChildren<Animator>(true)
            };

            ApplySkin(go, (int)player.skin);

            //  THIS is what makes movement update
            cb.OnChange(player, () =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;
                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0f, player.rotY, 0f);

                if (rd.anim != null)
                {
                    rd.anim.SetBool("IsWalking", player.anim == "walk");
                    rd.anim.SetBool("Sit", player.anim == "sit");
                }

                rd.anim = ApplySkin(rd.go, (int)player.skin);




            });
        });

        cb.OnRemove(state => state.players, (sessionId, player) =>
        {
            if (remotes.TryGetValue(sessionId, out var rd) && rd.go) Destroy(rd.go);
            remotes.Remove(sessionId);
        });
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

        foreach (var kv in remotes)
        {
            var r = kv.Value;
            r.go.transform.position = Vector3.Lerp(r.go.transform.position, r.targetPos, Time.deltaTime * positionLerp);
            r.go.transform.rotation = Quaternion.Slerp(r.go.transform.rotation, r.targetRot, Time.deltaTime * rotationSlerp);
        }

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

    public void SendMove(Vector3 pos, float rotY, string anim)
    {
        if (room == null) return;

        Debug.Log("SEND MOVE");

        room.Send("move", new
        {
            x = pos.x,
            y = pos.y,
            z = pos.z,
            rotY = rotY,
            anim = anim
        });

        
    }

    public void SendSkin(int skin)
    {
        if (room == null) return;
        Debug.Log("SEND SKIN " + skin);
        room.Send("skin", new { skin = skin });

    }

    Animator ApplySkin(GameObject go, int skinIndex)
    {
        Animator activeAnim = null;

        var skins = new System.Collections.Generic.List<Transform>();

        foreach (Transform t in go.transform)
        {
            if (t.name == "root" || t.name.StartsWith("character-"))
                skins.Add(t);
        }

        if (skins.Count == 0) return null;

        skinIndex = Mathf.Clamp(skinIndex, 0, skins.Count - 1);

        for (int i = 0; i < skins.Count; i++)
        {
            bool active = (i == skinIndex);
            skins[i].gameObject.SetActive(active);

            if (active)
                activeAnim = skins[i].GetComponentInChildren<Animator>(true);
        }

        return activeAnim;
    }






}

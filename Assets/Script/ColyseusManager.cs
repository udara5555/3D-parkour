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
    public TMP_InputField roomCodeInput;
    public TMP_Text roomCodeText;

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

    public string CurrentPhase { get; private set; } = "waiting";
    public float LocalPlayerSpeed { get; private set; } = 5f;
    public int ServerClickCount { get; private set; } = 0;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Vector3 initialPos;  // Store initial position
        public Quaternion targetRot;
        public Animator anim;
    }
    readonly Dictionary<string, RemoteData> remotes = new();

    void Awake()
    {
        client = new Client(serverUrl);
    }

    public async void CreateRoom()
    {
        await LeaveIfAny();
        room = await client.Create<MyState>(roomName);
        Debug.Log("CREATED roomId: " + room.RoomId);
        if (roomCodeText != null)
            roomCodeText.text = "Room ID :" + room.RoomId;
        HookStateCallbacks();
        LockAndHideCursor();
    }

    public async void JoinWithCode()
    {
        await LeaveIfAny();
        string code = roomCodeInput ? roomCodeInput.text.Trim() : "";
        if (string.IsNullOrEmpty(code)) { Debug.LogError("Room code empty"); return; }
        room = await client.JoinById<MyState>(code);
        Debug.Log("JOINED roomId: " + room.RoomId);
        HookStateCallbacks();
        LockAndHideCursor();
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
            if (sessionId == room.SessionId) return;

            var go = Instantiate(remotePlayerPrefab);
            Vector3 initialPosition = new Vector3(player.x, player.y, player.z);
            go.transform.position = initialPosition;
            Transform visualRoot = go.transform.Find("root");
            if (visualRoot != null)
                visualRoot.rotation = Quaternion.Euler(0f, player.rotY, 0f);

            var pm = go.GetComponent<PlayerMovement>(); if (pm) pm.enabled = false;
            var cc = go.GetComponent<CharacterController>(); if (cc) cc.enabled = false;

            var anim = ApplySkin(go, (int)player.skin);

            remotes[sessionId] = new RemoteData
            {
                go = go,
                targetPos = go.transform.position,
                initialPos = initialPosition,  // Store initial position
                targetRot = go.transform.rotation,
                anim = anim
            };

            cb.OnChange(player, () =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;
                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0f, player.rotY, 0f);

                if (rd.anim != null)
                {
                    rd.anim.SetBool("IsWalking", player.anim == "walk");
                    rd.anim.SetBool("Sit", player.anim == "sit");
                    rd.anim.SetBool("Jump", player.anim == "jump");
                }

                rd.anim = ApplySkin(rd.go, (int)player.skin);
            });
        });

        cb.OnRemove(state => state.players, (sessionId, player) =>
        {
            if (remotes.TryGetValue(sessionId, out var rd) && rd.go)
                Destroy(rd.go);
            remotes.Remove(sessionId);
        });

        // NOTE: SetLocalPlayerFrozen is REMOVED intentionally.
        // PlayerMovement stays enabled and checks CurrentPhase itself.
        // Disabling PlayerMovement would block click detection too.
        room.OnStateChange += (state, isFirst) =>
        {
            int total = 0;
            int readyCount = 0;
            foreach (Player p in state.players.Values)
            {
                total++;
                if (p.ready) readyCount++;
            }
            CountdownUI.Instance?.UpdateWaiting(readyCount, total);

            if (CurrentPhase != state.phase)
            {
                CurrentPhase = state.phase;
                Debug.Log("Phase changed: " + state.phase);

                if (state.phase == "countdown") CountdownUI.Instance?.Show();
                if (state.phase == "racing") CountdownUI.Instance?.Show();
                if (state.phase == "waiting")
                {
                    CountdownUI.Instance?.Hide();
                    // Reset all players to their initial positions
                    ReturnPlayersToInitialPositions();
                    FindAnyObjectByType<FloorScaler>()?.ResetFloorScale();

                    FindAnyObjectByType<WinMarkerSpawner>()?.ResetSpawner();
                    //FindAnyObjectByType<WinMarkerSpawner>()?.ResetSpawner();

                    
                }
            }

            if (state.phase == "countdown")
            {
                CountdownUI.Instance?.UpdateText((int)state.countdown);

                if (state.players.TryGetValue(room.SessionId, out var localP))
                    ServerClickCount = (int)localP.clicks;
            }

            if (state.phase == "racing")
            {
                CountdownUI.Instance?.UpdateRacingTimer((int)state.countdown);

                if (state.players.TryGetValue(room.SessionId, out var localP))
                {
                    LocalPlayerSpeed = localP.speed;
                    ServerClickCount = (int)localP.clicks;
                    Debug.Log("Race started! Clicks: " + ServerClickCount + " | Speed: " + LocalPlayerSpeed);
                }
            }
        };
    }

    void ReturnPlayersToInitialPositions()
    {
        // Return remote players to their initial positions
        foreach (var rd in remotes.Values)
        {
            if (rd.go)
            {
                rd.targetPos = rd.initialPos;
                rd.go.transform.position = rd.initialPos;
                Debug.Log("Player returned to initial position: " + rd.initialPos);
            }
        }

        // Return local player to their initial position
        if (localPlayer != null && room != null)
        {
            if (room.State.players.TryGetValue(room.SessionId, out var localP))
            {
                Vector3 initialPos = new Vector3(localP.x, localP.y, localP.z);
                localPlayer.position = initialPos;
                Debug.Log("Local player returned to initial position: " + initialPos);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockAndShowCursor();

        if (room == null || localPlayer == null) return;

        sendTimer += Time.deltaTime;
        if (sendTimer < sendInterval) return;
        sendTimer = 0f;

        Vector3 pos = localPlayer.position;
        float rotY = localPlayer.GetComponent<PlayerMovement>().characterModel.eulerAngles.y;

        var ccLocal = localPlayer.GetComponent<CharacterController>();
        bool isWalkingInput = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool isSitting = Input.GetKey(KeyCode.C);
        bool isInAir = (ccLocal != null) ? !ccLocal.isGrounded : false;
        string animState = isSitting ? "sit" : (isInAir ? "jump" : (isWalkingInput ? "walk" : "idle"));

        room.Send("move", new Dictionary<string, object> {
            { "x", pos.x }, { "y", pos.y }, { "z", pos.z },
            { "rotY", rotY }, { "anim", animState }
        });

        foreach (var kv in remotes)
        {
            var r = kv.Value;
            r.go.transform.position = Vector3.Lerp(r.go.transform.position, r.targetPos, Time.deltaTime * positionLerp);
            Transform visualRoot = null;

            var r0 = r.go.transform.Find("root");
            if (r0 != null && r0.gameObject.activeSelf)
                visualRoot = r0;

            if (visualRoot == null)
            {
                foreach (Transform t in r.go.transform)
                {
                    if (t.name.StartsWith("character-") && t.gameObject.activeSelf)
                    {
                        visualRoot = t.Find("root");
                        break;
                    }
                }
            }

            if (visualRoot != null)
            {
                visualRoot.rotation = Quaternion.Slerp(
                    visualRoot.rotation,
                    r.targetRot,
                    Time.deltaTime * rotationSlerp
                );
            }
        }
    }

    void LateUpdate()
    {
        foreach (var rd in remotes.Values)
        {
            if (!rd.go) continue;
            rd.go.transform.position = Vector3.Lerp(rd.go.transform.position, rd.targetPos, Time.deltaTime * positionLerp);
        }
    }

    public void SendMove(Vector3 pos, float rotY, string anim)
    {
        if (room == null) return;
        room.Send("move", new { x = pos.x, y = pos.y, z = pos.z, rotY = rotY, anim = anim });
    }

    public void SendSkin(int skin)
    {
        if (room == null) return;
        room.Send("skin", new { skin = skin });
    }

    public void SendReady(bool isReady)
    {
        if (room == null) return;
        room.Send(isReady ? "player_ready" : "player_unready");
    }

    public void SendClick()
    {
        if (room == null) return;
        room.Send("click");
    }

    Animator ApplySkin(GameObject go, int skinIndex)
    {
        Animator activeAnim = null;
        var skins = new System.Collections.Generic.List<Transform>();

        foreach (Transform t in go.transform)
            if (t.name == "root" || t.name.StartsWith("character-"))
                skins.Add(t);

        if (skins.Count == 0) return go.GetComponentInChildren<Animator>(true);

        skinIndex = Mathf.Clamp(skinIndex, 0, skins.Count - 1);

        for (int i = 0; i < skins.Count; i++)
        {
            bool active = (i == skinIndex);
            skins[i].gameObject.SetActive(active);
            if (active)
                activeAnim = skins[i].GetComponentInChildren<Animator>(true);
        }

        if (activeAnim == null)
            activeAnim = go.GetComponentInChildren<Animator>(true);

        return activeAnim;
    }

    void LockAndHideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void UnlockAndShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SendBonusClicks(int amount)
    {
        if (room == null) return;
        room.Send("bonus_clicks", new { amount = amount });
    }
}
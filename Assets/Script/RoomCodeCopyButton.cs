using TMPro;
using UnityEngine;

public class RoomCodeCopyButton : MonoBehaviour
{
    public TextMeshProUGUI roomCodeText;
    private string roomCode;

    public void CopyRoomCode()
    {
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Room code is empty!");
            return;
        }

        Debug.Log("Attempting to copy room code: " + roomCode);

        // Copy to clipboard (Editor only)
        #if UNITY_EDITOR
        GUIUtility.systemCopyBuffer = roomCode;
        Debug.Log("Room code copied to clipboard (Editor): " + roomCode);
        #endif

        // Copy using browser API (WebGL only)
        #if UNITY_WEBGL && !UNITY_EDITOR
        CopyToClipboardWebGL(roomCode);
        Debug.Log("Sent to clipboard via WebGL plugin: " + roomCode);
        #endif

        // Fallback for standalone builds
        #if !UNITY_EDITOR && !UNITY_WEBGL
        GUIUtility.systemCopyBuffer = roomCode;
        Debug.Log("Room code copied to clipboard (Standalone): " + roomCode);
        #endif
    }

    public void SetRoomCode(string code)
    {
        roomCode = code;
        Debug.Log("Room code set to: " + code);
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void CopyToClipboardWebGL(string text);
    #endif
}

using UnityEngine;
using TMPro;

public class ClickUI : MonoBehaviour
{
    public TMP_Text startClicksText;
    public TMP_Text currentClicksText;
    public TMP_Text winCountText;

    private PlayerMovementNew player;
    private WinMarkerSpawner winSpawner;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovementNew>();
        winSpawner = FindAnyObjectByType<WinMarkerSpawner>();
        
        if (player == null)
        {
            Debug.LogError("PlayerMovementNew not found!");
        }

        if (winSpawner == null)
        {
            Debug.LogError("WinMarkerSpawner not found!");
        }

        if (startClicksText == null)
        {
            Debug.LogError("startClicksText is not assigned!");
        }

        if (currentClicksText == null)
        {
            Debug.LogError("currentClicksText is not assigned!");
        }

        if (winCountText == null)
        {
            Debug.LogError("winCountText is not assigned!");
        }
    }

    void Update()
    {
        if (player == null) return;
        if (startClicksText == null || currentClicksText == null || winCountText == null) return;

        int start = player.startClicks > 0 ? player.startClicks : 0;

        startClicksText.text = "Bonus: " + start;
        currentClicksText.text = "Clicks: " + player.localClickCount;
        winCountText.text = "Wins: " + (winSpawner != null ? winSpawner.GetWins() : 0);
    }
}
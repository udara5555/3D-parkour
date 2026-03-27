using UnityEngine;
using TMPro;

public class ClickUI : MonoBehaviour
{
    public TMP_Text startClicksText;
    public TMP_Text currentClicksText;

    private PlayerMovement player;

    

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (player == null) return;

        int current = player.startClicks;

        // start clicks = first value when countdown started (approx)
        int start = current > 0 ? current : 0;

        startClicksText.text = "Bonus: " + start;
        currentClicksText.text = "Clicks: " + (player.localClickCount);
    }


}
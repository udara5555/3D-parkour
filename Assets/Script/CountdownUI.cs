using UnityEngine;
using TMPro;

// Attach this to a Canvas Panel GameObject
public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance;

    [Header("Countdown Panel")]
    public TMP_Text countdownText;   // shows 5,4,3,2,1,GO! or 10,9,8...1,GO!
    public GameObject panel;         // the countdown panel

    [Header("Waiting UI (optional)")]
    public TMP_Text waitingText;     // shows "Waiting... (1/2 ready)"

    void Awake()
    {
        Instance = this;
        if (panel) panel.SetActive(false);  // hidden at start
    }

    public void Show()
    {
        if (panel) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
    }

    public void UpdateText(int value)
    {
        if (countdownText == null) return;
        countdownText.text = value > 0 ? value.ToString() : "GO!";
    }

    public void UpdateRacingTimer(int value)
    {
        if (countdownText == null) return;
        countdownText.text = value > 0 ? value.ToString() : "GO!";
    }

    // called every state change to show how many are ready
    public void UpdateWaiting(int readyCount, int totalCount)
    {
        if (waitingText == null) return;
        waitingText.text = "Waiting... (" + readyCount + "/" + totalCount + " ready)";
    }
}
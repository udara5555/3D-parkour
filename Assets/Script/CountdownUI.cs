using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance;

    [Header("Countdown Panel")]
    public TMP_Text countdownText;
    public GameObject panel; // countdown panel

    [Header("Race Panel")] 
    public TMP_Text raceText;
    public GameObject racePanel;

    [Header("Waiting Panel")] 
    public TMP_Text waitingText;
    public GameObject waitingPanel;

    void Awake()
    {
        Instance = this;

        if (panel) panel.SetActive(false);
        if (racePanel) racePanel.SetActive(false);     
        if (waitingPanel) waitingPanel.SetActive(true); 
    }

    public void Show()
    {
        if (panel) panel.SetActive(true);
        if (racePanel) racePanel.SetActive(false);  
        if (waitingPanel) waitingPanel.SetActive(false);
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        if (racePanel) racePanel.SetActive(false);  
        if (waitingPanel) waitingPanel.SetActive(true); 
    }

    public void UpdateText(int value)
    {
        if (countdownText == null) return;

        Show(); // ensure countdown panel active

        countdownText.text = value > 0 ? value.ToString() : "GO!";
    }

    public void UpdateRacingTimer(int value)
    {
        if (raceText == null) return;

        if (panel) panel.SetActive(false);        //  switch panels
        if (waitingPanel) waitingPanel.SetActive(false);
        if (racePanel) racePanel.SetActive(true);

        raceText.text = value.ToString();
    }

    public void UpdateWaiting(int readyCount, int totalCount)
    {
        if (waitingText == null) return;

        if (panel) panel.SetActive(false);        //  switch panels
        if (racePanel) racePanel.SetActive(false);
        if (waitingPanel) waitingPanel.SetActive(true);

        waitingText.text = "Waiting... (" + readyCount + "/" + totalCount + " ready)";
    }
}
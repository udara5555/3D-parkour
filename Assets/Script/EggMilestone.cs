using UnityEngine;
using TMPro;

public class EggMilestone : MonoBehaviour
{
    public int requiredWins;
    public int eggValue;

    public GameObject lockVisual;
    public TMP_Text label;

    private WinMarkerSpawner spawner;
    private bool isUnlocked = false;

    void Start()
    {
        spawner = FindAnyObjectByType<WinMarkerSpawner>();

        if (label != null)
            label.text = requiredWins + " wins";
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("LocalPlayer")) return;

        int wins = spawner.GetWins();

        if (wins >= requiredWins && !isUnlocked)
        {
            isUnlocked = true;

            if (lockVisual != null)
                lockVisual.SetActive(false);

            Debug.Log("Unlocked egg: " + eggValue);
        }
        else
        {
            Debug.Log("Not enough wins");
        }
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}
using UnityEngine;
using TMPro;

public class EggMilestone : MonoBehaviour
{
    public int requiredWins;
    public int eggValue;

    public GameObject lockVisual;
    public TMP_Text label;

    public AudioClip unlockSound;
    public AudioClip notEnoughWinsSound;

    private WinMarkerSpawner spawner;
    private AudioSource audioSource;
    private bool isUnlocked = false;

    void Start()
    {
        spawner = FindAnyObjectByType<WinMarkerSpawner>();

        // Add AudioSource component if not already present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.playOnAwake = false;

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

            PlaySound(unlockSound);
            Debug.Log("Unlocked egg: " + eggValue);
        }
        else
        {
            PlaySound(notEnoughWinsSound);
            Debug.Log("Not enough wins");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}
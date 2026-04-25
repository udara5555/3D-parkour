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

    public GameObject notEnoughWinsPanel;
    public GameObject unlockConfirmPanel;
    public UnityEngine.UI.Button acceptButton;
    public UnityEngine.UI.Button denyButton;

    private WinMarkerSpawner spawner;
    private AudioSource audioSource;
    private bool isUnlocked = false;
    private bool playerInContact = false;
    private PlayerMovementNew playerMovement;

    void Start()
    {
        spawner = FindAnyObjectByType<WinMarkerSpawner>();
        playerMovement = FindAnyObjectByType<PlayerMovementNew>();

        // Add AudioSource component if not already present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.playOnAwake = false;

        if (label != null)
            label.text = requiredWins + " wins";

        // Ensure panels are hidden at start
        if (notEnoughWinsPanel != null)
            notEnoughWinsPanel.SetActive(false);

        if (unlockConfirmPanel != null)
            unlockConfirmPanel.SetActive(false);

        // Setup button listeners
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptUnlock);

        if (denyButton != null)
            denyButton.onClick.AddListener(OnDenyUnlock);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("LocalPlayer")) return;

        playerInContact = true;

        int wins = spawner.GetWins();

        if (wins >= requiredWins && !isUnlocked)
        {
            PlaySound(unlockSound);
            Debug.Log("Unlocked egg: " + eggValue);

            // Show confirmation panel instead of immediately unlocking
            if (unlockConfirmPanel != null)
                unlockConfirmPanel.SetActive(true);
        }
        else if (!isUnlocked)
        {
            PlaySound(notEnoughWinsSound);
            Debug.Log("Not enough wins");

            if (notEnoughWinsPanel != null)
                notEnoughWinsPanel.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("LocalPlayer")) return;

        playerInContact = false;

        if (notEnoughWinsPanel != null)
            notEnoughWinsPanel.SetActive(false);

        if (unlockConfirmPanel != null)
            unlockConfirmPanel.SetActive(false);
    }

    private void OnAcceptUnlock()
    {
        isUnlocked = true;

        if (lockVisual != null)
            lockVisual.SetActive(false);

        // Add bonus click value
        if (playerMovement != null)
        {
            playerMovement.startClicks += eggValue / 10;
            Debug.Log("Bonus clicks added: " + (eggValue / 10));
        }

        // Hide confirmation panel
        if (unlockConfirmPanel != null)
            unlockConfirmPanel.SetActive(false);

        Debug.Log("Egg unlocked and bonus accepted!");
    }

    private void OnDenyUnlock()
    {
        // Hide confirmation panel without unlocking
        if (unlockConfirmPanel != null)
            unlockConfirmPanel.SetActive(false);

        Debug.Log("Bonus denied!");
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
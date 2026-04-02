using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MuteButton : MonoBehaviour
{
    private Button muteButton;
    private TMP_Text buttonText;
    private Image buttonImage;

    public Sprite muteSprite;
    public Sprite unmuteSprite;

    void Start()
    {
        muteButton = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
        buttonImage = GetComponent<Image>();

        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found in scene!");
        }

        UpdateButtonVisuals();
    }

    public void OnMuteButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateButtonVisuals();
        }
    }

    private void UpdateButtonVisuals()
    {
        if (buttonText != null && AudioManager.Instance != null)
        {
            buttonText.text = AudioManager.Instance.IsMuted() ? "Unmute" : "Mute";
        }

        if (buttonImage != null && AudioManager.Instance != null)
        {
            buttonImage.sprite = AudioManager.Instance.IsMuted() ? unmuteSprite : muteSprite;
        }
    }
}
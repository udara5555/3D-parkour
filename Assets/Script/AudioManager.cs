using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private bool isMuted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        AudioListener.pause = mute;
        Debug.Log("Mute set to: " + mute);
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    public void ToggleMute()
    {
        SetMute(!isMuted);
    }
}
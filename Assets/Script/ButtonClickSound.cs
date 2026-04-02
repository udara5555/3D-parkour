using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        // add AudioSource on this GameObject automatically
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = clickSound;
    }

    public void PlaySound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.Play();
    }
}
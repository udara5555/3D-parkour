using UnityEngine;
using UnityEngine.UI;

public class ClickEffect : MonoBehaviour
{
    private float duration = 0.5f;
    private float timer = 0f;
    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        // fade out + scale up
        if (img != null)
        {
            Color c = img.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            img.color = c;
        }

        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, t);

        if (timer >= duration)
            Destroy(gameObject);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class DayNightChange : MonoBehaviour
{
    private Button dayNightButton;
    private Image buttonImage;

    // Sky materials
    public Material dayMaterial;
    public Material nightMaterial;

    // Button textures
    public Sprite dayButtonTexture;
    public Sprite nightButtonTexture;

    private bool isDay = true;

    void Start()
    {
        dayNightButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        if (dayNightButton != null)
        {
            dayNightButton.onClick.AddListener(OnDayNightButtonClicked);
        }
        else
        {
            Debug.LogError("Button component not found on DayNightChange GameObject!");
        }

        UpdateDayNightTextures();
    }

    public void OnDayNightButtonClicked()
    {
        isDay = !isDay;
        UpdateDayNightTextures();
    }

    private void UpdateDayNightTextures()
    {
        // Change sky material
        if (isDay)
        {
            RenderSettings.skybox = dayMaterial;
        }
        else
        {
            RenderSettings.skybox = nightMaterial;
        }

        // Change button texture
        if (buttonImage != null)
        {
            buttonImage.sprite = isDay ? dayButtonTexture : nightButtonTexture;
        }
    }
}
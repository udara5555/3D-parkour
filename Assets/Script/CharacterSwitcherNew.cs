using UnityEngine;

public class CharacterSwitcherNew : MonoBehaviour
{
    [Header("Skin Materials")]
    public Material[] skinMaterials;  // Array of 2 materials (Skin1, Skin2)

    [Header("Child Renderers")]
    public Renderer armsRenderer;     // Cube.010 (arms)
    public Renderer bodyRenderer;     // Cube.011 (body, legs, head)

    private int currentSkinIndex = 0;

    void Start()
    {
        if (skinMaterials == null || skinMaterials.Length < 2)
        {
            Debug.LogError("CharacterSwitcherNew: Need 2 materials assigned!");
            return;
        }

        ApplySkin(0);
    }

    public void ChangeSkin()
    {
        currentSkinIndex = (currentSkinIndex + 1) % skinMaterials.Length;
        ApplySkin(currentSkinIndex);
    }

    private void ApplySkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= skinMaterials.Length)
            return;

        Material activeMaterial = skinMaterials[skinIndex];

        if (armsRenderer != null)
            armsRenderer.material = activeMaterial;

        if (bodyRenderer != null)
            bodyRenderer.material = activeMaterial;

        Debug.Log($"Skin changed to: {activeMaterial.name}");
    }
}

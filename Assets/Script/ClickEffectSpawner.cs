using UnityEngine;

public class ClickEffectSpawner : MonoBehaviour
{
    [Header("Refs")]
    public GameObject clickEffectPrefab;  // drag ClickEffect prefab here
    public Canvas canvas;                  // drag your main Canvas here

    private ColyseusManager net;

    void Start()
    {
        net = FindAnyObjectByType<ColyseusManager>();
    }

    void Update()
    {
        // only spawn effect during countdown phase
        if (net == null || net.CurrentPhase != "countdown") return;

        if (Input.GetMouseButtonDown(0))
            SpawnEffect(Input.mousePosition);
    }

    void SpawnEffect(Vector2 screenPos)
    {
        if (clickEffectPrefab == null || canvas == null) return;

        GameObject fx = Instantiate(clickEffectPrefab, canvas.transform);

        // convert screen position to canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
        );

        fx.GetComponent<RectTransform>().localPosition = localPos;
    }
}
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Characters")]
    public GameObject player;        // default (character-a / root)
    public GameObject[] others;      // character-b ... r

    [Header("UI")]
    public GameObject skinPanel;     // SkinPanel object

    public Animator ActiveAnimator { get; private set; }  //ADD

    int currentIndex = 0;

    void Start()
    {
        if (skinPanel) skinPanel.SetActive(false);
        ClosePanel();
        SelectSkin(0); // default active
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            OpenPanel();
    }

    public void OpenPanel()
    {
        if (!skinPanel) return;
        skinPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        if (skinPanel) skinPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SelectSkin(int id)
    {
        currentIndex = Mathf.Clamp(id, 0, others.Length);

        bool usePlayer = (currentIndex == 0);
        if (player) player.SetActive(usePlayer);

        for (int i = 0; i < others.Length; i++)
            if (others[i]) others[i].SetActive(!usePlayer && i == currentIndex - 1);

        //set active animator for PlayerMovement
        GameObject activeObj = usePlayer ? player : others[currentIndex - 1];
        ActiveAnimator = activeObj ? activeObj.GetComponentInChildren<Animator>(true) : null;

        ClosePanel();
        GetComponent<PlayerMovement>()?.RefreshAnimator();
    }
}

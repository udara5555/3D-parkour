using UnityEngine;

public class RunInBg : MonoBehaviour
{
    void Awake()
    {
        Application.runInBackground = true;
    }
}

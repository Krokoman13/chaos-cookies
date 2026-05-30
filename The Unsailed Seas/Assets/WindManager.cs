using UnityEngine;

public class WindManager : MonoBehaviour
{
    static WindManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Duplicate wind instance found!");
        }

        instance = this;
    }
}

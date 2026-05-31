using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager instance;

    public float windAngle_degrees = 0;
    public float windSpeed = 1;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Duplicate wind instance found!");
        }

        instance = this;
    }
}

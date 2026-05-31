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

    public void RandomizeWind()
    {
        windAngle_degrees = Random.value * 360.0f;
    }

    public void FixedUpdate()
    {
        windAngle_degrees += (Random.value * 2 - 1.0f);

        windAngle_degrees = Mathf.Abs(windAngle_degrees);

        windAngle_degrees %= 360;
    }
}

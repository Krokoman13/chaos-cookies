using UnityEngine;

public class WindIndicator : MonoBehaviour
{
    [SerializeField] RectTransform windArrow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        windArrow = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Transform mainCamera = Camera.main.transform;

        float windHeading = WindManager.instance.windAngle_degrees;

        // Camera heading around Y axis
        float cameraHeading = mainCamera.eulerAngles.y;

        // Wind direction relative to camera
        float relativeAngle = windHeading - cameraHeading;

        // Arrow's 0° points down, so add 180° to make it point toward the wind heading
        float arrowRotation = relativeAngle;

        windArrow.localEulerAngles = new Vector3(0f, 0f, -arrowRotation);
    }
}

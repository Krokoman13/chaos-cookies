using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float turnSpeed = 50f;
    [SerializeField] private float sailTurnSpeed = 50f;

    [SerializeField] private Transform flagTransform;
    [SerializeField] private List<Transform> sails;

    [SerializeField] private float sailNormalAngle = 0f;
    [SerializeField] private float sailNormalMinAngle = -85f;
    [SerializeField] private float sailNormalMaxAngle = 85f;

    [SerializeField] float forwardVelocity = 0.1f;
    [SerializeField] float turnSpeed = 120f;

    [Header("Water")]
    [SerializeField] WaterWaveSettings waveSettings;
    [SerializeField] float heightOffset = 0.35f;




    [Header("Boat Model")]
    [SerializeField] Transform BoatModel;
    [SerializeField] float modelHeightSmoothTime = 0.08f;
    [SerializeField] float modelTiltStrength = 0.4f;
    [SerializeField] float visualHeightStrength = 0.35f;
    [SerializeField] float maxVisualHeightOffset = 0.18f;

    const float normalSampleDistance = 0.8f;

    Vector3 boatModelBaseLocalPosition;
    Quaternion boatModelBaseLocalRotation;

    float modelYVelocity;
    float baseWaterY;
    bool hasBaseWaterY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (BoatModel != null)
        {
            boatModelBaseLocalPosition = BoatModel.localPosition;
            boatModelBaseLocalRotation = BoatModel.localRotation;
        }
    }

    Vector2 Project(Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / Vector2.Dot(b, b) * b;
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Ship steering
        if (Input.GetKey(KeyCode.A) != Input.GetKey(KeyCode.D))
        {
            Vector3 targetRotation = transform.rotation.eulerAngles;

            if (Input.GetKey(KeyCode.A))
            {
                targetRotation.y -= turnSpeed * Time.fixedDeltaTime;
            }
            else
            {
                targetRotation.y += turnSpeed * Time.fixedDeltaTime;
            }

            rb.MoveRotation(Quaternion.Euler(targetRotation));
        }

        // Ship forward direction
        Vector2 shipForward = new Vector2(
            transform.forward.x,
            transform.forward.z
        ).normalized;

        // Sail normal in world space
        float sailWorldAngle = transform.eulerAngles.y + sailNormalAngle;

        Vector2 sailNormal = new Vector2(
            Mathf.Sin(sailWorldAngle * Mathf.Deg2Rad),
            Mathf.Cos(sailWorldAngle * Mathf.Deg2Rad)
        ).normalized;

        // Wind vector
        Vector2 windForce = new Vector2(
            Mathf.Sin(WindManager.instance.windAngle_degrees * Mathf.Deg2Rad),
            Mathf.Cos(WindManager.instance.windAngle_degrees * Mathf.Deg2Rad)
        ) * WindManager.instance.windSpeed;

        Vector2 reflectedForce = Vector2.Reflect(windForce.normalized, sailNormal);

        reflectedForce *= Mathf.Abs(Vector2.Dot(reflectedForce, sailNormal)) * WindManager.instance.windSpeed;

        Vector2 forceOnBoat = -reflectedForce;

        Vector2 finalForce = Project(forceOnBoat, shipForward);

        rb.AddForce(
            new Vector3(finalForce.x, 0, finalForce.y)
        );
    }

    private void Update()
    {
        // Flag points with the wind
        if (flagTransform)
        {
            flagTransform.rotation = Quaternion.Euler(
                0f,
                WindManager.instance.windAngle_degrees + 90.0f,
                0f
            );
        }

        // Sail controls
        if (Input.GetKey(KeyCode.LeftArrow) != Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                sailNormalAngle -= sailTurnSpeed * Time.deltaTime;
            }
            else
            {
                sailNormalAngle += sailTurnSpeed * Time.deltaTime;
            }
        }

        sailNormalAngle = Mathf.Clamp(
            sailNormalAngle,
            sailNormalMinAngle,
            sailNormalMaxAngle
        );

        // Visual sail rotation
        foreach (Transform sail in sails)
        {
            sail.localRotation = Quaternion.Euler(
                0f,
                90f + sailNormalAngle,
                90f
            );
        }
        else
        {
            forwardVector = BoatModel.forward;
        }

        Vector3 forwardOnWater = Vector3.ProjectOnPlane(
            forwardVector,
            upVector
        );

        if (forwardOnWater.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(
            forwardOnWater.normalized,
            upVector
        );

        BoatModel.rotation = Quaternion.RotateTowards(
            BoatModel.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }
}
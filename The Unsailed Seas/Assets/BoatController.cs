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

    [SerializeField] private ParticleSystem waterParticals;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    Vector2 Project(Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / Vector2.Dot(b, b) * b;
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        float velocity = rb.linearVelocity.magnitude;

        var main = waterParticals.main;

        main.startSpeed = 0.1f + (velocity * 0.25f);


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
    }
}
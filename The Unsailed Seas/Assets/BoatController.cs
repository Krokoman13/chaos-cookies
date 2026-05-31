using UnityEngine;

public class BoatController : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] float forwardVelocity = 0.1f;
    [SerializeField] float turnSpeed = 120f;

    const float normalSampleDistance = 0.8f;

    Vector3 boatModelBaseLocalPosition;
    Quaternion boatModelBaseLocalRotation;

    float modelYVelocity;
    float baseWaterY;
    bool hasBaseWaterY;

    void HandleMovement()
    {
        if (Camera.main == null)
            return;

        Vector3 camera_t_boat = transform.position - Camera.main.transform.position;

        Vector3 forwardVector = camera_t_boat;
        forwardVector.y = 0f;

        if (forwardVector.sqrMagnitude < 0.001f)
            return;

        forwardVector = forwardVector.normalized;

        Vector3 backwardVector = -forwardVector;
        Vector3 rightVector = Quaternion.AngleAxis(90f, Vector3.up) * forwardVector;
        Vector3 leftVector = -rightVector;

        Vector3 directionVector = Vector3.zero;

        if (Input.GetKey(KeyCode.S))
            directionVector += backwardVector;

        if (Input.GetKey(KeyCode.W))
            directionVector += forwardVector;

        if (Input.GetKey(KeyCode.A))
            directionVector += leftVector;

        if (Input.GetKey(KeyCode.D))
            directionVector += rightVector;

        if (directionVector.sqrMagnitude > 0.1f)
        {
            directionVector = directionVector.normalized * forwardVelocity;
            rb.AddForce(directionVector);
        }
    }
}
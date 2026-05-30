using UnityEngine;

public class BoatController : MonoBehaviour
{
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [SerializeField] float forwardVelocity = 0.1f;
    [SerializeField] float turnSpeed = 0.1f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 camera_t_boat = transform.position - Camera.main.transform.position;


        Vector3 forwardVector = camera_t_boat;
        forwardVector.y = 0;
        forwardVector = forwardVector.normalized;

        Vector3 backwardVector = -forwardVector;
        Vector3 rightVector = Quaternion.AngleAxis(90, Vector3.up) * forwardVector;
        Vector3 leftVector = -rightVector;

        Vector3 directionVector = Vector3.zero;

        if (Input.GetKey(KeyCode.S))
        {
            directionVector += backwardVector;
        }

        if (Input.GetKey(KeyCode.W))
        {
            directionVector += forwardVector;
        }

        if (Input.GetKey(KeyCode.A))
        {
            directionVector += leftVector;
        }

        if (Input.GetKey(KeyCode.D))
        {
            directionVector += rightVector;
        }

        if (directionVector.sqrMagnitude > 0.1)
        {
            directionVector = directionVector.normalized * forwardVelocity;

            rb.AddForce(directionVector);
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;

        if (velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(velocity.normalized);

            rb.MoveRotation(
                Quaternion.RotateTowards(
                    rb.rotation,
                    targetRotation,
                    turnSpeed
                )
            );
        }
    }
}

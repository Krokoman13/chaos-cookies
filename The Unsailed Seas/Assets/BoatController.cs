using UnityEngine;

public class BoatController : MonoBehaviour
{
    Rigidbody rb;

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

    void FixedUpdate()
    {
        if (rb == null)
            return;

        HandleMovement();

        // Important:
        // Do NOT apply water buoyancy to the Rigidbody here.
        // The root boat object stays fixed/stable for 2D-style collisions.
        // Only the child BoatModel moves visually.
    }

    void LateUpdate()
    {
        UpdateBoatModelVisual();
    }

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

    void UpdateBoatModelVisual()
    {
        if (BoatModel == null)
            return;

        UpdateBoatModelHeight();
        UpdateBoatModelRotation();
    }

    void UpdateBoatModelHeight()
    {
        Vector3 modelLocalPosition = BoatModel.localPosition;

        if (waveSettings == null)
        {
            hasBaseWaterY = false;

            modelLocalPosition.y = Mathf.SmoothDamp(
                BoatModel.localPosition.y,
                boatModelBaseLocalPosition.y,
                ref modelYVelocity,
                modelHeightSmoothTime
            );

            BoatModel.localPosition = modelLocalPosition;
            return;
        }

        float waterY = GetWaterYAtBoatModelBase();

        if (!hasBaseWaterY)
        {
            baseWaterY = waterY;
            hasBaseWaterY = true;
        }

        float waterDelta = waterY - baseWaterY;

        waterDelta *= visualHeightStrength;

        waterDelta = Mathf.Clamp(
            waterDelta,
            -maxVisualHeightOffset,
            maxVisualHeightOffset
        );

        float targetLocalY =
            boatModelBaseLocalPosition.y
            + heightOffset
            + waterDelta;

        modelLocalPosition.y = Mathf.SmoothDamp(
            BoatModel.localPosition.y,
            targetLocalY,
            ref modelYVelocity,
            modelHeightSmoothTime
        );

        modelLocalPosition.x = boatModelBaseLocalPosition.x;
        modelLocalPosition.z = boatModelBaseLocalPosition.z;

        BoatModel.localPosition = modelLocalPosition;
    }
    float GetWaterYAtBoatModelBase()
    {
        Vector3 sampleWorldPosition = transform.TransformPoint(
            boatModelBaseLocalPosition
        );

        Vector2 worldXZ = new Vector2(
            sampleWorldPosition.x,
            sampleWorldPosition.z
        );

        return WaterWaveMath.EvaluateSurfaceY(
            worldXZ,
            waveSettings,
            Time.time
        );
    }

    void UpdateBoatModelRotation()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;

        Vector3 upVector = GetWaterUpVector();

        Vector3 forwardVector;

        if (velocity.sqrMagnitude > 0.1f)
        {
            forwardVector = velocity.normalized;
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

    Vector3 GetWaterUpVector()
    {
        if (waveSettings == null)
            return Vector3.up;

        Vector3 sampleWorldPosition = transform.TransformPoint(
            boatModelBaseLocalPosition
        );

        Vector2 worldXZ = new Vector2(
            sampleWorldPosition.x,
            sampleWorldPosition.z
        );

        Vector3 waveNormal = WaterWaveMath.EvaluateNormal(
            worldXZ,
            waveSettings,
            Time.time,
            normalSampleDistance
        );

        return Vector3.Slerp(
            Vector3.up,
            waveNormal,
            modelTiltStrength
        );
    }
}
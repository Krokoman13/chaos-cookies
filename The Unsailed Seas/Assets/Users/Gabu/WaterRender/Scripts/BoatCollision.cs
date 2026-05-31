using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatBuoyancy : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] WaterWaveSettings waveSettings;
    [SerializeField] float waterLevelOffset = 0f;

    [Header("Buoyancy Points")]
    [SerializeField] Transform[] buoyancyPoints;

    [Header("Floating")]
    [SerializeField] float floatStrength = 20f;
    [SerializeField] float waterDrag = 8f;
    [SerializeField] float sinkDepth = 1.5f;

    [Header("Stability")]
    [SerializeField] float uprightStrength = 6f;
    [SerializeField] float uprightDamping = 1.5f;
    [SerializeField] float horizontalDrag = 1f;

    [Header("Debug")]
    [SerializeField] bool debugBuoyancy = false;
    [SerializeField] float debugInterval = 1f;

    float nextDebugTime;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (waveSettings == null || buoyancyPoints == null || buoyancyPoints.Length == 0)
            return;

        ApplyBuoyancy();
        ApplyUprightStability();
    }
    void ApplyBuoyancy()
    {
        float time = Time.time;

        int validPointCount = 0;

        foreach (Transform point in buoyancyPoints)
        {
            if (point != null)
                validPointCount++;
        }

        if (validPointCount == 0)
            return;

        float pointFloatStrength = floatStrength / validPointCount;

        bool shouldDebug = debugBuoyancy && Time.time >= nextDebugTime;

        if (shouldDebug)
        {
            nextDebugTime = Time.time + debugInterval;

            Debug.Log(
                $"[BoatBuoyancy] Valid Points: {validPointCount} | " +
                $"Point Float Strength: {pointFloatStrength:F2} | " +
                $"Boat Velocity: {rb.linearVelocity}"
            );
        }

        int submergedCount = 0;

        foreach (Transform point in buoyancyPoints)
        {
            if (point == null)
                continue;

            Vector3 pointPosition = point.position;

            Vector2 worldXZ = new Vector2(
                pointPosition.x,
                pointPosition.z
            );

            float waterY =
                WaterWaveMath.EvaluateSurfaceY(
                    worldXZ,
                    waveSettings,
                    time
                )
                + waterLevelOffset;

            float submergeAmount =
                waterY - pointPosition.y;

            if (submergeAmount <= 0f)
            {
                if (shouldDebug)
                {
                    Debug.Log(
                        $"[BoatBuoyancy] {point.name} ABOVE WATER | " +
                        $"Point Y: {pointPosition.y:F2} | " +
                        $"Water Y: {waterY:F2} | " +
                        $"Submerge Amount: {submergeAmount:F2}"
                    );
                }

                continue;
            }

            submergedCount++;

            float submerge01 = Mathf.Clamp01(
                submergeAmount / sinkDepth
            );

            Vector3 pointVelocity = rb.GetPointVelocity(pointPosition);

            float upwardMagnitude = submerge01 * pointFloatStrength;
            Vector3 upwardForce = Vector3.up * upwardMagnitude;

            float verticalVelocity = pointVelocity.y;
            float damping = -verticalVelocity * submerge01 * waterDrag;
            float maxDamping = pointFloatStrength * 2f;
            damping = Mathf.Clamp(damping, -maxDamping, maxDamping);
            Vector3 dampingForce = Vector3.up * damping;

            Vector3 horizontalVel = new Vector3(pointVelocity.x, 0f, pointVelocity.z);
            Vector3 horizontalDampForce = -horizontalVel * submerge01 * horizontalDrag;
            float maxHorizDamp = pointFloatStrength * 3f;
            horizontalDampForce = Vector3.ClampMagnitude(horizontalDampForce, maxHorizDamp);

            Vector3 totalForce = upwardForce + dampingForce + horizontalDampForce;

            if (shouldDebug)
            {
                Debug.Log(
                    $"[BoatBuoyancy] {point.name} | " +
                    $"Point Pos: {pointPosition} | " +
                    $"Water Y: {waterY:F2} | " +
                    $"Point Y: {pointPosition.y:F2} | " +
                    $"Submerge Amount: {submergeAmount:F2} | " +
                    $"Submerge01: {submerge01:F2}"
                );

                Debug.Log(
                    $"[BoatBuoyancy] {point.name} Forces | " +
                    $"Upward: {upwardForce} | " +
                    $"Damping: {dampingForce} | " +
                    $"HorizDrag: {horizontalDampForce} | " +
                    $"Total: {totalForce}"
                );
            }

            rb.AddForceAtPosition(
                totalForce,
                pointPosition,
                ForceMode.Acceleration
            );
        }
    }
    void ApplyUprightStability()
    {
        Vector3 predictedUp = rb.rotation * Vector3.up;
        Vector3 torqueAxis = Vector3.Cross(predictedUp, Vector3.up);

        rb.AddTorque(
            torqueAxis * uprightStrength - rb.angularVelocity * uprightDamping,
            ForceMode.Acceleration
        );
    }
}

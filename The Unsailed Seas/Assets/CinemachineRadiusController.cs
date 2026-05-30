using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineRadiusController : MonoBehaviour
{
    CinemachineOrbitalFollow orbitRig;

    [SerializeField] float minRadius = 2f;
    [SerializeField] float maxRadius = 20f;
    [SerializeField] float radiusChanceSpeed = 0.1f;


    private void Awake()
    {
        orbitRig = gameObject.GetComponent<CinemachineOrbitalFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        if (orbitRig == null) return;

        //use scrollweel to zoom in and out
        orbitRig.Radius = Mathf.Clamp(orbitRig.Radius + (-Input.mouseScrollDelta.y) * radiusChanceSpeed, minRadius, maxRadius);
    }
}

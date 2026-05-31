using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
public class XMarksTheSpot : MonoBehaviour
{
    [Header("Target Search")]
    public string targetTag = "Target";

    [Header("World Area")]
    public Vector3 worldBoxCenter = Vector3.zero;
    public Vector3 worldBoxSize = new Vector3(100f, 20f, 100f);

    [Header("UI Area (Parent RectTransform Local Space)")]
    public Vector2 uiBoxCenter = Vector2.zero;
    public Vector2 uiBoxSize = new Vector2(200f, 200f);

    private RectTransform markerRect;

    private void Awake()
    {
        markerRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        GameObject target = FindTarget();

        if (target == null)
            return;

        UpdateMarkerPosition(target.transform.position);
    }

    private GameObject FindTarget()
    {
        Bounds bounds = new Bounds(worldBoxCenter, worldBoxSize);

        GameObject[] objects = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject obj in objects)
        {
            if (bounds.Contains(obj.transform.position))
                return obj;
        }

        return null;
    }

    private void UpdateMarkerPosition(Vector3 worldPosition)
    {
        // Normalize world position within the world box
        float normalizedX = Mathf.InverseLerp(
            worldBoxCenter.x - worldBoxSize.x * 0.5f,
            worldBoxCenter.x + worldBoxSize.x * 0.5f,
            worldPosition.x);

        float normalizedZ = Mathf.InverseLerp(
            worldBoxCenter.z - worldBoxSize.z * 0.5f,
            worldBoxCenter.z + worldBoxSize.z * 0.5f,
            worldPosition.z);

        // Convert to parent-local UI coordinates
        Vector2 uiPosition = new Vector2(
            uiBoxCenter.x + ((normalizedX - 0.5f) * uiBoxSize.x),
            uiBoxCenter.y + ((normalizedZ - 0.5f) * uiBoxSize.y)
        );

        markerRect.anchoredPosition = uiPosition;
    }

    private void OnDrawGizmosSelected()
    {
        // -------------------------
        // World-space box
        // -------------------------
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(worldBoxCenter, worldBoxSize);

        // -------------------------
        // UI-space box
        // -------------------------
        RectTransform parentRect = transform.parent as RectTransform;

        if (parentRect == null)
            return;

        Vector2 half = uiBoxSize * 0.5f;

        Vector3 bl = parentRect.TransformPoint(
            uiBoxCenter + new Vector2(-half.x, -half.y));

        Vector3 br = parentRect.TransformPoint(
            uiBoxCenter + new Vector2(half.x, -half.y));

        Vector3 tr = parentRect.TransformPoint(
            uiBoxCenter + new Vector2(half.x, half.y));

        Vector3 tl = parentRect.TransformPoint(
            uiBoxCenter + new Vector2(-half.x, half.y));

#if UNITY_EDITOR
        Handles.color = Color.red;
        Handles.DrawAAPolyLine(3f, bl, br, tr, tl, bl);

        // Center marker
        Vector3 center = parentRect.TransformPoint(uiBoxCenter);

        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(
            2f,
            center + Vector3.left * 10f,
            center + Vector3.right * 10f);

        Handles.DrawAAPolyLine(
            2f,
            center + Vector3.up * 10f,
            center + Vector3.down * 10f);
#endif
    }
}
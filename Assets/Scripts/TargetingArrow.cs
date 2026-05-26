using UnityEngine;

public class TargetingArrow : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public int segments = 20;

    private void Awake() => lineRenderer = GetComponent<LineRenderer>();

    public void Show(bool active)
    { 
        lineRenderer.enabled = active;
    }

    public void UpdateArrow(Vector3 start, Vector3 end)
    {
        Vector3 control = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 2.0f;
        lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            lineRenderer.SetPosition(i, CalculateBezierPoint(t, start, control, end));
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }
}
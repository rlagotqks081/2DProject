using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TargetingArrow : MonoBehaviour
{
    public GameObject arrowSegmentPrefab;
    public int segmentCount = 20;
    private List<Image> segments = new List<Image>();
    [SerializeField] public Canvas canvas;
    [SerializeField] public RectTransform canvasRect;

    private void Awake()
    {
        // 1. 오브젝트 풀링: 화살표 조각 미리 생성
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject obj = Instantiate(arrowSegmentPrefab, transform);
            obj.GetComponent<Image>().raycastTarget = false; // 마우스 입력 방해 금지
            segments.Add(obj.GetComponent<Image>());
        }
    }
    public void Show(bool isVisible)
    {
        foreach (var segment in segments)
        {
            segment.gameObject.SetActive(isVisible);
        }
    }

    public void UpdateCurve(Vector3 start, Vector3 end)
    {

        Vector2 startLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, start),
            null,
            out startLocalPos);

        // 2. 마우스 좌표를 캔버스 기준 로컬 좌표로 변환
        Vector2 endLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            end,
            null,
            out endLocalPos);

        Vector3 control = Vector3.Lerp(startLocalPos, endLocalPos, 0.5f) + Vector3.up * 300f;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 pos = CalculateBezierPoint(t, startLocalPos, control, endLocalPos);

            segments[i].rectTransform.anchoredPosition = pos;

            if (i > 0)
            {
                Vector3 dir = (Vector2)pos - segments[i - 1].rectTransform.anchoredPosition;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                segments[i].rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }
}
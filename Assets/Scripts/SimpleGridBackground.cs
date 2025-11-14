using UnityEngine;

public class SimpleGridBackground : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("그리드 셀 크기 (한 칸)")]
    public float gridSize = 1f;
    
    [Tooltip("그리드 범위")]
    public float gridRange = 100f;
    
    [Tooltip("그리드 선 색상")]
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    
    [Tooltip("그리드 선 두께")]
    public float lineWidth = 0.05f;
    
    [Tooltip("5칸마다 진한 선")]
    public bool useMajorGrid = true;
    
    [Tooltip("진한 선 색상")]
    public Color majorGridColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    [Tooltip("진한 선 두께")]
    public float majorLineWidth = 0.08f;
    
    [Tooltip("배경 순서 (Sorting Order)")]
    public int sortingOrder = -100;

    void Start()
    {
        CreateGridLines();
    }

    void CreateGridLines()
    {
        GameObject gridParent = new GameObject("GridLines");
        gridParent.transform.SetParent(transform);
        gridParent.transform.localPosition = Vector3.zero;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        
        int lineCount = Mathf.CeilToInt(gridRange / gridSize);
        
        // 수직선 (세로선)
        for (int i = -lineCount; i <= lineCount; i++)
        {
            float x = i * gridSize;
            bool isMajor = useMajorGrid && (i % 5 == 0);
            
            GameObject lineObj = new GameObject($"VerticalLine_{i}");
            lineObj.transform.SetParent(gridParent.transform);
            lineObj.transform.position = Vector3.zero;
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startColor = isMajor ? majorGridColor : gridColor;
            lr.endColor = isMajor ? majorGridColor : gridColor;
            lr.startWidth = isMajor ? majorLineWidth : lineWidth;
            lr.endWidth = isMajor ? majorLineWidth : lineWidth;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(x, -gridRange, 0));
            lr.SetPosition(1, new Vector3(x, gridRange, 0));
            lr.sortingOrder = sortingOrder;
            lr.useWorldSpace = true;
        }
        
        // 수평선 (가로선)
        for (int i = -lineCount; i <= lineCount; i++)
        {
            float y = i * gridSize;
            bool isMajor = useMajorGrid && (i % 5 == 0);
            
            GameObject lineObj = new GameObject($"HorizontalLine_{i}");
            lineObj.transform.SetParent(gridParent.transform);
            lineObj.transform.position = Vector3.zero;
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startColor = isMajor ? majorGridColor : gridColor;
            lr.endColor = isMajor ? majorGridColor : gridColor;
            lr.startWidth = isMajor ? majorLineWidth : lineWidth;
            lr.endWidth = isMajor ? majorLineWidth : lineWidth;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(-gridRange, y, 0));
            lr.SetPosition(1, new Vector3(gridRange, y, 0));
            lr.sortingOrder = sortingOrder;
            lr.useWorldSpace = true;
        }
        
        Debug.Log($"모눈종이 그리드 생성 완료: {gridRange * 2}x{gridRange * 2} 범위, 칸 크기: {gridSize}");
    }
}


using UnityEngine;

public class GridBackground : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("그리드 셀 크기")]
    public float gridSize = 1f;
    
    [Tooltip("그리드 선 색상")]
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    
    [Tooltip("그리드 선 두께")]
    public float lineWidth = 0.02f;
    
    [Tooltip("메인 그리드 간격 (진한 선)")]
    public int majorGridEvery = 5;
    
    [Tooltip("메인 그리드 색상")]
    public Color majorGridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Tooltip("메인 그리드 선 두께")]
    public float majorLineWidth = 0.04f;
    
    [Header("Grid Range")]
    [Tooltip("그리드 표시 범위 (카메라 기준)")]
    public float gridExtent = 50f;
    
    private Material lineMaterial;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        CreateLineMaterial();
    }

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity의 기본 라인 그리기용 셰이더 사용
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        if (cam == null) return;
        
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);

        Vector3 camPos = cam.transform.position;
        
        // 카메라 위치 기준으로 그리드 범위 계산
        float startX = Mathf.Floor((camPos.x - gridExtent) / gridSize) * gridSize;
        float endX = Mathf.Ceil((camPos.x + gridExtent) / gridSize) * gridSize;
        float startY = Mathf.Floor((camPos.y - gridExtent) / gridSize) * gridSize;
        float endY = Mathf.Ceil((camPos.y + gridExtent) / gridSize) * gridSize;

        // 수직선 그리기
        for (float x = startX; x <= endX; x += gridSize)
        {
            int gridIndex = Mathf.RoundToInt(x / gridSize);
            bool isMajor = (gridIndex % majorGridEvery) == 0;
            
            GL.Color(isMajor ? majorGridColor : gridColor);
            float width = isMajor ? majorLineWidth : lineWidth;
            
            DrawThickLine(
                new Vector3(x, startY, 0),
                new Vector3(x, endY, 0),
                width
            );
        }

        // 수평선 그리기
        for (float y = startY; y <= endY; y += gridSize)
        {
            int gridIndex = Mathf.RoundToInt(y / gridSize);
            bool isMajor = (gridIndex % majorGridEvery) == 0;
            
            GL.Color(isMajor ? majorGridColor : gridColor);
            float width = isMajor ? majorLineWidth : lineWidth;
            
            DrawThickLine(
                new Vector3(startX, y, 0),
                new Vector3(endX, y, 0),
                width
            );
        }

        GL.End();
        GL.PopMatrix();
    }

    void DrawThickLine(Vector3 start, Vector3 end, float width)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * width * 0.5f;

        // 두꺼운 선을 위해 사각형으로 그리기
        GL.Vertex(start - perpendicular);
        GL.Vertex(start + perpendicular);
        
        GL.Vertex(start + perpendicular);
        GL.Vertex(end + perpendicular);
        
        GL.Vertex(end + perpendicular);
        GL.Vertex(end - perpendicular);
        
        GL.Vertex(end - perpendicular);
        GL.Vertex(start - perpendicular);
    }
}


using UnityEngine;

public class MouseHoverHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("테두리 색상")]
    public Color outlineColor = Color.yellow;
    
    [Tooltip("테두리 두께 (스케일 배율)")]
    public float outlineScale = 1.15f;
    
    [Tooltip("테두리 알파값")]
    public float outlineAlpha = 0.8f;
    
    private SpriteRenderer spriteRenderer;
    private GameObject outlineObject;
    private SpriteRenderer outlineRenderer;
    private bool isHovering = false;
    private Camera mainCamera;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            CreateOutlineObject();
        }
    }

    void CreateOutlineObject()
    {
        // 테두리용 오브젝트 생성
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * outlineScale;
        
        // 스프라이트 렌더러 추가
        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = spriteRenderer.sprite;
        outlineRenderer.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineAlpha);
        outlineRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // 원본보다 뒤에
        
        // 처음엔 숨김
        outlineObject.SetActive(false);
    }

    void Update()
    {
        if (mainCamera == null) return;
        
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        
        // 콜라이더가 있는지 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 마우스가 콜라이더 위에 있는지 확인
            bool nowHovering = col.OverlapPoint(mouseWorldPos);
            
            if (nowHovering != isHovering)
            {
                isHovering = nowHovering;
                UpdateHighlight();
            }
        }
        else
        {
            // 콜라이더가 없으면 거리 기반으로 체크
            float distance = Vector2.Distance(transform.position, mouseWorldPos);
            bool nowHovering = distance < 0.5f;
            
            if (nowHovering != isHovering)
            {
                isHovering = nowHovering;
                UpdateHighlight();
            }
        }
    }

    void UpdateHighlight()
    {
        if (outlineObject == null) return;
        
        // 아웃라인 오브젝트 표시/숨김
        outlineObject.SetActive(isHovering);
        
        // 아웃라인 스프라이트가 원본과 동일한지 확인하고 업데이트
        if (isHovering && outlineRenderer != null && spriteRenderer != null)
        {
            if (outlineRenderer.sprite != spriteRenderer.sprite)
            {
                outlineRenderer.sprite = spriteRenderer.sprite;
            }
        }
    }
    
    void OnDestroy()
    {
        // 아웃라인 오브젝트 정리
        if (outlineObject != null)
        {
            Destroy(outlineObject);
        }
    }

    void OnMouseEnter()
    {
        isHovering = true;
        UpdateHighlight();
    }

    void OnMouseExit()
    {
        isHovering = false;
        UpdateHighlight();
    }
}


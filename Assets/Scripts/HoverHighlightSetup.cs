using UnityEngine;

[ExecuteInEditMode]
public class HoverHighlightSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("시작 시 자동으로 하이라이트 설정")]
    public bool autoSetupOnStart = true;
    
    [Tooltip("Collider 크기 (0이면 자동)")]
    public float colliderRadius = 0.5f;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupHighlight();
        }
    }

    [ContextMenu("Setup Hover Highlight")]
    public void SetupHighlight()
    {
        // CircleCollider2D 추가 (없으면)
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true; // 물리 충돌은 하지 않음
            
            if (colliderRadius > 0)
            {
                col.radius = colliderRadius;
            }
            
            Debug.Log($"[HoverHighlightSetup] {gameObject.name}에 CircleCollider2D 추가됨");
        }
        
        // MouseHoverHighlight 추가 (없으면)
        MouseHoverHighlight highlight = GetComponent<MouseHoverHighlight>();
        if (highlight == null)
        {
            highlight = gameObject.AddComponent<MouseHoverHighlight>();
            Debug.Log($"[HoverHighlightSetup] {gameObject.name}에 MouseHoverHighlight 추가됨");
        }
        
        // Follower인 경우 FollowerDragSwap도 추가
        Follower followerComponent = GetComponent<Follower>();
        if (followerComponent != null)
        {
            FollowerDragSwap dragSwap = GetComponent<FollowerDragSwap>();
            if (dragSwap == null)
            {
                dragSwap = gameObject.AddComponent<FollowerDragSwap>();
                Debug.Log($"[HoverHighlightSetup] {gameObject.name}에 FollowerDragSwap 추가됨 (드래그로 순서 변경 가능)");
            }
        }
    }
}


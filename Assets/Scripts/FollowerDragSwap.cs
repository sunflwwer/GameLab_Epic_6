using UnityEngine;

public class FollowerDragSwap : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("드래그 중 투명도")]
    public float dragAlpha = 0.7f;
    
    [Tooltip("드래그 중 스케일")]
    public float dragScale = 1.2f;
    
    private bool isDragging = false;
    private Vector3 dragOffset;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private int originalSortingOrder;
    private Camera mainCamera;
    private Follower followerComponent;
    private static FollowerDragSwap currentlyDragging = null;
    private static bool wasSpaceFrozen = false; // 스페이스바로 정지된 상태 추적

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        followerComponent = GetComponent<Follower>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
        
        originalScale = transform.localScale;
        
        // Collider가 없으면 자동 추가 (마우스 이벤트를 위해 필요)
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.radius = 0.5f;
            circleCol.isTrigger = true;
            Debug.Log($"[FollowerDragSwap] {gameObject.name}에 CircleCollider2D 자동 추가");
        }
    }



    void OnMouseDown()
    {
        // Follower 컴포넌트가 있어야 함
        if (followerComponent == null)
        {
            Debug.Log("[FollowerDragSwap] Follower 컴포넌트가 없습니다.");
            return;
        }
        
        if (mainCamera == null) return;
        
        isDragging = true;
        currentlyDragging = this;
        
        // 마우스 위치와 오브젝트 위치의 오프셋 계산
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        dragOffset = transform.position - mousePos;
        
        // 드래그 시작 시각 효과
        if (spriteRenderer != null)
        {
            Color dragColor = originalColor;
            dragColor.a = dragAlpha;
            spriteRenderer.color = dragColor;
            spriteRenderer.sortingOrder = 100; // 최상위로
        }
        
        transform.localScale = originalScale * dragScale;
        
        // 스페이스바로 이미 정지된 상태인지 확인
        wasSpaceFrozen = Input.GetKey(KeyCode.Space);
        
        // 스페이스바로 정지되지 않은 상태라면 드래그로 정지
        if (!wasSpaceFrozen)
        {
            FreezeAllMovement(true);
        }
        
        Debug.Log($"[FollowerDragSwap] {gameObject.name} 드래그 시작! (스페이스바 정지 상태: {wasSpaceFrozen})");
    }

    void OnMouseDrag()
    {
        if (!isDragging || mainCamera == null) return;
        
        // 마우스 위치로 오브젝트 이동
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        transform.position = mousePos + dragOffset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        
        isDragging = false;
        currentlyDragging = null;
        
        // 원래 시각 상태로 복원
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
        
        transform.localScale = originalScale;
        
        // 가장 가까운 Follower와 순서 교환
        SwapWithNearestFollower();
        
        // 스페이스바로 정지되지 않은 상태였다면 재개
        if (!wasSpaceFrozen)
        {
            FreezeAllMovement(false);
        }
        
        Debug.Log($"[FollowerDragSwap] {gameObject.name} 드래그 종료! (스페이스바로 정지 유지: {wasSpaceFrozen})");
    }

    void SwapWithNearestFollower()
    {
        // 현재 리더 찾기
        GameObject leaderObj = GameObject.FindGameObjectWithTag("Player");
        if (leaderObj == null) 
        {
            RebuildChain();
            return;
        }
        
        // 모든 Follower 찾기
        Follower[] allFollowers = FindObjectsByType<Follower>(FindObjectsSortMode.None);
        
        // 리더로부터 거리순으로 정렬
        System.Collections.Generic.List<Follower> sortedFollowers = new System.Collections.Generic.List<Follower>();
        foreach (Follower f in allFollowers)
        {
            if (f != followerComponent)
            {
                sortedFollowers.Add(f);
            }
        }
        
        sortedFollowers.Sort((a, b) =>
        {
            float distA = Vector2.Distance(leaderObj.transform.position, a.transform.position);
            float distB = Vector2.Distance(leaderObj.transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
        
        // 가장 가까운 Follower 찾기
        Follower nearest = null;
        float minDistance = 5f;
        
        foreach (Follower other in sortedFollowers)
        {
            float distance = Vector2.Distance(transform.position, other.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = other;
            }
        }
        
        // 맨 마지막 Follower인지 확인 (마지막에 배치하기 쉽게)
        if (sortedFollowers.Count > 0)
        {
            Follower lastFollower = sortedFollowers[sortedFollowers.Count - 1];
            float distanceToLast = Vector2.Distance(transform.position, lastFollower.transform.position);
            
            // 마지막 Follower와의 거리가 8 유닛 이내면 맨 뒤에 배치
            if (distanceToLast < 8f && (nearest == null || lastFollower == nearest || distanceToLast < minDistance * 1.5f))
            {
                Debug.Log($"[FollowerDragSwap] {gameObject.name}을(를) 맨 뒤({lastFollower.gameObject.name} 뒤)에 배치");
                // 맨 마지막보다 더 뒤에 위치시키기
                Vector3 direction = (lastFollower.transform.position - leaderObj.transform.position).normalized;
                transform.position = lastFollower.transform.position + direction * 2f;
                
                RebuildChain();
                return;
            }
        }
        
        // 가까운 Follower가 있으면 위치 교환
        if (nearest != null)
        {
            Debug.Log($"[FollowerDragSwap] {gameObject.name}과(와) {nearest.gameObject.name} 위치 교환 (거리: {minDistance:F2})");
            
            // 두 Follower의 위치 교환
            Vector3 tempPos = transform.position;
            transform.position = nearest.transform.position;
            nearest.transform.position = tempPos;
        }
        else
        {
            Debug.Log($"[FollowerDragSwap] {gameObject.name} 근처에 교환할 Follower 없음");
        }
        
        // 항상 체인 재구성
        RebuildChain();
    }

    void RebuildChain()
    {
        // 모든 Follower 찾기
        Follower[] allFollowers = FindObjectsByType<Follower>(FindObjectsSortMode.None);
        
        // Player 태그를 가진 리더 찾기
        GameObject leaderObj = GameObject.FindGameObjectWithTag("Player");
        if (leaderObj == null) return;
        
        Follower leader = leaderObj.GetComponent<Follower>();
        
        // 리더를 제외한 나머지 Follower들
        System.Collections.Generic.List<Follower> otherFollowers = new System.Collections.Generic.List<Follower>();
        foreach (Follower f in allFollowers)
        {
            if (f != leader)
            {
                otherFollowers.Add(f);
            }
        }
        
        // 리더로부터의 거리로 정렬
        otherFollowers.Sort((a, b) =>
        {
            float distA = Vector2.Distance(leaderObj.transform.position, a.transform.position);
            float distB = Vector2.Distance(leaderObj.transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
        
        // 체인 재구성
        Transform currentTarget = leaderObj.transform;
        Follower previousFollower = leader;
        
        foreach (Follower follower in otherFollowers)
        {
            follower.target = currentTarget;
            
            if (previousFollower != null)
            {
                previousFollower.nextFollower = follower;
            }
            
            previousFollower = follower;
            currentTarget = follower.transform;
        }
        
        if (previousFollower != null)
        {
            previousFollower.nextFollower = null;
        }
        
        // 리더의 타겟은 null
        if (leader != null)
        {
            leader.target = null;
        }
        
        // 디버그: 재구성된 순서 출력
        string chainOrder = leader != null ? leader.gameObject.name : "None";
        Follower current = leader;
        while (current != null && current.nextFollower != null)
        {
            current = current.nextFollower;
            chainOrder += " → " + current.gameObject.name;
        }
        Debug.Log($"[FollowerDragSwap] 체인 재구성 완료: {chainOrder}");
    }


    void FreezeAllMovement(bool freeze)
    {
        // Player 태그를 가진 리더의 LeaderFollowMouse 정지/재개
        GameObject leader = GameObject.FindGameObjectWithTag("Player");
        if (leader != null)
        {
            LeaderFollowMouse leaderMovement = leader.GetComponent<LeaderFollowMouse>();
            if (leaderMovement != null)
            {
                leaderMovement.enabled = !freeze;
            }
        }
        
        // 모든 Follower 이동 정지/재개
        Follower[] allFollowers = FindObjectsByType<Follower>(FindObjectsSortMode.None);
        foreach (Follower follower in allFollowers)
        {
            // Player 태그를 가진 리더는 Follower 컴포넌트 비활성화 필요 없음
            if (!follower.CompareTag("Player"))
            {
                follower.enabled = !freeze;
            }
        }
        
        Debug.Log($"[FollowerDragSwap] 모든 이동 {(freeze ? "정지" : "재개")}");
    }

    void OnDrawGizmosSelected()
    {
        if (isDragging)
        {
            // 드래그 중인 Follower 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}


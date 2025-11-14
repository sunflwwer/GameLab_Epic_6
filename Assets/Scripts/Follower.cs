using UnityEngine;
using System.Collections.Generic;

public class Follower : MonoBehaviour
{
    public enum FollowerType
    {
        ShieldBearer,  // 방패용사
        Swordsman,     // 검사
        Archer,        // 궁수
        Healer         // 힐러
    }
    
    [Header("Follower Type")]
    [Tooltip("친구의 능력/타입")]
    public FollowerType followerType = FollowerType.ShieldBearer;
    
    [Header("Follow Settings")]
    [Tooltip("따라갈 타겟 (플레이어 또는 앞 친구)")]
    public Transform target;
    
    [Tooltip("타겟과의 거리")]
    public float followDistance = 2f;
    
    [Tooltip("부드러운 이동 속도 (값이 작을수록 빠름)")]
    public float smoothTime = 0.1f;
    
    [Tooltip("최대 이동 속도")]
    public float maxSpeed = 30f;
    
    [Header("Rotation Settings")]
    [Tooltip("이동 방향을 바라보게 할지")]
    public bool faceMovementDirection = true;
    
    [Tooltip("회전 속도")]
    public float rotationSpeed = 8f;
    
    [Header("Chain Settings")]
    [Tooltip("이 친구를 따라오는 다음 친구")]
    public Follower nextFollower;
    
    [Header("Shield Settings (방패용사 전용)")]
    [Tooltip("방패 크기")]
    public float shieldSize = 0.8f;
    
    [Tooltip("방패 색상")]
    public Color shieldColor = new Color(0.7f, 0.7f, 0.9f, 1f);
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastTargetPosition;
    private static List<Follower> allFollowers = new List<Follower>();
    private GameObject shieldObject;

    void Start()
    {
        // 리스트에 등록
        if (!allFollowers.Contains(this))
        {
            allFollowers.Add(this);
        }
        
        // 타겟이 설정되지 않았다면 자동으로 설정
        if (target == null)
        {
            AutoAssignTarget();
        }
        
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
        
        // 타입별 초기화
        InitializeByType();
    }

    void OnDestroy()
    {
        // 리스트에서 제거
        allFollowers.Remove(this);
        
        // 방패 제거
        if (shieldObject != null)
        {
            Destroy(shieldObject);
        }
    }

    void InitializeByType()
    {
        switch (followerType)
        {
            case FollowerType.ShieldBearer:
                CreateShield();
                break;
            case FollowerType.Swordsman:
                InitializeSwordsman();
                break;
            case FollowerType.Archer:
                InitializeArcher();
                break;
            case FollowerType.Healer:
                InitializeHealer();
                break;
        }
    }

    void InitializeSwordsman()
    {
        // 검사 공격 스크립트 추가
        gameObject.AddComponent<SwordsmanAttack>();
        Debug.Log($"[Follower] {gameObject.name}의 검사 능력 활성화!");
    }

    void InitializeArcher()
    {
        // 궁수 공격 스크립트 추가
        gameObject.AddComponent<ArcherAttack>();
        Debug.Log($"[Follower] {gameObject.name}의 궁수 능력 활성화!");
    }

    void InitializeHealer()
    {
        // 힐러 스크립트 추가
        gameObject.AddComponent<HealerSupport>();
        Debug.Log($"[Follower] {gameObject.name}의 힐러 능력 활성화!");
    }

    void CreateShield()
    {
        // 방패 오브젝트 생성
        shieldObject = new GameObject("Shield");
        shieldObject.transform.SetParent(transform);
        shieldObject.transform.localPosition = new Vector3(0, shieldSize / 2f, 0);
        
        // 스프라이트 렌더러 추가
        SpriteRenderer shieldRenderer = shieldObject.AddComponent<SpriteRenderer>();
        shieldRenderer.sprite = CreateShieldSprite();
        shieldRenderer.color = shieldColor;
        shieldRenderer.sortingOrder = 3;
        
        // 방패 콜라이더 추가 (적의 공격을 막기 위해)
        BoxCollider2D shieldCollider = shieldObject.AddComponent<BoxCollider2D>();
        shieldCollider.size = new Vector2(shieldSize * 1.5f, shieldSize * 0.5f);
        shieldCollider.isTrigger = true;
        
        // 방패 방어 스크립트 추가
        ShieldDefense shieldDefense = shieldObject.AddComponent<ShieldDefense>();
        shieldDefense.owner = gameObject;
        
        Debug.Log($"[Follower] {gameObject.name}의 방패 생성 완료!");
    }

    Sprite CreateShieldSprite()
    {
        // 가로로 긴 방패 텍스처 생성
        int width = (int)(shieldSize * 150);  // 가로를 더 길게
        int height = (int)(shieldSize * 50);  // 세로는 더 짧게
        
        Texture2D texture = new Texture2D(width, height);
        
        Color transparent = new Color(0, 0, 0, 0);
        
        // 배경을 투명하게
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, transparent);
            }
        }
        
        // 가로로 긴 방패 모양 그리기 (좌우가 둥근 직사각형)
        int centerY = height / 2;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 중앙 부분 (직사각형)
                if (x > width * 0.15f && x < width * 0.85f)
                {
                    if (y > height * 0.1f && y < height * 0.9f)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
                // 좌측 둥근 부분
                else if (x <= width * 0.15f)
                {
                    float normalizedX = x / (width * 0.15f);
                    int curveHeight = (int)(height * 0.4f * normalizedX);
                    if (Mathf.Abs(y - centerY) < curveHeight)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
                // 우측 둥근 부분
                else if (x >= width * 0.85f)
                {
                    float normalizedX = 1f - (x - width * 0.85f) / (width * 0.15f);
                    int curveHeight = (int)(height * 0.4f * normalizedX);
                    if (Mathf.Abs(y - centerY) < curveHeight)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    void AutoAssignTarget()
    {
        // 자신이 Player 태그를 가진 리더인지 확인
        if (CompareTag("Player"))
        {
            // 리더는 타겟이 필요 없음 (마우스를 따라감)
            target = null;
            Debug.Log($"[Follower] {gameObject.name}은(는) 리더입니다 (마우스 따라가기).");
            return;
        }
        
        // Player 태그를 가진 Follower(리더) 찾기
        Follower leader = null;
        foreach (Follower follower in allFollowers)
        {
            if (follower.CompareTag("Player"))
            {
                leader = follower;
                break;
            }
        }
        
        if (leader == null)
        {
            Debug.LogError("[Follower] Player 태그를 가진 리더를 찾을 수 없습니다!");
            return;
        }
        
        // 체인에서 마지막 Follower 찾기
        Follower lastInChain = leader;
        foreach (Follower follower in allFollowers)
        {
            if (follower == this || follower == leader) continue;
            
            // 체인의 끝 찾기
            if (follower.target != null && follower.nextFollower == null)
            {
                lastInChain = follower;
            }
        }
        
        if (lastInChain != null && lastInChain != this)
        {
            // 체인의 마지막을 따라가기
            target = lastInChain.transform;
            lastInChain.nextFollower = this;
            Debug.Log($"[Follower] {gameObject.name}이(가) {lastInChain.gameObject.name}을(를) 따라갑니다.");
        }
    }

    void Update()
    {
        // Player 태그를 가진 리더는 LeaderFollowMouse가 처리
        if (CompareTag("Player")) return;
        
        if (target == null) return;
        
        // 타겟의 이동 방향 계산
        Vector3 targetMoveDirection = (target.position - lastTargetPosition).normalized;
        lastTargetPosition = target.position;
        
        // 타겟 뒤쪽 위치 계산
        Vector3 followPosition = target.position;
        
        // 타겟이 움직이고 있다면 그 반대 방향으로
        if (targetMoveDirection.magnitude > 0.01f)
        {
            followPosition = target.position - targetMoveDirection * followDistance;
        }
        else
        {
            // 타겟이 정지해 있다면 현재 방향 유지
            Vector3 directionToFollower = (transform.position - target.position).normalized;
            if (directionToFollower.magnitude > 0.01f)
            {
                followPosition = target.position + directionToFollower * followDistance;
            }
        }
        
        // Z 위치 고정 (2D)
        followPosition.z = transform.position.z;
        
        // 현재 거리 계산
        float currentDistance = Vector2.Distance(transform.position, target.position);
        
        // 거리가 일정 수준 이상일 때만 이동
        if (currentDistance > followDistance + 0.5f)
        {
            // 거리에 따른 동적 속도 조절
            float distanceRatio = Mathf.Clamp01((currentDistance - followDistance) / followDistance);
            float dynamicSmoothTime = Mathf.Lerp(smoothTime, smoothTime * 0.3f, distanceRatio);
            float dynamicMaxSpeed = Mathf.Lerp(maxSpeed, maxSpeed * 2f, distanceRatio);
            
            // 부드럽게 따라가기 (거리가 멀수록 빠르게)
            Vector3 newPosition = Vector3.SmoothDamp(
                transform.position,
                followPosition,
                ref velocity,
                dynamicSmoothTime,
                dynamicMaxSpeed
            );
            newPosition.z = transform.position.z;
            transform.position = newPosition;
            
            // 이동 방향으로 회전 (2D)
            if (faceMovementDirection && velocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        else if (currentDistance < followDistance - 0.5f)
        {
            // 너무 가까우면 약간 멀어지기
            Vector3 awayDirection = (transform.position - target.position).normalized;
            Vector3 newPosition = transform.position + awayDirection * Time.deltaTime * 2f;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // 타겟과의 연결선 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            
            // 추적 거리 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, followDistance);
        }
    }
}

// 방패 방어 스크립트
public class ShieldDefense : MonoBehaviour
{
    public GameObject owner;
    
    [Header("Flash Effect")]
    [Tooltip("방어 시 플래시 색상")]
    public Color flashColor = Color.white;
    
    [Tooltip("플래시 지속 시간")]
    public float flashDuration = 0.15f;
    
    private SpriteRenderer shieldRenderer;
    private Color originalColor;
    private bool isFlashing;
    
    void Start()
    {
        shieldRenderer = GetComponent<SpriteRenderer>();
        if (shieldRenderer != null)
        {
            originalColor = shieldRenderer.color;
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 적의 검 공격 막기
        if (collision.CompareTag("Enemy") || collision.gameObject.name.Contains("EnemySword"))
        {
            Debug.Log($"[ShieldDefense] 방패가 적의 공격을 막았습니다! ({collision.gameObject.name})");
            
            // 플래시 효과
            if (shieldRenderer != null && !isFlashing)
            {
                StartCoroutine(FlashEffect());
            }
            
            // 적의 검이라면 비활성화 (공격 무효화)
            if (collision.gameObject.name.Contains("EnemySword"))
            {
                collision.gameObject.SetActive(false);
            }
        }
    }
    
    System.Collections.IEnumerator FlashEffect()
    {
        isFlashing = true;
        
        // 원래 색상 저장
        if (shieldRenderer != null)
        {
            originalColor = shieldRenderer.color;
            
            // 플래시 색상으로 변경
            shieldRenderer.color = flashColor;
            
            // 지속 시간만큼 대기
            yield return new UnityEngine.WaitForSeconds(flashDuration);
            
            // 원래 색상으로 복원
            shieldRenderer.color = originalColor;
        }
        
        isFlashing = false;
    }
}


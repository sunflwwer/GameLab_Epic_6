using UnityEngine;

public class ArcherAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("공격 범위 (반경) - 검사보다 넓음")]
    public float attackRange = 5f;
    
    [Tooltip("공격 데미지")]
    public int damage = 1;
    
    [Tooltip("공격 쿨타임 (초)")]
    public float attackCooldown = 0.7f;
    
    [Header("Bow Settings")]
    [Tooltip("활 길이 (세로)")]
    public float bowLength = 1f;
    
    [Tooltip("활 두께 (가로)")]
    public float bowWidth = 0.15f;
    
    [Tooltip("활 색상")]
    public Color bowColor = new Color(0.5f, 0.3f, 0.1f, 1f); // 갈색
    
    [Header("Arrow Settings")]
    [Tooltip("화살의 길이")]
    public float arrowLength = 1.2f;
    
    [Tooltip("화살의 두께 (얇음)")]
    public float arrowWidth = 0.08f;
    
    [Tooltip("화살 색상")]
    public Color arrowColor = new Color(0.6f, 0.4f, 0.2f, 1f); // 나무 색상
    
    [Tooltip("화살 속도")]
    public float arrowSpeed = 12f;
    
    [Tooltip("화살 최대 거리")]
    public float arrowMaxDistance = 6f;
    
    private float lastAttackTime;
    private GameObject bowObject;

    void Start()
    {
        CreateBow();
    }

    void CreateBow()
    {
        // 활 오브젝트 생성
        bowObject = new GameObject("Bow");
        bowObject.transform.SetParent(transform);
        bowObject.transform.localPosition = new Vector3(bowWidth / 2f, 0, 0); // 옆에 위치
        
        // 스프라이트 렌더러 추가
        SpriteRenderer bowRenderer = bowObject.AddComponent<SpriteRenderer>();
        bowRenderer.sprite = CreateBowSprite();
        bowRenderer.color = bowColor;
        bowRenderer.sortingOrder = 3;
        
        Debug.Log($"[ArcherAttack] {gameObject.name}의 활 생성 완료!");
    }

    Sprite CreateBowSprite()
    {
        // 세로로 긴 직사각형 활 스프라이트 생성
        int width = Mathf.Max(1, (int)(bowWidth * 100));
        int height = Mathf.Max(1, (int)(bowLength * 100));
        
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // 모든 픽셀을 흰색으로
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    void Update()
    {
        // 주변 적 찾기
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            FindAndAttackEnemy();
        }
    }

    void FindAndAttackEnemy()
    {
        // 범위 내 모든 적 찾기
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider2D col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.GetComponent<Enemy>();
                }
            }
        }
        
        // 가장 가까운 적 공격
        if (closestEnemy != null)
        {
            ShootArrow(closestEnemy);
        }
    }

    void ShootArrow(Enemy enemy)
    {
        lastAttackTime = Time.time;
        
        // 화살 오브젝트 생성
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.position = transform.position;
        
        // 스프라이트 렌더러 추가
        SpriteRenderer arrowRenderer = arrow.AddComponent<SpriteRenderer>();
        arrowRenderer.sprite = CreateArrowSprite();
        arrowRenderer.color = arrowColor;
        arrowRenderer.sortingOrder = 5;
        
        // 예측 조준: 적의 이동을 고려한 위치 계산
        Vector2 targetPosition = PredictTargetPosition(enemy);
        
        // 예측된 위치로 방향 설정
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        
        // 콜라이더 추가
        BoxCollider2D arrowCollider = arrow.AddComponent<BoxCollider2D>();
        arrowCollider.size = new Vector2(arrowWidth, arrowLength);
        arrowCollider.isTrigger = true;
        
        // 화살 이동 스크립트 추가
        ArrowProjectile projectile = arrow.AddComponent<ArrowProjectile>();
        projectile.direction = direction;
        projectile.speed = arrowSpeed;
        projectile.maxDistance = arrowMaxDistance;
        projectile.damage = damage;
        projectile.owner = gameObject;
        
        Debug.Log($"[ArcherAttack] {gameObject.name}이(가) 화살 발사!");
    }

    Vector2 PredictTargetPosition(Enemy enemy)
    {
        // 적의 현재 위치
        Vector2 enemyPos = enemy.transform.position;
        
        // 적까지의 거리 계산
        float distance = Vector2.Distance(transform.position, enemyPos);
        
        // 화살이 도달하는데 걸리는 시간
        float timeToReach = distance / arrowSpeed;
        
        // 적의 Rigidbody2D가 있으면 속도 가져오기
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        Vector2 enemyVelocity = Vector2.zero;
        
        if (enemyRb != null)
        {
            enemyVelocity = enemyRb.linearVelocity;
        }
        else
        {
            // Rigidbody가 없으면 이전 프레임과의 위치 차이로 속도 추정
            // (Enemy 스크립트는 transform.position을 직접 수정하므로)
            // 일반적인 적의 이동 속도를 가정 (Enemy.moveSpeed 기본값 2)
            Vector2 directionToPlayer = Vector2.zero;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                directionToPlayer = (player.transform.position - enemy.transform.position).normalized;
                enemyVelocity = directionToPlayer * 2f; // 적의 평균 이동 속도
            }
        }
        
        // 예측된 위치 = 현재 위치 + (속도 * 시간)
        Vector2 predictedPosition = enemyPos + enemyVelocity * timeToReach;
        
        return predictedPosition;
    }

    Sprite CreateArrowSprite()
    {
        // 얇은 화살 스프라이트 생성
        int width = Mathf.Max(1, (int)(arrowWidth * 100));
        int height = Mathf.Max(1, (int)(arrowLength * 100));
        
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 100f);
    }

    void OnDrawGizmosSelected()
    {
        // 공격 범위 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

// 화살 발사체 스크립트
public class ArrowProjectile : MonoBehaviour
{
    public Vector2 direction;
    public float speed = 8f;
    public float maxDistance = 6f;
    public int damage = 1;
    public GameObject owner;
    
    private Vector3 startPosition;
    private bool hasHit;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (hasHit) return;
        
        // 화살 이동
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        
        // 최대 거리 도달 시 제거
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        
        // 적에게 맞음
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"화살이 적에게 {damage} 데미지!");
            }
            
            hasHit = true;
            Destroy(gameObject);
        }
    }
}


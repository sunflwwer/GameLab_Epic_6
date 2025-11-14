using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("공격 범위 (반경)")]
    public float attackRange = 2f;
    
    [Tooltip("공격 데미지")]
    public int damage = 1;
    
    [Tooltip("공격 쿨타임 (초)")]
    public float attackCooldown = 1f;
    
    [Header("Sword Settings")]
    [Tooltip("검의 길이")]
    public float swordLength = 0.8f;
    
    [Tooltip("검의 두께")]
    public float swordWidth = 0.15f;
    
    [Tooltip("검 색상 (진한 빨간색)")]
    public Color swordColor = new Color(0.8f, 0f, 0f, 1f);
    
    [Tooltip("공격 지속 시간")]
    public float attackDuration = 0.3f;
    
    private float lastAttackTime;
    private Transform player;
    private GameObject swordObject;
    private SpriteRenderer swordRenderer;
    private bool isAttacking;
    private float attackStartTime;
    private Vector3 targetPosition;

    void Start()
    {
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // 검이 이미 생성되어 있는지 확인
        if (swordObject == null)
        {
            CreateSword();
        }
    }

    void CreateSword()
    {
        // 이미 자식으로 EnemySword가 있는지 확인
        Transform existingSword = transform.Find("EnemySword");
        if (existingSword != null)
        {
            Debug.LogWarning($"[EnemyAttack] {gameObject.name}에 이미 EnemySword가 있습니다. 재사용합니다.");
            swordObject = existingSword.gameObject;
            swordRenderer = swordObject.GetComponent<SpriteRenderer>();
            return;
        }
        
        swordObject = new GameObject("EnemySword");
        swordObject.transform.SetParent(transform);
        swordObject.transform.localPosition = Vector3.zero;
        
        swordRenderer = swordObject.AddComponent<SpriteRenderer>();
        swordRenderer.sprite = CreateSwordSprite();
        swordRenderer.color = swordColor;
        swordRenderer.sortingOrder = 5;
        
        BoxCollider2D swordCollider = swordObject.AddComponent<BoxCollider2D>();
        swordCollider.size = new Vector2(swordWidth, swordLength);
        swordCollider.isTrigger = true;
        
        EnemySwordDamage swordDamage = swordObject.AddComponent<EnemySwordDamage>();
        swordDamage.damage = damage;
        swordDamage.owner = gameObject;
        
        swordObject.SetActive(false);
    }

    Sprite CreateSwordSprite()
    {
        int width = Mathf.Max(1, (int)(swordWidth * 100));
        int height = Mathf.Max(1, (int)(swordLength * 100));
        
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

    void Update()
    {
        if (player == null) return;
        
        if (isAttacking)
        {
            UpdateSwordAttack();
        }
        else
        {
            // 플레이어가 범위 안에 있는지 확인
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    void UpdateSwordAttack()
    {
        float elapsed = Time.time - attackStartTime;
        float progress = elapsed / attackDuration;
        
        if (progress < 0.5f)
        {
            // 전진
            float t = progress * 2f;
            swordObject.transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        }
        else if (progress < 1f)
        {
            // 후퇴
            float t = (progress - 0.5f) * 2f;
            swordObject.transform.position = Vector3.Lerp(targetPosition, transform.position, t);
        }
        else
        {
            // 공격 종료
            isAttacking = false;
            swordObject.SetActive(false);
        }
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        attackStartTime = Time.time;
        
        // 플레이어 방향 계산
        Vector2 direction = (player.position - transform.position).normalized;
        
        // 검 활성화
        swordObject.SetActive(true);
        
        // 검 회전 (플레이어를 향하도록)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        swordObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        
        // 목표 위치 (플레이어 방향으로 검 길이만큼)
        targetPosition = transform.position + (Vector3)direction * swordLength;
        
        Debug.Log($"적이 플레이어를 향해 검 공격!");
    }

    void OnDrawGizmosSelected()
    {
        // 공격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

// 적 검 데미지 처리 스크립트
public class EnemySwordDamage : MonoBehaviour
{
    public int damage = 1;
    public GameObject owner;
    
    private System.Collections.Generic.HashSet<GameObject> hitTargets = new System.Collections.Generic.HashSet<GameObject>();

    void OnEnable()
    {
        // 검이 활성화될 때마다 히트 목록 초기화
        hitTargets.Clear();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hitTargets.Contains(collision.gameObject))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hitTargets.Add(collision.gameObject);
                Debug.Log($"적 검이 플레이어에게 {damage} 데미지!");
            }
        }
    }
}



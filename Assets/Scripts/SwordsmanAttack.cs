using UnityEngine;

public class SwordsmanAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("공격 범위 (반경)")]
    public float attackRange = 3f;
    
    [Tooltip("공격 데미지")]
    public int damage = 1;
    
    [Tooltip("공격 쿨타임 (초)")]
    public float attackCooldown = 0.5f;
    
    [Header("Sword Settings")]
    [Tooltip("검의 길이")]
    public float swordLength = 1f;
    
    [Tooltip("검의 두께")]
    public float swordWidth = 0.2f;
    
    [Tooltip("검 색상")]
    public Color swordColor = new Color(0.9f, 0.9f, 0.7f, 1f); // 금색 계열
    
    [Tooltip("공격 지속 시간")]
    public float attackDuration = 0.2f;
    
    private float lastAttackTime;
    private GameObject swordObject;
    private SpriteRenderer swordRenderer;
    private bool isAttacking;
    private float attackStartTime;
    private Vector3 targetPosition;

    void Start()
    {
        // 검이 이미 생성되어 있는지 확인
        if (swordObject == null)
        {
            CreateSword();
        }
    }

    void CreateSword()
    {
        // 이미 자식으로 SwordsmanSword가 있는지 확인
        Transform existingSword = transform.Find("SwordsmanSword");
        if (existingSword != null)
        {
            Debug.LogWarning($"[SwordsmanAttack] {gameObject.name}에 이미 SwordsmanSword가 있습니다. 재사용합니다.");
            swordObject = existingSword.gameObject;
            swordRenderer = swordObject.GetComponent<SpriteRenderer>();
            return;
        }
        
        swordObject = new GameObject("SwordsmanSword");
        swordObject.transform.SetParent(transform);
        swordObject.transform.localPosition = Vector3.zero;
        
        swordRenderer = swordObject.AddComponent<SpriteRenderer>();
        swordRenderer.sprite = CreateSwordSprite();
        swordRenderer.color = swordColor;
        swordRenderer.sortingOrder = 5;
        
        BoxCollider2D swordCollider = swordObject.AddComponent<BoxCollider2D>();
        swordCollider.size = new Vector2(swordWidth, swordLength);
        swordCollider.isTrigger = true;
        
        SwordsmanSwordDamage swordDamage = swordObject.AddComponent<SwordsmanSwordDamage>();
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
        if (isAttacking)
        {
            UpdateSwordAttack();
        }
        else
        {
            // 주변 적 찾기
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                FindAndAttackEnemy();
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
            AttackEnemy(closestEnemy);
        }
    }

    void AttackEnemy(Enemy enemy)
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        attackStartTime = Time.time;
        
        // 적 방향 계산
        Vector2 direction = (enemy.transform.position - transform.position).normalized;
        
        // 검 활성화
        swordObject.SetActive(true);
        
        // 검 회전 (적을 향하도록)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        swordObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        
        // 목표 위치 (적 방향으로 검 길이만큼)
        targetPosition = transform.position + (Vector3)direction * swordLength;
        
        Debug.Log($"[SwordsmanAttack] {gameObject.name}이(가) 적을 향해 검 공격!");
    }

    void OnDrawGizmosSelected()
    {
        // 공격 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

// 검사 검 데미지 처리 스크립트
public class SwordsmanSwordDamage : MonoBehaviour
{
    public int damage = 1;
    public GameObject owner;
    
    private System.Collections.Generic.HashSet<GameObject> hitEnemies = new System.Collections.Generic.HashSet<GameObject>();

    void OnEnable()
    {
        // 검이 활성화될 때마다 히트 목록 초기화
        hitEnemies.Clear();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !hitEnemies.Contains(collision.gameObject))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitEnemies.Add(collision.gameObject);
                Debug.Log($"검사의 검이 적에게 {damage} 데미지!");
            }
        }
    }
}


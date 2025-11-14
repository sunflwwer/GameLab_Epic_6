﻿using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("적의 이동 속도")]
    public float moveSpeed = 2f;
    
    [Tooltip("플레이어 추적 시작 거리")]
    public float chaseRange = 10f;
    
    [Header("Combat Settings")]
    [Tooltip("적의 최대 체력")]
    public int maxHealth = 3;
    
    [Tooltip("적의 현재 체력")]
    public int health = 3;
    
    [Header("Visual Settings")]
    [Tooltip("추적 중일 때 색상")]
    public Color chaseColor = Color.red;
    
    [Tooltip("대기 중일 때 색상 (연한 빨간색)")]
    public Color idleColor = new Color(1f, 0.7f, 0.7f, 1f); // 연한 빨간색
    
    [Header("Damage Effect")]
    [Tooltip("피격 시 깜빡임 지속 시간")]
    public float blinkDuration = 0.3f;
    
    [Tooltip("깜빡임 속도")]
    public float blinkSpeed = 10f;
    
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private bool isChasing = false;
    private float lastDamageTime = -999f;
    private Color originalColor;

    void Start()
    {
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("플레이어를 찾을 수 없습니다! 플레이어에 'Player' 태그를 설정하세요.");
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 체력 초기화
        health = maxHealth;
    }

    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 플레이어 추적
        if (distanceToPlayer <= chaseRange)
        {
            isChasing = true;
            
            // 플레이어를 향해 이동
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(
                transform.position, 
                player.position, 
                moveSpeed * Time.deltaTime
            );
            
            // 플레이어 방향으로 회전 (2D)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
        else
        {
            isChasing = false;
        }
        
        // 피격 깜빡임 효과
        if (spriteRenderer != null)
        {
            if (Time.time - lastDamageTime < blinkDuration)
            {
                // 깜빡임 효과
                float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                Color blinkColor = isChasing ? chaseColor : idleColor;
                blinkColor.a = alpha;
                spriteRenderer.color = blinkColor;
            }
            else
            {
                // 정상 색상
                Color normalColor = isChasing ? chaseColor : idleColor;
                normalColor.a = 1f;
                spriteRenderer.color = normalColor;
            }
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        Debug.Log($"[Enemy] TakeDamage 호출됨! 현재 체력: {health}, 받은 데미지: {damageAmount}");
        health -= damageAmount;
        lastDamageTime = Time.time; // 깜빡임 효과 트리거
        Debug.Log($"[Enemy] 데미지 처리 후 체력: {health}/{maxHealth}");
        
        if (health <= 0)
        {
            Debug.Log("[Enemy] 체력 0 이하, Die() 호출");
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("적 사망!");
        Destroy(gameObject);
    }
    
    // Scene 뷰에서 추적 범위 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}


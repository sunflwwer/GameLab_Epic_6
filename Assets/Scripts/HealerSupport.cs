using UnityEngine;
using System.Collections;

public class HealerSupport : MonoBehaviour
{
    [Header("Heal Settings")]
    [Tooltip("힐 주기 (초)")]
    public float healInterval = 5f;
    
    [Tooltip("힐 회복량")]
    public int healAmount = 1;
    
    [Tooltip("힐 범위 (0이면 모든 아군)")]
    public float healRange = 0f;
    
    [Header("Staff Settings")]
    [Tooltip("지팡이 크기")]
    public float staffSize = 0.5f;
    
    [Tooltip("지팡이 색상")]
    public Color staffColor = new Color(1f, 0.9f, 0.6f, 1f); // 황금색
    
    [Header("Heal Effect")]
    [Tooltip("힐 이펙트 색상")]
    public Color healEffectColor = new Color(0.5f, 1f, 0.5f, 1f); // 초록색
    
    [Tooltip("힐 이펙트 지속 시간")]
    public float healEffectDuration = 0.5f;
    
    private GameObject staffObject;
    private float lastHealTime;

    void Start()
    {
        CreateStaff();
        lastHealTime = Time.time;
    }

    void Update()
    {
        // 주기적으로 힐
        if (Time.time - lastHealTime >= healInterval)
        {
            PerformHeal();
            lastHealTime = Time.time;
        }
    }

    void CreateStaff()
    {
        // 지팡이 오브젝트 생성
        staffObject = new GameObject("HealerStaff");
        staffObject.transform.SetParent(transform);
        staffObject.transform.localPosition = new Vector3(0, -staffSize * 0.2f, 0); // 플레이어 약간 아래에 배치
        
        // 스프라이트 렌더러 추가
        SpriteRenderer staffRenderer = staffObject.AddComponent<SpriteRenderer>();
        staffRenderer.sprite = CreateStaffSprite();
        staffRenderer.color = staffColor;
        staffRenderer.sortingOrder = 3;
        
        Debug.Log($"[HealerSupport] {gameObject.name}의 힐 지팡이 생성 완료!");
    }

    Sprite CreateStaffSprite()
    {
        // 십자가 모양의 지팡이 스프라이트 생성
        int size = (int)(staffSize * 100);
        Texture2D texture = new Texture2D(size, size);
        
        Color transparent = new Color(0, 0, 0, 0);
        
        // 배경을 투명하게
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                texture.SetPixel(x, y, transparent);
            }
        }
        
        // 십자가(+) 그리기
        int center = size / 2;
        int thickness = size / 8;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 세로 막대
                if (x > center - thickness && x < center + thickness)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                // 가로 막대
                if (y > center - thickness && y < center + thickness)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    void PerformHeal()
    {
        Debug.Log($"[HealerSupport] {gameObject.name}이(가) 힐 시전!");
        
        // 플레이어 힐
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (healRange <= 0 || Vector2.Distance(transform.position, player.transform.position) <= healRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(healAmount);
                    ShowHealEffect(player.transform);
                }
            }
        }
        
        // 모든 Follower 힐
        Follower[] followers = FindObjectsByType<Follower>(FindObjectsSortMode.None);
        foreach (Follower follower in followers)
        {
            if (follower == null) continue;
            if (healRange > 0 && Vector2.Distance(transform.position, follower.transform.position) > healRange) continue;
            
            // Follower에 체력 시스템이 있다면 힐 (추후 확장 가능)
            ShowHealEffect(follower.transform);
            Debug.Log($"[HealerSupport] {follower.gameObject.name}에게 힐!");
        }
    }

    void ShowHealEffect(Transform target)
    {
        // 힐 이펙트 생성
        GameObject healEffect = new GameObject("HealEffect");
        healEffect.transform.position = target.position;
        
        // 스프라이트 렌더러 추가
        SpriteRenderer effectRenderer = healEffect.AddComponent<SpriteRenderer>();
        effectRenderer.sprite = CreateHealEffectSprite();
        effectRenderer.color = healEffectColor;
        effectRenderer.sortingOrder = 10;
        
        // 이펙트 애니메이션 스크립트 추가
        HealEffectAnimation effectAnim = healEffect.AddComponent<HealEffectAnimation>();
        effectAnim.duration = healEffectDuration;
        effectAnim.target = target;
    }

    Sprite CreateHealEffectSprite()
    {
        // 작은 십자가 이펙트
        int size = 30;
        Texture2D texture = new Texture2D(size, size);
        
        Color transparent = new Color(0, 0, 0, 0);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                texture.SetPixel(x, y, transparent);
            }
        }
        
        int center = size / 2;
        int thickness = size / 6;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if ((x > center - thickness && x < center + thickness) ||
                    (y > center - thickness && y < center + thickness))
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    void OnDrawGizmosSelected()
    {
        if (healRange > 0)
        {
            // 힐 범위 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healRange);
        }
    }
}

// 힐 이펙트 애니메이션 스크립트
public class HealEffectAnimation : MonoBehaviour
{
    public float duration = 0.5f;
    public Transform target;
    
    private float startTime;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float progress = elapsed / duration;
        
        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        
        // 타겟 따라가기
        if (target != null)
        {
            transform.position = target.position + Vector3.up * progress;
        }
        
        // 페이드 아웃
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f - progress;
            spriteRenderer.color = color;
        }
        
        // 크기 증가
        transform.localScale = Vector3.one * (1f + progress * 0.5f);
    }
}


using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("최대 체력")]
    public int maxHealth = 100;
    
    [Tooltip("현재 체력")]
    public int currentHealth;
    
    [Header("UI Settings")]
    [Tooltip("체력바 Slider (Slider 사용 시)")]
    public Slider healthSlider;
    
    [Tooltip("체력바 Fill Image (Image 사용 시)")]
    public Image healthBarFill;
    
    [Tooltip("체력 텍스트 (선택)")]
    public Text healthText;
    
    [Header("Damage Settings")]
    [Tooltip("무적 시간 (초)")]
    public float invincibilityTime = 1f;
    
    private float lastDamageTime = -999f; // 초기화: 게임 시작 시 깜빡임 방지
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Slider 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            Debug.Log($"[PlayerHealth] Slider 초기화: {currentHealth}/{maxHealth}");
        }
        
        UpdateHealthUI();
    }

    void Update()
    {
        // 무적 시간 깜빡임 효과
        if (Time.time - lastDamageTime < invincibilityTime)
        {
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
        else
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // 무적 시간 체크
        if (Time.time - lastDamageTime < invincibilityTime)
        {
            return;
        }

        currentHealth -= damage;
        lastDamageTime = Time.time;
        
        Debug.Log($"플레이어가 {damage} 데미지를 받았습니다! 남은 체력: {currentHealth}/{maxHealth}");
        
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
        Debug.Log($"플레이어가 {amount} 회복했습니다! 현재 체력: {currentHealth}/{maxHealth}");
    }

    void UpdateHealthUI()
    {
        // Slider 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            Debug.Log($"[PlayerHealth] Slider 업데이트: {healthSlider.value}/{healthSlider.maxValue}");
        }
        
        // Fill Image 업데이트
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        
        // 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망!");
        // 게임 오버 로직 추가 가능
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 재시작
    }
}


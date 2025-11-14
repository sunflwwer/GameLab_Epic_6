using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Manual UI Setup - Inspector에서 직접 연결")]
    [Tooltip("체력바 Slider (Slider 사용 시)")]
    public Slider healthSlider;
    
    [Tooltip("체력바 Fill Image (Image 사용 시)")]
    public Image healthBarFill;
    
    private Enemy enemy;

    void Start()
    {
        // 같은 오브젝트 또는 부모 오브젝트에서 Enemy 찾기
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = GetComponentInParent<Enemy>();
        }
        
        if (enemy == null)
        {
            Debug.LogError("[EnemyHealthBar] Enemy 컴포넌트를 찾을 수 없습니다! 이 스크립트는 Enemy GameObject 또는 그 자식에 있어야 합니다.");
            return;
        }
        
        Debug.Log($"[EnemyHealthBar] Enemy 찾음: {enemy.gameObject.name}");
        
        // Slider 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = enemy.maxHealth;
            healthSlider.value = enemy.health;
            Debug.Log($"[EnemyHealthBar] Slider 초기화: {enemy.health}/{enemy.maxHealth}");
        }
        // Fill Image 초기화
        else if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)enemy.health / enemy.maxHealth;
            Debug.Log($"[EnemyHealthBar] Fill Image 초기화: {enemy.health}/{enemy.maxHealth}");
        }
        else
        {
            Debug.LogWarning("[EnemyHealthBar] healthSlider 또는 healthBarFill을 Inspector에서 연결해주세요!");
        }
    }

    void Update()
    {
        if (enemy == null) return;
        
        // Slider 방식
        if (healthSlider != null)
        {
            healthSlider.value = enemy.health;
        }
        // Fill Image 방식
        else if (healthBarFill != null)
        {
            float fillAmount = (float)enemy.health / enemy.maxHealth;
            healthBarFill.fillAmount = fillAmount;
        }
    }
}


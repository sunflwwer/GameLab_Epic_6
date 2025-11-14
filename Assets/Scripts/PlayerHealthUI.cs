using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas canvas;
    public Image healthBarFill;
    public Text healthText;
    
    private PlayerHealth playerHealth;

    void Start()
    {
        // Canvas가 없으면 생성
        if (canvas == null)
        {
            CreateHealthUI();
        }
        
        // PlayerHealth 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.healthBarFill = healthBarFill;
                playerHealth.healthText = healthText;
            }
        }
    }

    void CreateHealthUI()
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("PlayerHealthCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 체력바 패널 생성
        GameObject panelObj = new GameObject("HealthBarPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.02f, 0.95f);
        panelRect.anchorMax = new Vector2(0.3f, 0.98f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;
        
        // 배경
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(panelObj.transform);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // 체력바
        GameObject fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(bgObj.transform);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-10, -10);
        fillRect.anchoredPosition = Vector2.zero;
        
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = new Color(0.2f, 1f, 0.2f, 1f); // 초록색
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        // 체력 텍스트
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(panelObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        healthText = textObj.AddComponent<Text>();
        healthText.text = "100/100";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 24;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        
        Debug.Log("플레이어 체력 UI 생성 완료!");
    }
}


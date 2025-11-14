using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("스폰할 적 프리팹")]
    public GameObject enemyPrefab;
    
    [Tooltip("스폰 간격 (초)")]
    public float spawnInterval = 3f;
    
    [Tooltip("최대 동시 적 개수")]
    public int maxEnemies = 10;
    
    [Tooltip("스폰 범위 (카메라 기준)")]
    public float spawnDistance = 15f;
    
    [Header("Spawn Area")]
    [Tooltip("스폰 영역 타입")]
    public SpawnAreaType spawnAreaType = SpawnAreaType.AroundCamera;
    
    [Tooltip("고정 스폰 영역 (SpawnArea 타입이 Fixed일 때)")]
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    
    private float nextSpawnTime;
    private Camera mainCamera;
    private int currentEnemyCount = 0;
    
    public enum SpawnAreaType
    {
        AroundCamera,   // 카메라 주변
        Fixed           // 고정 영역
    }

    void Start()
    {
        mainCamera = Camera.main;
        nextSpawnTime = Time.time + spawnInterval;
        
        if (enemyPrefab == null)
        {
            Debug.LogError("적 프리팹이 설정되지 않았습니다! Inspector에서 Enemy Prefab을 설정하세요.");
        }
    }

    void Update()
    {
        if (enemyPrefab == null) return;
        
        // 적 개수 업데이트
        currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        
        // 스폰 타이밍 및 최대 개수 체크
        if (Time.time >= nextSpawnTime && currentEnemyCount < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnEnemy()
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy"; // 태그 자동 설정
        
        Debug.Log($"적 스폰! 위치: {spawnPosition}, 현재 적 개수: {currentEnemyCount + 1}");
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        Vector2 spawnPos = Vector2.zero;
        
        if (spawnAreaType == SpawnAreaType.AroundCamera && mainCamera != null)
        {
            // 카메라 밖에서 랜덤 스폰
            Vector2 cameraPos = mainCamera.transform.position;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            spawnPos = cameraPos + new Vector2(
                Mathf.Cos(angle) * spawnDistance,
                Mathf.Sin(angle) * spawnDistance
            );
        }
        else
        {
            // 고정 영역 내 랜덤 스폰
            spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
        }
        
        return spawnPos;
    }
    
    // Scene 뷰에서 스폰 영역 표시
    void OnDrawGizmos()
    {
        if (spawnAreaType == SpawnAreaType.AroundCamera && mainCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mainCamera.transform.position, spawnDistance);
        }
        else if (spawnAreaType == SpawnAreaType.Fixed)
        {
            Gizmos.color = Color.cyan;
            Vector2 center = (spawnAreaMin + spawnAreaMax) / 2f;
            Vector2 size = spawnAreaMax - spawnAreaMin;
            Gizmos.DrawWireCube(center, size);
        }
    }
}


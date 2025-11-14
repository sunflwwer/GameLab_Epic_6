using UnityEngine;
using System.Collections.Generic;

public class FollowerManager : MonoBehaviour
{
    [Header("Follower Settings")]
    [Tooltip("친구 프리팹")]
    public GameObject followerPrefab;
    
    [Tooltip("생성할 친구 수")]
    public int followerCount = 3;
    
    [Tooltip("친구들 사이의 거리")]
    public float spacing = 2f;
    
    [Tooltip("친구들의 타입 (순서대로)")]
    public Follower.FollowerType[] followerTypes = new Follower.FollowerType[]
    {
        Follower.FollowerType.ShieldBearer,
        Follower.FollowerType.Swordsman,
        Follower.FollowerType.Archer,
        Follower.FollowerType.Healer
    };
    
    [Header("Auto Setup")]
    [Tooltip("시작 시 자동으로 친구들 생성")]
    public bool autoCreateOnStart = true;
    
    private List<Follower> followers = new List<Follower>();
    private Transform player;

    void Start()
    {
        if (autoCreateOnStart && followerPrefab != null)
        {
            CreateFollowers();
        }
    }

    public void CreateFollowers()
    {
        if (followerPrefab == null)
        {
            Debug.LogError("[FollowerManager] 친구 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        // 기존 친구들 제거
        ClearFollowers();
        
        Transform lastTarget = null;
        Vector3 spawnCenter = transform.position;
        
        for (int i = 0; i < followerCount; i++)
        {
            // 친구 생성
            Vector3 spawnPosition = spawnCenter - Vector3.right * spacing * i;
            GameObject followerObj = Instantiate(followerPrefab, spawnPosition, Quaternion.identity);
            followerObj.name = $"Follower_{i + 1}";
            followerObj.transform.SetParent(transform); // FollowerManager의 자식으로 설정
            
            Follower follower = followerObj.GetComponent<Follower>();
            if (follower == null)
            {
                follower = followerObj.AddComponent<Follower>();
            }
            
            // 첫 번째 Follower는 리더 (Player 태그 부여)
            if (i == 0)
            {
                followerObj.tag = "Player";
                
                // LeaderFollowMouse 컴포넌트 추가
                LeaderFollowMouse leaderMovement = followerObj.AddComponent<LeaderFollowMouse>();
                
                follower.target = null; // 리더는 타겟이 없음
                
                Debug.Log($"[FollowerManager] {followerObj.name}을(를) 리더로 설정 (마우스 따라가기)");
            }
            else
            {
                // 타입 설정 (배열에서 순환, 첫 번째는 리더이므로 i-1)
                if (followerTypes != null && followerTypes.Length > 0)
                {
                    follower.followerType = followerTypes[(i - 1) % followerTypes.Length];
                }
                
                // 타겟 설정 (이전 친구)
                follower.target = lastTarget;
                follower.followDistance = spacing;
                
                Debug.Log($"[FollowerManager] {followerObj.name} 생성 완료. 타겟: {(lastTarget != null ? lastTarget.name : "없음")}");
            }
            
            // 체인 연결
            if (followers.Count > 0)
            {
                followers[followers.Count - 1].nextFollower = follower;
            }
            
            followers.Add(follower);
            lastTarget = followerObj.transform;
        }
        
        Debug.Log($"[FollowerManager] 총 {followerCount}명의 Follower 생성 완료! (리더 1명 포함)");
    }

    public void ClearFollowers()
    {
        foreach (Follower follower in followers)
        {
            if (follower != null)
            {
                Destroy(follower.gameObject);
            }
        }
        followers.Clear();
    }

    public void AddFollower()
    {
        if (followerPrefab == null || player == null) return;
        
        Transform lastTarget = followers.Count > 0 ? followers[followers.Count - 1].transform : player;
        
        Vector3 spawnPosition = lastTarget.position - Vector3.right * spacing;
        GameObject followerObj = Instantiate(followerPrefab, spawnPosition, Quaternion.identity);
        followerObj.name = $"Follower_{followers.Count + 1}";
        followerObj.transform.SetParent(transform);
        
        Follower follower = followerObj.GetComponent<Follower>();
        if (follower == null)
        {
            follower = followerObj.AddComponent<Follower>();
        }
        
        // 타입 설정
        if (followerTypes != null && followerTypes.Length > 0)
        {
            follower.followerType = followerTypes[followers.Count % followerTypes.Length];
        }
        
        follower.target = lastTarget;
        follower.followDistance = spacing;
        
        if (followers.Count > 0)
        {
            followers[followers.Count - 1].nextFollower = follower;
        }
        
        followers.Add(follower);
        
        Debug.Log($"[FollowerManager] {followerObj.name} 추가됨!");
    }

    public void RemoveLastFollower()
    {
        if (followers.Count == 0) return;
        
        Follower lastFollower = followers[followers.Count - 1];
        followers.RemoveAt(followers.Count - 1);
        
        if (followers.Count > 0)
        {
            followers[followers.Count - 1].nextFollower = null;
        }
        
        Destroy(lastFollower.gameObject);
        
        Debug.Log("[FollowerManager] 마지막 친구 제거됨!");
    }
}


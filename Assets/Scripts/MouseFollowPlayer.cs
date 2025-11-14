using UnityEngine;

public class MouseFollowPlayer : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("마우스 커서 뒤에서 얼마나 떨어져 있을지")]
    public float followDistance = 3f;
    
    [Tooltip("플레이어의 Y 높이 (고정)")]
    public float height = 1f;
    
    [Tooltip("부드러운 이동 속도 (값이 클수록 더 천천히 따라옴)")]
    public float smoothTime = 0.2f;
    
    [Header("Rotation Settings")]
    [Tooltip("이동 방향을 바라보게 할지")]
    public bool faceMovementDirection = true;
    
    [Tooltip("회전 속도 (값이 클수록 빠르게 회전)")]
    public float rotationSpeed = 5f;
    
    [Header("Camera Bounds")]
    [Tooltip("카메라 범위 내로 플레이어 제한")]
    public bool constrainToCamera = true;
    
    [Tooltip("카메라 경계에서 안쪽 여백")]
    public float cameraPadding = 0.5f;
    
    private Vector3 velocity = Vector3.zero;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 2D: 마우스 위치를 월드 좌표로 직접 변환
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = mainCamera.nearClipPlane + 10f; // 카메라로부터의 거리
        
        Vector3 cursorWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        
        // 디버그: 마우스 위치 표시 (2D에서는 원으로)
        Debug.DrawLine(cursorWorldPosition + Vector3.up * 0.5f, cursorWorldPosition - Vector3.up * 0.5f, Color.red);
        Debug.DrawLine(cursorWorldPosition + Vector3.right * 0.5f, cursorWorldPosition - Vector3.right * 0.5f, Color.red);
        
        // 카메라에서 커서로 가는 방향
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cursorDirection = (cursorWorldPosition - cameraPosition).normalized;
        
        // 커서 위치에서 카메라 방향의 반대쪽으로 followDistance만큼 떨어진 위치
        Vector3 targetPosition = cursorWorldPosition - cursorDirection * followDistance;
        
        // Z 위치는 플레이어의 현재 Z 유지 (2D에서 중요)
        targetPosition.z = transform.position.z;
        
        // 디버그: 목표 위치 표시
        Debug.DrawLine(targetPosition + Vector3.up * 0.7f, targetPosition - Vector3.up * 0.7f, Color.green);
        Debug.DrawLine(targetPosition + Vector3.right * 0.7f, targetPosition - Vector3.right * 0.7f, Color.green);
        Debug.DrawLine(cursorWorldPosition, targetPosition, Color.yellow);
        
        // 부드럽게 이동
        Vector3 newPosition = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            smoothTime
        );
        newPosition.z = transform.position.z; // Z 위치 고정
        
        // 카메라 범위 내로 제한
        if (constrainToCamera)
        {
            newPosition = ClampToCamera(newPosition);
        }
        
        transform.position = newPosition;
        
        // 2D 회전: Z축 기준으로 회전
        if (faceMovementDirection && velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f); // -90은 스프라이트 기본 방향 조정
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }

    Vector3 ClampToCamera(Vector3 position)
    {
        if (mainCamera == null) return position;
        
        // 카메라 범위 계산 (2D Orthographic)
        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        
        Vector3 camPos = mainCamera.transform.position;
        
        // 카메라 경계 계산 (여백 포함)
        float minX = camPos.x - camWidth + cameraPadding;
        float maxX = camPos.x + camWidth - cameraPadding;
        float minY = camPos.y - camHeight + cameraPadding;
        float maxY = camPos.y + camHeight - cameraPadding;
        
        // 위치를 카메라 범위 내로 제한
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
}

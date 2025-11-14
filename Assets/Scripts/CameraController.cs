using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("카메라 이동 속도")]
    public float moveSpeed = 5f;
    
    [Tooltip("Shift 키를 누르면 빠르게 이동")]
    public float fastSpeedMultiplier = 2f;
    
    [Header("Zoom Settings")]
    [Tooltip("마우스 휠 줌 속도")]
    public float zoomSpeed = 2f;
    
    [Tooltip("최소 줌 (가까이)")]
    public float minZoom = 2f;
    
    [Tooltip("최대 줌 (멀리)")]
    public float maxZoom = 20f;
    
    [Header("Boundary Settings")]
    [Tooltip("이동 범위 제한 사용")]
    public bool useBoundary = false;
    
    [Tooltip("최소 X 좌표")]
    public float minX = -50f;
    
    [Tooltip("최대 X 좌표")]
    public float maxX = 50f;
    
    [Tooltip("최소 Y 좌표")]
    public float minY = -50f;
    
    [Tooltip("최대 Y 좌표")]
    public float maxY = 50f;
    
    private float currentSpeed;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera 컴포넌트를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 마우스 휠로 줌 조절
        HandleZoom();
        
        // Shift 키를 누르면 빠른 속도
        currentSpeed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) 
            ? moveSpeed * fastSpeedMultiplier 
            : moveSpeed;
        
        // WASD 입력 받기
        float horizontal = 0f;
        float vertical = 0f;
        
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        
        // 이동 벡터 계산
        Vector3 movement = new Vector3(horizontal, vertical, 0f);
        
        // 대각선 이동 시 속도 정규화
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }
        
        // 카메라 이동
        transform.position += movement * currentSpeed * Time.deltaTime;
        
        // 범위 제한
        if (useBoundary)
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            transform.position = clampedPosition;
        }
    }
    
    void HandleZoom()
    {
        if (cam == null) return;
        
        // 마우스 휠 입력 받기
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0f)
        {
            // Orthographic Size 조절 (2D 카메라)
            if (cam.orthographic)
            {
                cam.orthographicSize -= scrollInput * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
            // Perspective 카메라의 경우 (3D)
            else
            {
                float newZ = transform.position.z + scrollInput * zoomSpeed;
                newZ = Mathf.Clamp(newZ, -maxZoom, -minZoom);
                Vector3 pos = transform.position;
                pos.z = newZ;
                transform.position = pos;
            }
        }
    }
    
    // Scene 뷰에서 이동 범위 표시
    void OnDrawGizmosSelected()
    {
        if (useBoundary)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}


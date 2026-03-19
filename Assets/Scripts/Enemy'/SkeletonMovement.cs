using UnityEngine;

public class SkeletonMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;      // Các điểm patrol (kéo PatrolRoute children vào đây)
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Chase Settings")]
    [SerializeField] private Transform player;              // Kéo Player GameObject vào đây trong Inspector
    [SerializeField] private float chaseSpeed = 4f;         // Nhanh hơn patrol
    [SerializeField] private float detectionRange = 10f;    // Khoảng cách phát hiện player (tăng/giảm tùy ý)
    [SerializeField] private float attackRange = 1.5f;      // Khi gần thế này thì dừng lại tấn công (nếu có)

    private int currentPointIndex = 0;
    private Animator animator;
    private float currentSpeed;

    void Start()
    {
        animator = GetComponent<Animator>(); // Hoặc GetComponentInChildren nếu Animator ở child (như Root_M)
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform; // Tự tìm nếu chưa kéo
            if (player == null) Debug.LogWarning("Không tìm thấy Player! Tag Player phải là 'Player'");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Chase mode
            ChasePlayer();
            currentSpeed = chaseSpeed;

            // Nếu có animation Run
            if (animator != null) animator.SetFloat("Speed", 1f); // Giả sử "Speed" >0 là run
        }
        else
        {
            // Patrol mode
            if (patrolPoints.Length > 0)
            {
                Patrol();
                currentSpeed = patrolSpeed;
                if (animator != null) animator.SetFloat("Speed", 0.5f); // Walk chậm
            }
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        // Di chuyển
        transform.position += direction * chaseSpeed * Time.deltaTime;

        // Xoay về player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
        }

        // Nếu gần quá → có thể trigger attack (thêm sau nếu cần)
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            // Ví dụ: animator.SetTrigger("Attack");
            // Hoặc gọi hàm damage player
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += direction * patrolSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;  // ← Thêm Animator
    public Transform player;

    [Header("Detection & Attack Range")]
    public float detectionRange = 12f;
    public float attackRange = 2.5f;

    [Header("Roaming Settings")]
    public float roamRadius = 15f;
    public float minDistanceToTarget = 1.5f;

    public EnemyStateMachine stateMachine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();  // Tìm Animator trong con
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stateMachine = GetComponent<EnemyStateMachine>();
    }

    private void Start()
    {
        stateMachine.Initialize(new RoamingState(this));
    }

    private void Update()
    {
        // Cập nhật Speed cho Animator (tự động chạy Walk/Run dựa trên vận tốc)
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    // Các hàm helper
    public bool PlayerInDetectionRange()
    {
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    public bool PlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

    // Hàm public để các state gọi chuyển state
    public void ChangeState(EnemyState newState)
    {
        stateMachine.ChangeState(newState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
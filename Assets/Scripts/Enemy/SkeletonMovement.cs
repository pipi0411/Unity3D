using UnityEngine;
using UnityEngine.AI;

public class SkeletonMovement : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    [SerializeField] private float wanderRadius = 40f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float playerSearchInterval = 1f;
    [SerializeField] private float navMeshSnapDistance = 2f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private NavMeshPath cachedPath;
    private float nextPlayerSearchTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("SkeletonMovement requires a NavMeshAgent component.", this);
            enabled = false;
            return;
        }

        cachedPath = new NavMeshPath();

        TryFindPlayer(force: true);

        agent.stoppingDistance = 1.0f;
        agent.acceleration = 8f;

        TrySnapToNavMesh();
        SetRandomDestination();
    }

    void Update()
    {
        if (agent == null) return;

        if (!agent.isOnNavMesh && !TrySnapToNavMesh()) return;

        TryFindPlayer();

        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Wander();
        }

        UpdateAnimation();
    }

    void ChasePlayer()
    {
        if (player == null || !agent.isOnNavMesh) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void Wander()
    {
        if (!agent.isOnNavMesh) return;

        agent.speed = patrolSpeed;

        bool needsNewDestination = !agent.pathPending &&
                                   (!agent.hasPath ||
                                    agent.pathStatus != NavMeshPathStatus.PathComplete ||
                                    agent.remainingDistance <= Mathf.Max(agent.stoppingDistance + 0.2f, 0.5f));

        if (needsNewDestination)
        {
            SetRandomDestination();
        }
    }

    void SetRandomDestination()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 finalPosition = Vector3.zero;
        bool foundPoint = false;
        Vector3 currentPosition = transform.position;

        // Chọn điểm trên mặt phẳng XZ để tránh chọn điểm cao/thấp gây lỗi path.
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 randomDirection = currentPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, navMeshSnapDistance, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(hit.position, cachedPath) && cachedPath.status == NavMeshPathStatus.PathComplete)
                {
                    finalPosition = hit.position;
                    foundPoint = true;
                    break;
                }
            }
        }

        if (foundPoint)
        {
            agent.SetDestination(finalPosition);
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            float speedPercent = agent.velocity.magnitude / chaseSpeed;
            animator.SetFloat("Speed", speedPercent);
        }
    }

    bool TryFindPlayer(bool force = false)
    {
        if (!force && Time.time < nextPlayerSearchTime) return player != null;

        nextPlayerSearchTime = Time.time + Mathf.Max(0.2f, playerSearchInterval);

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        player = playerObj != null ? playerObj.transform : null;

        return player != null;
    }

    bool TrySnapToNavMesh()
    {
        if (agent.isOnNavMesh) return true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            return agent.Warp(hit.position);
        }

        return false;
    }
}
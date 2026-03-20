using UnityEngine;
using UnityEngine.AI;

public class SkeletonMovement : MonoBehaviour, IDamageable
{
    [Header("Cấu hình di chuyển")]
    [SerializeField] private float wanderRadius = 40f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float playerSearchInterval = 1f;
    [SerializeField] private float navMeshSnapDistance = 2f;

    [Header("Cấu hình máu")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Hiển thị máu trên đầu")]
    [SerializeField] private bool showHealthNumber = true;
    [SerializeField] private Vector3 healthNumberOffset = new Vector3(0f, 2.1f, 0f);
    [SerializeField] private int healthTextFontSize = 42;
    [SerializeField] private float healthTextCharacterSize = 0.04f;
    [SerializeField] private Color fullHealthColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float healthVisibleDistance = 15f;
    [SerializeField] private bool hideHealthNumberOnDeath = true;
    [SerializeField] private TextMesh healthNumberText;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private IDamageable playerDamageable;
    private NavMeshPath cachedPath;
    private float nextPlayerSearchTime;
    private float nextAttackTime;
    private float currentHealth;
    private bool isDead;
    private Transform healthNumberTransform;
    private Camera mainCamera;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        isDead = false;

        SetupHealthNumber();
        RefreshHealthNumber();
    }

    void LateUpdate()
    {
        UpdateHealthNumberBillboard();
    }

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
        if (agent == null || isDead) return;

        if (!agent.isOnNavMesh && !TrySnapToNavMesh()) return;

        TryFindPlayer();

        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Wander();
        }

        UpdateAnimation();
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        RefreshHealthNumber();

        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    void HandleDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        if (hideHealthNumberOnDeath && healthNumberText != null)
        {
            healthNumberText.gameObject.SetActive(false);
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, Mathf.Max(0f, destroyDelay));
        }
    }

    void ChasePlayer(float distanceToPlayer)
    {
        if (player == null || !agent.isOnNavMesh) return;

        agent.speed = chaseSpeed;

        if (distanceToPlayer <= attackRange)
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            TryAttackPlayer();
            return;
        }

        agent.SetDestination(player.position);
    }

    void TryAttackPlayer()
    {
        if (playerDamageable == null)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + Mathf.Max(0.1f, attackCooldown);
        playerDamageable.TakeDamage(Mathf.Max(0f, attackDamage));
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
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Player playerComponent = FindAnyObjectByType<Player>();
            player = playerComponent != null ? playerComponent.transform : null;
        }

        playerDamageable = player != null ? player.GetComponentInParent<IDamageable>() : null;

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

    void SetupHealthNumber()
    {
        if (!showHealthNumber)
        {
            return;
        }

        mainCamera = Camera.main;

        if (healthNumberText == null)
        {
            GameObject textObject = new GameObject("EnemyHealthNumber");
            textObject.transform.SetParent(transform);
            healthNumberText = textObject.AddComponent<TextMesh>();
        }

        healthNumberTransform = healthNumberText.transform;
        healthNumberTransform.position = transform.position + healthNumberOffset;

        healthNumberText.anchor = TextAnchor.MiddleCenter;
        healthNumberText.alignment = TextAlignment.Center;
        healthNumberText.fontSize = Mathf.Max(8, healthTextFontSize);
        healthNumberText.characterSize = Mathf.Max(0.01f, healthTextCharacterSize);
    }

    void UpdateHealthNumberBillboard()
    {
        if (!showHealthNumber || healthNumberTransform == null)
        {
            return;
        }

        if (isDead && hideHealthNumberOnDeath)
        {
            if (healthNumberText != null && healthNumberText.gameObject.activeSelf)
            {
                healthNumberText.gameObject.SetActive(false);
            }

            return;
        }

        healthNumberTransform.position = transform.position + healthNumberOffset;

        if (mainCamera == null || !mainCamera.isActiveAndEnabled)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            float maxDistance = Mathf.Max(0f, healthVisibleDistance);
            bool isInVisibleRange = maxDistance <= 0f ||
                                    Vector3.SqrMagnitude(mainCamera.transform.position - healthNumberTransform.position) <= maxDistance * maxDistance;

            if (healthNumberText != null)
            {
                healthNumberText.gameObject.SetActive(isInVisibleRange);
            }

            if (!isInVisibleRange)
            {
                return;
            }

            healthNumberTransform.LookAt(
                healthNumberTransform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
    }

    void RefreshHealthNumber()
    {
        if (!showHealthNumber || healthNumberText == null)
        {
            return;
        }

        int currentHealthInt = Mathf.CeilToInt(currentHealth);
        int maxHealthInt = Mathf.CeilToInt(maxHealth);
        healthNumberText.text = currentHealthInt + "/" + maxHealthInt;

        float healthPercent = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        healthNumberText.color = Color.Lerp(lowHealthColor, fullHealthColor, Mathf.Clamp01(healthPercent));
    }
}
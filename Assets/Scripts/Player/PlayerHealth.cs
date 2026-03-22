using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath;

    [Header("Auto Regeneration")]
    [SerializeField] private bool enableAutoRegen = true;
    [SerializeField] private float minNoDamageTime = 5f;
    [SerializeField] private float maxNoDamageTime = 10f;

    [Header("Lose Scene")]
    [SerializeField] private bool loadSceneOnDeath = true;
    [SerializeField] private string lostSceneName = "LostGame";

    [Header("Fall Death")]
    [SerializeField] private bool dieWhenFalling = true;
    [SerializeField] private float fallDeathY = -10f;

    private float currentHealth;
    private bool isDead;
    private float regenReadyTime = -1f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        isDead = false;
    }

    private void OnEnable()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (!isDead && dieWhenFalling && transform.position.y <= fallDeathY)
        {
            currentHealth = 0f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            HandleDeath();
            return;
        }

        if (!enableAutoRegen || isDead)
        {
            return;
        }

        if (currentHealth >= maxHealth)
        {
            regenReadyTime = -1f;
            return;
        }

        if (regenReadyTime > 0f && Time.time >= regenReadyTime)
        {
            RestoreFullHealth();
            regenReadyTime = -1f;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            HandleDeath();
            return;
        }

        ScheduleAutoRegen();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth >= maxHealth)
        {
            regenReadyTime = -1f;
        }
    }

    public void RestoreFullHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        regenReadyTime = -1f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        isDead = currentHealth <= 0f;
        regenReadyTime = -1f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void ScheduleAutoRegen()
    {
        if (!enableAutoRegen)
        {
            return;
        }

        float minDelay = Mathf.Max(0f, minNoDamageTime);
        float maxDelay = Mathf.Max(minDelay, maxNoDamageTime);
        regenReadyTime = Time.time + UnityEngine.Random.Range(minDelay, maxDelay);
    }

    private void HandleDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        OnDeath?.Invoke();

        if (loadSceneOnDeath)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!string.IsNullOrWhiteSpace(lostSceneName) && Application.CanStreamedLevelBeLoaded(lostSceneName))
            {
                SceneManager.LoadScene(lostSceneName);
                return;
            }
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public class AttackState : EnemyState
{
    private float attackCooldown = 1.5f;
    private float timer = 0f;

    public AttackState(EnemyAI enemy) : base(enemy) { }

public override void Update()
{
    timer -= Time.deltaTime;

    if (timer <= 0)
    {
        enemy.animator.SetTrigger("Attack");   // ← Kích hoạt animation Attack
        timer = attackCooldown;
        enemy.animator.SetTrigger("GetHit");
        enemy.animator.SetTrigger("Die");
        enemy.animator.SetBool("IsDead", true);
    }
}
}
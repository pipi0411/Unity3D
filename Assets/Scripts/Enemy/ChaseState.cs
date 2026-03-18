using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

public override void Update()
{
    enemy.agent.SetDestination(enemy.player.position);
    enemy.animator.SetFloat("Speed", enemy.agent.velocity.magnitude);   // ← Thêm

    if (enemy.PlayerInAttackRange())
        enemy.stateMachine.ChangeState(new AttackState(enemy));

        enemy.animator.SetTrigger("GetHit");
        enemy.animator.SetTrigger("Die");
        enemy.animator.SetBool("IsDead", true);
}
}
using UnityEngine;
using UnityEngine.AI;

public class RoamingState : EnemyState
{
    private Vector3 roamTarget;
    private float waitTimer = 0f;
    private float waitTime = 2.5f;  // Chờ 2.5 giây ở mỗi điểm (tùy chỉnh)

    public RoamingState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        ChooseNewRoamTarget();
        enemy.agent.SetDestination(roamTarget);
    }

    public override void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        // Nếu đến gần điểm → chờ rồi chọn điểm mới
        if (Vector3.Distance(enemy.transform.position, roamTarget) < enemy.minDistanceToTarget)
        {
            waitTimer = waitTime;
            ChooseNewRoamTarget();
            enemy.agent.SetDestination(roamTarget);
        }

        // Nếu thấy player → chuyển sang Chase
        if (enemy.PlayerInDetectionRange())
        {
            enemy.stateMachine.ChangeState(new ChaseState(enemy));
        }

        enemy.animator.SetFloat("Speed", enemy.agent.velocity.magnitude);
        enemy.animator.SetTrigger("GetHit");
        enemy.animator.SetTrigger("Die");
        enemy.animator.SetBool("IsDead", true);
    }

    private void ChooseNewRoamTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * enemy.roamRadius;
        randomDirection += enemy.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, enemy.roamRadius, NavMesh.AllAreas))
        {
            roamTarget = hit.position;
        }
        else
        {
            roamTarget = enemy.transform.position;
        }
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(enemy.transform.position, enemy.roamRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(roamTarget, 0.5f);
        Gizmos.DrawLine(enemy.transform.position, roamTarget);
    }
}
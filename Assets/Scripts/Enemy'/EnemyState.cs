using UnityEngine;

public abstract class EnemyState
{
    protected EnemyAI enemy;

    public EnemyState(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    // Vẽ Gizmos cho từng state (nếu cần debug)
    public virtual void OnDrawGizmos() { }
}
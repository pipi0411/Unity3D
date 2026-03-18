using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public EnemyState currentState;

    public void Initialize(EnemyState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    private void Update()
    {
        if (currentState != null)
            currentState.Update();
    }

    private void OnDrawGizmos()
    {
        if (currentState != null)
            currentState.OnDrawGizmos();
    }
}
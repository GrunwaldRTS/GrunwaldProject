using UnityEngine;
using UnityEngine.AI;

public abstract class UnitsState
{
    protected UnitsFSMBase b;
    protected Camera mainCamera;
    protected NavMeshAgent agent;
    public UnitsState(UnitsFSMBase b)
    {
        this.b = b;
        mainCamera = b.MainCamera;
        agent = b.Agent;
    }
    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}

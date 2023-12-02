using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : UnitState
{
    private Collider collider;
    private Rigidbody rBody;
    public IdleState(PlayerSFMBase b, Collider collider, Rigidbody rBody) : base(b)
    {
        this.collider = collider;
        this.rBody = rBody;
    }
    public override void Start()
    {
        Debug.Log("Idle State");
    }
    public override void Update()
    {
        ChangeToWalkingState();
        ChangeToJumpState();
    }
    public override void FixedUpdate()
    {

    }
    public override void Leave()
    {

    }
    private void ChangeToWalkingState()
    {
        if (InputManager.Instance.GetMoveDown())
        {
            b.SwitchState(b.WalkingState);
        }
    }
    private void ChangeToJumpState()
    {
        if (InputManager.Instance.GetJumpDown())
        {
            b.SwitchState(b.JumpingState);
        }
    }
}

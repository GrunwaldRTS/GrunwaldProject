using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingUnitState : UnitState
{
    private Collider collider;
    private Rigidbody rBody;
    public WalkingUnitState(PlayerSFMBase b, Collider collider, Rigidbody rBody) : base(b)
    {
        this.collider = collider;
        this.rBody = rBody;
    }
    public override void Start()
    {
        Debug.Log("Walking State");
    }
    public override void Update()
    {
        ChangeToIdleState();
        ChangeToJumpState();
    }
    public override void FixedUpdate()
    {
        ApplyAccelForce();
        ApplyDeccelForce();
    }
    public override void Leave()
    {
        
    }
    private void ApplyAccelForce()
    {
        Vector2 directionVector = InputManager.Instance.GetMove();
        //if (rBody.velocity.magnitude > b.WalkingSpeed) return;

        rBody.AddForce(b.transform.TransformDirection(new Vector3(directionVector.x, 0, directionVector.y) * b.WalkingAccelForce));
    }
    private void ApplyDeccelForce()
    {
        if (InputManager.Instance.GetMoveDown()) return;

        float xDeccelForce = -rBody.velocity.x * b.WalkingDeccelForce;
		float zDeccelForce = -rBody.velocity.z * b.WalkingDeccelForce;

        rBody.AddForce(new Vector3(xDeccelForce, 0, zDeccelForce));
    }
    private void ChangeToIdleState()
    {
        if (rBody.velocity.magnitude <= 0.01f)
        {
            b.SwitchState(b.IdleState);
        }
    }
    private void ChangeToJumpState()
    {
        if(b.IsGrounded() && InputManager.Instance.GetJumpDown())
        {
            b.SwitchState(b.JumpingState);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JumpState : UnitState
{
    private Collider collider;
    private Rigidbody rBody;
    public JumpState(PlayerSFMBase b, Rigidbody rBody, Collider collider) : base(b)
    {
        this.rBody = rBody;
        this.collider = collider;
    }
    public override void Start()
    {
        Debug.Log("Jump State");
        rBody.AddForce(new Vector3(0, b.JumpForce, 0), ForceMode.Impulse);
    }
    public override void Update()
    {
        ChangeToIdleState();
    }

    public override void FixedUpdate()
    {

    }

    public override void Leave()
    {

    }
    private void ChangeToIdleState()
    {
        if (b.IsGrounded())
        {
            b.SwitchState(b.IdleState);
        }
    }
}

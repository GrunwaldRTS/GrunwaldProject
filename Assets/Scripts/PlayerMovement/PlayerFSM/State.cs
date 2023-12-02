using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitState
{
    protected PlayerSFMBase b;
    public UnitState(PlayerSFMBase b)
    {
        this.b = b;
    }
    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Leave();
}

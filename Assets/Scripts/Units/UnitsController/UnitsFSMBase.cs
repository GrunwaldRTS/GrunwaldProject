using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitsFSMBase : MonoBehaviour
{
    public Camera MainCamera { get; private set; }
    public NavMeshAgent Agent { get; private set; }

	public UnitsState IdleState { get; private set; }
	UnitsState currentState;

	private void Start()
	{
		MainCamera = Camera.main;
		Agent = GetComponent<NavMeshAgent>();

		IdleState = new IdleUnitsState(this);

		currentState = IdleState;
		currentState.Start();
	}
	private void Update()
	{
		currentState.Update();
	}
	public void Switch(UnitsState state)
	{
		state.Exit();
		currentState = state;
		currentState.Start();
	}
}

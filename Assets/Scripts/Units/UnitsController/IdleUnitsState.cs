
using UnityEngine;
using UnityEngine.AI;

public class IdleUnitsState : UnitsState
{
    public IdleUnitsState(UnitsFSMBase b) : base(b)
    {
        
    }
    public override void Start()
	{

	}
	public override void Update()
	{
		if (InputManager.Instance.GetRightClickDown())
		{
			RaycastHit hit;
			Ray ray = mainCamera.ScreenPointToRay(InputManager.Instance.GetMousePosition());

			Debug.Log("casting");

			if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground")))
			{
				agent.SetDestination(hit.point);
			}
		}
	}
	public override void FixedUpdate()
	{
		
	}
	public override void Exit()
	{
		
	}
}

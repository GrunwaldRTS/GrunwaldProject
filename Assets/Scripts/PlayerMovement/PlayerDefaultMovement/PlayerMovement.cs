using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float walkingAccelForce;
	[SerializeField] private float walkingDeccelForce;
	[SerializeField] private float walkingSpeed;
	[SerializeField] private float height;

	private Rigidbody rBody;
	private void Awake()
	{
		rBody = GetComponent<Rigidbody>();
	}
    void Update()
    {
        ApplyAccelForce();
		ApplyDeccelForce();
		AdjustHeight();
    }
	private void ApplyAccelForce()
	{
		Vector2 directionVector = InputManager.Instance.GetMove();
		if (rBody.velocity.magnitude > walkingSpeed) return;

		rBody.AddForce(transform.TransformDirection(new Vector3(directionVector.x, 0, directionVector.y) * walkingAccelForce));
	}
	private void ApplyDeccelForce()
	{
		if (InputManager.Instance.GetMoveDown()) return;

		float xDeccelForce = -rBody.velocity.x * walkingDeccelForce;
		float zDeccelForce = -rBody.velocity.z * walkingDeccelForce;

		rBody.AddForce(new Vector3(xDeccelForce, 0, zDeccelForce));
	}
	private void AdjustHeight()
	{
		RaycastHit hit;
		Ray ray = new(transform.position, Vector3.down);

		if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground"))){
			float adjustValue = height - hit.distance;
			transform.position += new Vector3(0, adjustValue, 0);
		}
	}
}

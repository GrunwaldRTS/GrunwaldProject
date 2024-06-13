using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPathfindingAgent : MonoBehaviour
{
    AStarPathfindingAgent agent;
    private void Awake()
    {
        agent = GetComponent<AStarPathfindingAgent>();
    }
    void Update()
    {
        if (InputManager.Instance.GetLeftClickDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMousePosition());

            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.green, 10000f);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
            {
                Debug.Log("set destination");
                agent.SetDestination(hit.point);
            }
        }
    }
}

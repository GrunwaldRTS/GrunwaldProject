using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AgentController1 : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; }
    public bool FinishedPath { get; private set; } = false;
    bool terrainGenerated;
    public int Health { get; set; } = 100 ;
    bool gotUpdated = false;
    bool isAlive = true;
    private Coroutine rotationCoroutine;


    private float rotationSpeed = 0.05f;
    private Quaternion rotation = Quaternion.identity;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
	private void Start()
	{
        EventManager.OnChunkMeshesInstanced.AddListener(() => { terrainGenerated = true; });
	}
	private void Update()
    {
        if (!terrainGenerated) return;
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending && gotUpdated)
        {
            OnDestinationReached();
        }
    }
    IEnumerator Rotator(Quaternion targetRotation)
    {
        float t = 0;
        while (t <= 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            t += rotationSpeed * Time.deltaTime;

            yield return null;
        }
    }
    private void RotateTowards(Quaternion targetRotation)
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(Rotator(targetRotation));
    }
    public void MoveToDestination(Vector3 pos, Quaternion targetRotation)
    {
		Debug.Log($"warriorDestination: {pos}");
		agent.SetDestination(pos);
        Debug.Log($"endPath: {agent.nextPosition}");
        gotUpdated = true;
        FinishedPath = false;
        rotation = targetRotation;

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
    }
    private void OnDestinationReached()
    {
        RotateTowards(rotation);
        gotUpdated = false;
        FinishedPath = true;
    }
}
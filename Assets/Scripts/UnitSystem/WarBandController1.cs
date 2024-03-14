using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarBandController1 : MonoBehaviour
{
	[SerializeField] GameObject rectangleFormationPrefab;
	[SerializeField] GameObject NestRepPrefab;
	public int Capacity { get; private set; } = 10;
	public char GroupID { get; set; } = ' ';
	public int ArmyId { get; private set; }
	public int AttackMode { get; set; } = 1;
	Camera mainCamera;
	List<GameObject> warriors;
	GameObject formation;
	GameObject warbandBanner;
	GameObject army;
	Coroutine FinishedPathDetectorCoroutine;
	bool coroutineFinished = true;


	List<DestinationQueue> destinationQueue = new List<DestinationQueue>();

	struct DestinationQueue
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public bool isAttack { get; set; }
		public GameObject warbandToAttack { get; set; }
		public DestinationQueue(Vector3 pos, Quaternion rot, bool type = false, GameObject warband = null)
		{
			Position = pos;
			Rotation = rot;
			isAttack = type;
			warbandToAttack = warband;
		}
	}

    void OnMouseEnter()
    {
		Debug.Log("mouse entered");
		Outline script = transform.gameObject.GetComponent<Outline>();
		if (script.OutlineWidth == 3.5f)
		{
			script.OutlineWidth = 4f;
		}
		else
		{
			script.OutlineWidth = 2f;
		}
	}

	void OnMouseExit()
    {
		Outline script = transform.gameObject.GetComponent<Outline>();
		if (script.OutlineWidth == 4f)
		{
			script.OutlineWidth = 3.5f;
		}
		else
		{
			script.OutlineWidth = 0f;
		}
	}
    private void Awake()
	{
		mainCamera = Camera.main;
		warriors = GetWarriorsInWarBand();
		army = transform.parent.gameObject;
		ArmyId = army.GetComponent<ArmyManager>().ArmyId;
		formation = Instantiate(rectangleFormationPrefab);
	}
	public void GettingAttacked(GameObject warband)
    {
		int fightingWarriors = 0;
		foreach(GameObject warrior in warriors)
        {
            if (warrior.GetComponent<AgentController1>().isAttacking)
            {
				fightingWarriors++;
            }
        }
		if(fightingWarriors <= 5)
        {
			AttackWarband(warband);
        }
    }

	public void SetGroupID(char id)
	{
		GroupID = id;
		warbandBanner.GetComponent<BannerManager>().SetGroupIdDisplay(id);
	}
	public Vector3 GetBannerPosition()
    {
		return warbandBanner.transform.position;
    }

	List<GameObject> GetWarriorsInWarBand()
	{
		warriors = new List<GameObject>();

		int i = 0;
		foreach (Transform trans in transform)
		{
			if (trans.gameObject.name.Contains("Warrior"))
			{
				warriors.Add(trans.gameObject);
				i++;
			}
			else if (trans.gameObject.name.Contains("Banner"))
			{
				warbandBanner = trans.gameObject;
			}
		}

		return warriors;
	}
	public List<GameObject> GetWarriorsInWarBandByDistance(GameObject warband, Vector3 pos)
	{
		List<GameObject> warriorsByDistance = warband.GetComponent<WarBandController1>().GetWarriorsInWarBand();
		int n = warriorsByDistance.Count;
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < n-1; j++)
			{
				float distanceX = Vector3.Distance(transform.position, warriorsByDistance[j].transform.position); 
				float distanceY = Vector3.Distance(transform.position, warriorsByDistance[j + 1].transform.position);

				// Swap warriors if they are out of order
				if (distanceX > distanceY)
				{
					GameObject temp = warriorsByDistance[j];
					warriorsByDistance[j] = warriorsByDistance[j + 1];
					warriorsByDistance[j + 1] = temp;
				}
			}
		}
		return warriorsByDistance;
	}
	private void Update()
	{
		HoldBanner();
		warbandBanner.GetComponent<BannerManager>().SetHealthBar(GetHealthInfo());
		if (!IsMarching() && destinationQueue.Count > 0)
		{
            if (destinationQueue[0].isAttack)
            {
				AttackWarband(destinationQueue[0].warbandToAttack);
            }
            else
            {
				MoveFormation(destinationQueue[0].Position, destinationQueue[0].Rotation);
			}
			
			destinationQueue.RemoveAt(0);
		}
		int fallenWarriors = 0;
		foreach(GameObject warrior in warriors)
        {
			if (!warrior.GetComponent<AgentController1>().isAlive)
            {
				fallenWarriors++;
            }
        }
		if(fallenWarriors == Capacity)
        {
			Destroy(transform.gameObject);
        }

	}

	public void HoldBanner(float BannerScaleMultiplier = .001f, float BannerUpwardSpeed = .001f)
	{
		float distanceToCamera = Vector3.Distance(warbandBanner.transform.position, mainCamera.transform.position);
		float newScale = 0.4f + distanceToCamera * BannerScaleMultiplier;
		float upwardMovement = GetWarriorsPosition().y + distanceToCamera * BannerUpwardSpeed + 10;
		warbandBanner.transform.position = new Vector3(GetWarriorsPosition().x, upwardMovement, GetWarriorsPosition().z);
		warbandBanner.transform.localScale = new Vector3(newScale, newScale, newScale);
		Vector3 directionToCamera = (mainCamera.transform.position - warbandBanner.transform.position).normalized;
		Quaternion newRotation = Quaternion.LookRotation(directionToCamera);
		warbandBanner.transform.rotation = newRotation;
	}
	public float[] GetHealthInfo()
	{
		float[] healthArr = new float[Capacity];
		int i = 0;
		foreach (GameObject warrior in warriors)
		{
			healthArr[i] = warrior.GetComponent<AgentController1>().Health;
			i++;
		}
		System.Array.Sort(healthArr);
		System.Array.Reverse(healthArr);
		return healthArr;
	}
	public void SetDestination(Vector3 pos, Quaternion rotation)
	{
		destinationQueue.Clear();
		if (!coroutineFinished)
		{
			StopCoroutine(FinishedPathDetectorCoroutine);
			coroutineFinished = true;
		}
		MoveFormation(pos, rotation);
	}
	public void AttackWarband(GameObject enemyWarband)
    {
		List<GameObject> enemyWarriors = GetWarriorsInWarBandByDistance(enemyWarband,GetWarriorsPosition());
		int n = enemyWarriors.Count;
		for (int i = 0; i < warriors.Count; i++)
        {
			if(i >= (n / 2) && AttackMode == 1)
            {
				warriors[i].GetComponent<AgentController1>().Attack(enemyWarriors[i - 5]);
            }
            else
            {
				warriors[i].GetComponent<AgentController1>().Attack(enemyWarriors[i]);
			}
        }
	}
	public void AddPositionToQueue(Vector3 pos, Quaternion rotation)
	{
		destinationQueue.Add(new DestinationQueue(pos, rotation));
	}
	public void AddAttackToQueue(GameObject warband)
	{
		destinationQueue.Add(new DestinationQueue(Vector3.zero,Quaternion.identity,true,warband));
	}
	public Vector3 GetWarriorsPosition()
	{


		Vector3 positionSum = Vector3.zero;

		foreach (GameObject warrior in warriors)
		{
			positionSum += warrior.transform.position;
		}

		positionSum /= warriors.Count;

		return positionSum;
	}
	public Vector3 GetCurrentDestination()
	{
		if (destinationQueue.Count > 0)
		{
			return destinationQueue[destinationQueue.Count - 1].Position;
		}
		else return GetWarriorsPosition();
	}
	private Vector3[] GetWarriorsDestinations(Vector3 destinationPos, Quaternion rotation)
	{
		Vector3[] nestArray = new Vector3[Capacity];

		formation.transform.position = destinationPos;
		formation.transform.rotation = rotation;

		int i = 0;
		foreach (Transform nest in formation.transform)
		{
			if (nest.name.Contains("Nest"))
			{
				Vector3 rayCastDirection = Vector3.down;
				Vector3 nestPos = nest.position;
				Vector3 hitPosition = Vector3.zero;
				RaycastHit hit;
				//using raycasts fired from above to accurately get positions
				if (Physics.Raycast(nestPos, rayCastDirection, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
				{
					hitPosition = hit.point;
					GameObject newNestRep = Instantiate(NestRepPrefab, hitPosition, rotation);
				}
				nestArray[i] = hitPosition;
				i++;
			}
		}

		return nestArray;
	}
	IEnumerator FinishedPathDetector(float interval)
	{
		int finishedWarriors = 0;
		while (finishedWarriors < Capacity)
		{
			finishedWarriors = 0;
			foreach (GameObject warrior in warriors)
			{
				if (warrior.GetComponent<AgentController1>().FinishedPath)
				{
					finishedWarriors++;
				}
			}
			yield return new WaitForSeconds(interval);
		}
		Debug.Log("destination reached");
		coroutineFinished = true;
	}

	public void MoveFormation(Vector3 destinationPos, Quaternion rotation)
	{

		Vector3[] nestArray = GetWarriorsDestinations(destinationPos, rotation);
		int i = 0;
		foreach (GameObject warrior in warriors)
		{
			AgentController1 agentController = warrior.GetComponent<AgentController1>();
			agentController.MoveToDestination(NetworkManagement.Instance.GetInputState(nestArray[i], rotation));
			i++;
		}
		FinishedPathDetectorCoroutine = StartCoroutine(FinishedPathDetector(0.1f));
		coroutineFinished = false;
	}
	public bool IsMarching()
	{
		if (!coroutineFinished)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
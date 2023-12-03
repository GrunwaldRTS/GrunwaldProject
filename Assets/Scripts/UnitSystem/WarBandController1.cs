using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarBandController1 : MonoBehaviour
{
	[SerializeField] GameObject rectangleFormationPrefab;
	public int Capacity { get; private set; } = 10;
	public char GroupID { get; set; } = ' ';

	Camera mainCamera;
	GameObject[] warriors;
	GameObject formation;
	GameObject warbandBanner;
	Coroutine FinishedPathDetectorCoroutine;
	bool coroutineFinished = true;


	List<DestinationQueue> destinationQueue = new List<DestinationQueue>();

	struct DestinationQueue
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public DestinationQueue(Vector3 pos, Quaternion rot)
		{
			Position = pos;
			Rotation = rot;
		}
	}
	private void Awake()
	{
		mainCamera = Camera.main;
		warriors = GetWarriorsInWarBand();

		formation = Instantiate(rectangleFormationPrefab);
	}

	public void SetGroupID(char id)
	{
		GroupID = id;
		warbandBanner.GetComponent<BannerManager>().SetGroupIdDisplay(id);
	}

	GameObject[] GetWarriorsInWarBand()
	{
		warriors = new GameObject[Capacity];

		int i = 0;
		foreach (Transform trans in transform)
		{
			if (trans.gameObject.name.Contains("Warrior"))
			{
				warriors[i] = trans.gameObject;
				i++;
			}
			else if (trans.gameObject.name.Contains("Banner"))
			{
				warbandBanner = trans.gameObject;
			}
		}

		return warriors;
	}
	private void Update()
	{
		HoldBanner();
		warbandBanner.GetComponent<BannerManager>().SetHealthBar(GetHealthInfo());
		if (!IsMarching() && destinationQueue.Count > 0)
		{
			MoveFormation(destinationQueue[0].Position, destinationQueue[0].Rotation);
			destinationQueue.RemoveAt(0);
		}
	}

	public void HoldBanner(float BannerScaleMultiplier = .001f, float BannerUpwardSpeed = .001f)
	{
		float distanceToCamera = Vector3.Distance(warbandBanner.transform.position, mainCamera.transform.position);
		float newScale = 0.4f + distanceToCamera * BannerScaleMultiplier;
		float upwardMovement = 36 + distanceToCamera * BannerUpwardSpeed;
		warbandBanner.transform.position = new Vector3(GetWarriorsPosition().x, upwardMovement, GetWarriorsPosition().z);
		warbandBanner.transform.localScale = new Vector3(newScale, newScale, newScale);
		Vector3 directionToCamera = (mainCamera.transform.position - warbandBanner.transform.position).normalized;
		Quaternion newRotation = Quaternion.LookRotation(directionToCamera);
		warbandBanner.transform.rotation = newRotation;
	}
	public int[] GetHealthInfo()
	{
		int[] healthArr = new int[Capacity];
		int i = 0;
		foreach (GameObject warrior in warriors)
		{
			healthArr[i] = warrior.GetComponent<AgentController1>().Health;
			i++;
		}
		System.Array.Sort(healthArr);
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
	public void AddPositionToQueue(Vector3 pos, Quaternion rotation)
	{
		destinationQueue.Add(new DestinationQueue(pos, rotation));
	}
	public Vector3 GetWarriorsPosition()
	{


		Vector3 positionSum = Vector3.zero;

		foreach (GameObject warrior in warriors)
		{
			positionSum += warrior.transform.position;
		}

		positionSum /= warriors.Length;

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
				if (Physics.Raycast(nestPos, rayCastDirection, out hit, Mathf.Infinity))
				{
					hitPosition = hit.point;
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
			agentController.MoveToDestination(nestArray[i], rotation);
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
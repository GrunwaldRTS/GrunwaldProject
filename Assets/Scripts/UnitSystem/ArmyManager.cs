using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArmyManager : NetworkBehaviour
{
	[SerializeField] GameObject warbandPrefab;
	[SerializeField] GameObject rectangleArmyFormationPrefab;
	GameObject Formation;
	int maxArmySize = 10;
	public List<GameObject> ArmyWarBands { get; private set; }
	public int ArmyId;

	void Awake()
	{
		Formation = Instantiate(rectangleArmyFormationPrefab);
	}
    public override void OnNetworkSpawn()
    {
        ArmyId = (int)OwnerClientId;

		StartSpawnWarband();
    }
	void StartSpawnWarband()
	{
		if (!IsServer) return;

		GameObject go = Instantiate(warbandPrefab);
		NetworkObject obj = go.GetComponent<NetworkObject>();
		obj.SpawnWithOwnership(OwnerClientId, true);
		Debug.Log(obj.TrySetParent(gameObject, true));
	}
    private void Update()
    {
		ArmyWarBands = GetAllWarbands();
    }
    public List<GameObject> GetAllWarbands()
	{
		List<GameObject> allWarbands = new List<GameObject>();
		foreach (Transform trans in transform)
		{
            if (trans.gameObject.name.Contains("WarBand"))
            {
				allWarbands.Add(trans.gameObject);
			}
		}
		return allWarbands;
	}
	public void SetArmyDestination(Vector3 destinationPos, List<GameObject> warBands)
	{
		Vector3[] nestPositions = GetWarBandsDestinations(destinationPos, warBands, false);
		int i = 0;
		foreach (GameObject warBand in warBands)
		{
			WarBandController1 warBandController = warBand.GetComponent<WarBandController1>();
			warBandController.SetDestination(nestPositions[i], GetRotation(destinationPos, warBands, false));
			i++;
		}
	}

	public void SetArmyAttack(GameObject enemyWarband, List<GameObject> warBands)
    {
		foreach (GameObject warBand in warBands)
		{
			WarBandController1 warBandController = warBand.GetComponent<WarBandController1>();
			warBandController.AttackWarband(enemyWarband);
		}
	}

	public void AddArmyAttackToQueue(GameObject enemyWarband, List<GameObject> warBands)
	{
		foreach (GameObject warBand in warBands)
		{
			WarBandController1 warBandController = warBand.GetComponent<WarBandController1>();
			warBandController.AddAttackToQueue(enemyWarband);
			Debug.Log("added attack");
		}
	}

	public void SetArmyDestination(Vector3 destinationPos, List<GameObject> warBands, Quaternion givenRotation)
	{
		Vector3[] nestPositions = GetWarBandsDestinations(destinationPos, warBands, givenRotation);
		int i = 0;
		foreach (GameObject warBand in warBands)
		{
			WarBandController1 warBandController = warBand.GetComponent<WarBandController1>();
			warBandController.SetDestination(nestPositions[i], givenRotation);
			i++;
		}
	}

	public void AddArmyDestinationToQueue(Vector3 destinationPos, List<GameObject> warBands)
	{
		Vector3[] warBandsPositions = GetWarBandsDestinations(destinationPos, warBands, true);
		int i = 0;
		foreach (GameObject warBand in warBands)
		{
			WarBandController1 warBandController = warBand.GetComponent<WarBandController1>();
			warBandController.AddPositionToQueue(warBandsPositions[i], GetRotation(destinationPos, warBands, true));
			i++;
		}
	}

	public void AddArmyDestinationToQueue(Vector3 destinationPos, List<GameObject> warBands, Quaternion givenRotation)
	{
		Vector3[] warBandsPositions = GetWarBandsDestinations(destinationPos, warBands, givenRotation);
		int i = 0;
		foreach (GameObject warBand in warBands)
		{
			WarBandController1 warBandController = warBand.GetComponent<WarBandController1>();
			warBandController.AddPositionToQueue(warBandsPositions[i], givenRotation);
			i++;
		}
	}

	private Vector3 GetArmyPosition(List<GameObject> warBands, bool planned)
	{
		if (warBands.Count == 0) return Vector3.zero;

		Vector3 positionSum = Vector3.zero;
		foreach (GameObject warband in warBands)
		{
			WarBandController1 warBandController = warband.GetComponent<WarBandController1>();
			if (!planned)
			{
				positionSum += warBandController.GetWarriorsPosition();
			}
			else
			{
				positionSum += warBandController.GetCurrentDestination();
			}
		}

		positionSum /= warBands.Count;

		return positionSum;
	}

	private Quaternion GetRotation(Vector3 destinationPos, List<GameObject> warBands, bool planned)
	{
		destinationPos.y = 0;
		Vector3 currentPos = GetArmyPosition(warBands, planned);

		Vector3 forward = (destinationPos - currentPos).normalized;
		Quaternion rotation = Quaternion.LookRotation(forward);
		rotation.x = 0;
		rotation.z = 0;
		return rotation;
	}
	private Vector3[] GetWarBandsDestinations(Vector3 destinationPos, List<GameObject> warBands, bool planned)
	{
		Quaternion rotation = GetRotation(destinationPos, warBands, planned);
		destinationPos.y = 90;
		Vector3[] nestArray = new Vector3[maxArmySize];
		Formation.transform.position = destinationPos;
		Formation.transform.rotation = rotation;
		Transform trans = Formation.transform;
		int i = 0;
		foreach (Transform nest in trans)
		{
			if (nest.name.Contains("Nest"))
			{
				Vector3 nestPos = nest.position;
				nestArray[i] = nestPos;
				i++;
			}
		}
		return nestArray;
	}
	private Vector3[] GetWarBandsDestinations(Vector3 destinationPos, List<GameObject> warBands, Quaternion givenRotation)
	{
		Quaternion rotation = givenRotation;
		destinationPos.y = 90;
		Vector3[] nestArray = new Vector3[maxArmySize];
		Formation.transform.position = destinationPos;
		Formation.transform.rotation = rotation;
		Transform trans = Formation.transform;
		int i = 0;
		foreach (Transform nest in trans)
		{
			if (nest.name.Contains("Nest"))
			{
				Vector3 nestPos = nest.position;
				nestArray[i] = nestPos;
				i++;
			}
		}
		return nestArray;
	}
}
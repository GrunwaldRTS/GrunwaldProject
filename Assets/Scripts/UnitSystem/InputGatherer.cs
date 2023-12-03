using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputGatherer : MonoBehaviour
{
	public List<GameObject> selectedWarbands = new List<GameObject>();
	[SerializeField] private Camera mainCam;
	[SerializeField] private GameObject Army;
	ArmyManager armyManager;
	float SelectedOutlineWidth = 3.5f;
	void Awake()
	{
		armyManager = Army.GetComponent<ArmyManager>();
	}

	private void SelectAllWarbands()
	{
		foreach (Transform trans in Army.transform)
		{

			if (trans.gameObject.name.Contains("WarBand"))
			{
				SelectWarband(trans.gameObject);
			}

		}
	}

	private void SelectWarbandsByGroupID(char id)
	{
		DeselectAllWarbands();
		foreach (Transform trans in Army.transform)
		{
			if (trans.name.Contains("WarBand"))
			{
				char groupID = trans.gameObject.GetComponent<WarBandController1>().GroupID;
				if (groupID == id)
				{
					SelectWarband(trans.gameObject);
				}
			}
		}
	}

	private void DeselectAllWarbands()
	{
		foreach (GameObject warband in selectedWarbands)
		{
			warband.GetComponent<Outline>().OutlineWidth = 0f;
		}
		selectedWarbands.Clear();
	}

	private void SelectWarband(GameObject warband)
	{
		warband.GetComponent<Outline>().OutlineWidth = SelectedOutlineWidth;

		if (!selectedWarbands.Contains(warband))
		{
			selectedWarbands.Add(warband);
		}
	}

	private void DeselectWarband(GameObject warband)
	{
		warband.GetComponent<Outline>().OutlineWidth = 0f;
		if (selectedWarbands.Contains(warband))
		{
			selectedWarbands.Remove(warband);
		}
	}

	private GameObject GetParentObj(GameObject child)
	{
		return child.transform.parent.gameObject;
	}

	private void ResetWarBandGroupID(char groupID)
	{
		foreach (Transform trans in Army.transform)
		{
			if (trans.gameObject.name.Contains("WarBand"))
			{
				if (trans.gameObject.GetComponent<WarBandController1>().GroupID == groupID)
				{
					trans.gameObject.GetComponent<WarBandController1>().SetGroupID('#');
				}
			}
		}
	}

	private void SetSelecdtedWarBandsGroupID(char groupID)
	{
		ResetWarBandGroupID(groupID);
		foreach (GameObject warband in selectedWarbands)
		{
			Debug.Log(warband.name);
			warband.GetComponent<WarBandController1>().SetGroupID(groupID);
		}

	}

	// Update is called once per frame
	void Update()
	{
		// Debug.Log("update");
		if (InputManager.Instance.GetRightClickDown() && !InputManager.Instance.GetShiftHold())
		{
			Debug.Log("move");
			Ray MovePosition = mainCam.ScreenPointToRay(InputManager.Instance.GetMousePosition());
			List<GameObject> warBands = selectedWarbands;
			if (Physics.Raycast(MovePosition, out var hitInfo, float.MaxValue, LayerMask.GetMask("Ground")))
			{
				armyManager.SetArmyDestination(hitInfo.point, warBands);
			}
		}

		if (InputManager.Instance.GetRightClickDown() && InputManager.Instance.GetShiftHold())
		{
			Debug.Log("addmove");
			Ray MovePosition = mainCam.ScreenPointToRay(InputManager.Instance.GetMousePosition());
			List<GameObject> warBands = selectedWarbands;
			if (Physics.Raycast(MovePosition, out var hitInfo, float.MaxValue, LayerMask.GetMask("Ground")))
			{
				armyManager.AddArmyDestinationToQueue(hitInfo.point, warBands);
			}
		}

		if (InputManager.Instance.GetLeftClickDown() && !InputManager.Instance.GetShiftHold())
		{
			Debug.Log("check");
			Ray MovePosition = Camera.main.ScreenPointToRay(InputManager.Instance.GetMousePosition());

			if (Physics.Raycast(MovePosition, out var hitInfo, Mathf.Infinity))
			{
				GameObject hitObject = hitInfo.collider.gameObject;
				if (hitObject.name.Contains("Banner") || hitObject.name.Contains("Warrior"))
				{
					GameObject parentObj = GetParentObj(hitObject);
					DeselectAllWarbands();
					SelectWarband(parentObj);
				}
				else
				{
					DeselectAllWarbands();
				}
			}
		}

		if (InputManager.Instance.GetLeftClickDown() && InputManager.Instance.GetShiftHold())
		{
			Debug.Log("addToChecked");
			Ray MovePosition = Camera.main.ScreenPointToRay(InputManager.Instance.GetMousePosition());

			if (Physics.Raycast(MovePosition, out var hitInfo, Mathf.Infinity))
			{
				Debug.Log(hitInfo.point);
				GameObject hitObject = hitInfo.collider.gameObject;
				if (hitObject.name.Contains("Banner") || hitObject.name.Contains("Warrior"))
				{
					GameObject parentObj = GetParentObj(hitObject);
					SelectWarband(parentObj);
				}
			}
		}

		if (InputManager.Instance.GetShiftHold() && InputManager.Instance.GetADown())
		{
			SelectAllWarbands();
		}

		Dictionary<int, Func<bool>> keys = new()
		{
			{0, () => InputManager.Instance.Get0Down()},
			{1, () => InputManager.Instance.Get1Down()},
			{2, () => InputManager.Instance.Get2Down()},
			{3, () => InputManager.Instance.Get3Down()},
			{4, () => InputManager.Instance.Get4Down()},
			{5, () => InputManager.Instance.Get5Down()},
			{6, () => InputManager.Instance.Get6Down()},
			{7, () => InputManager.Instance.Get7Down()},
			{8, () => InputManager.Instance.Get8Down()},
			{9, () => InputManager.Instance.Get9Down()},
		};

		//sprawdzanie ka¿dego klawisza cyfrowego
		for (int i = 0; i <= 9; i++)
		{
			bool clicked = keys[i].Invoke();
			char charDigit = (char)(i + '0');
			if (InputManager.Instance.GetShiftHold())
			{
				Debug.Log(clicked);
				if (clicked)
				{
					Debug.Log($"clicked {i}");
					SetSelecdtedWarBandsGroupID(charDigit);
				}
			}
			else if (clicked)
			{
				Debug.Log($"clicked {i}");
				SelectWarbandsByGroupID(charDigit);
			}

		}
	}
}
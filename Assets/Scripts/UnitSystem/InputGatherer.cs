using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputGatherer : MonoBehaviour
{
	public List<GameObject> selectedWarbands = new List<GameObject>();
	[SerializeField] private Camera mainCam;
	[SerializeField] private GameObject Army;
	[SerializeField] private RectTransform boxVisual;
	Vector2 boxStartPos;
	Vector2 boxEndPos;
	Rect selectionBox;
	ArmyManager armyManager;
	float SelectedOutlineWidth = 3.5f;
	void Awake()
	{
		armyManager = Army.GetComponent<ArmyManager>();
		boxEndPos = Vector2.zero;
		boxStartPos = Vector2.zero;
		DrawBox();
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
		if (InputManager.Instance.GetRightClickDown() && !InputManager.Instance.GetShiftHold())
		{
			Ray MovePosition = mainCam.ScreenPointToRay(InputManager.Instance.GetMousePosition());
			List<GameObject> warBands = selectedWarbands;
			if (Physics.Raycast(MovePosition, out var hitInfo, float.MaxValue, LayerMask.GetMask("Ground")))
			{
				armyManager.SetArmyDestination(hitInfo.point, warBands);
			}
		}

		if (InputManager.Instance.GetRightClickDown() && InputManager.Instance.GetShiftHold())
		{
			Ray MovePosition = mainCam.ScreenPointToRay(InputManager.Instance.GetMousePosition());
			List<GameObject> warBands = selectedWarbands;
			if (Physics.Raycast(MovePosition, out var hitInfo, float.MaxValue, LayerMask.GetMask("Ground")))
			{
				armyManager.AddArmyDestinationToQueue(hitInfo.point, warBands);
			}
		}

		if (InputManager.Instance.GetLeftClickDown() && !InputManager.Instance.GetShiftHold())
		{
			boxStartPos = InputManager.Instance.GetMousePosition();
			selectionBox = new Rect();
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
			boxStartPos = InputManager.Instance.GetMousePosition();
			selectionBox = new Rect();
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

        if (InputManager.Instance.GetLeftClickHold())
        {
			boxEndPos = InputManager.Instance.GetMousePosition();
			DrawBox();
		}

		if (InputManager.Instance.GetLeftClickUp())
		{
			DrawBox();
			DrawSelection();
			boxStartPos = Vector2.zero;
			boxEndPos = Vector2.zero;
			DrawBox();
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
	private void DrawBox()
	{
		Vector2 boxStart = boxStartPos;
		Vector2 boxEnd = boxEndPos;
		Vector2 boxCenter = (boxStart + boxEnd) / 2;
		boxVisual.position = boxCenter;
		Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x-boxEnd.x),Mathf.Abs(boxStart.y-boxEnd.y));
		boxVisual.sizeDelta = boxSize;
	}
	private void DrawSelection()
    {
        //calculating X
		if (InputManager.Instance.GetMousePosition().x < boxStartPos.x)
        {
			//dragging left
			selectionBox.xMin = InputManager.Instance.GetMousePosition().x;
			selectionBox.xMax = boxStartPos.x;
		}
        else
        {
			selectionBox.xMin = boxStartPos.x;
			selectionBox.xMax = InputManager.Instance.GetMousePosition().x;
		}
		//calculating Y
		if (InputManager.Instance.GetMousePosition().y < boxStartPos.y)
		{
			selectionBox.yMin = InputManager.Instance.GetMousePosition().y;
			selectionBox.yMax = boxStartPos.y;
		}
		else
		{
			selectionBox.yMin = boxStartPos.y;
			selectionBox.yMax = InputManager.Instance.GetMousePosition().y;
		}
		SelectUnitsInBox();
	}
	private void SelectUnitsInBox()
    {
		
		foreach (Transform trans in Army.transform)
		{
			if (trans.gameObject.name.Contains("WarBand"))
			{
				if (selectionBox.Contains(mainCam.WorldToScreenPoint(trans.gameObject.GetComponent<WarBandController1>().GetBannerPosition())))
                {
					SelectWarband(trans.gameObject);
				}
			}
		}
	}

	
}
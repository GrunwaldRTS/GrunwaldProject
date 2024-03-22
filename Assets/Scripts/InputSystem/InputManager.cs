using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class InputManager : Singelton<InputManager>
{
    private Input input;
    private void Awake()
    {
        input = new();
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    public bool GetMoveDown()
    {
        Vector2 directionVector = input.Player.Move.ReadValue<Vector2>();
        return Mathf.Abs(directionVector.x) > 0.1f || Mathf.Abs(directionVector.y) > 0.1f;
    }
    public Vector2 GetMove()
    {
        return input.Player.Move.ReadValue<Vector2>();
    }
    public bool GetJumpDown()
    {
        return input.Player.Jump.triggered;
    }
    public bool GetCrouchDown()
    {
        return input.Player.Crouch.triggered;
    }
    public bool GetSprintDown()
    {
        return input.Player.Sprint.triggered;
    }
    public Vector2 GetMouseDelta()
    {
        return input.Player.Rotation.ReadValue<Vector2>();
    }
    public bool GetRightClickDown()
    {
        return input.Player.MouseRightDown.triggered;
    }
    public bool GetRightClickUp()
    {
        return input.Player.MouseRightUp.triggered;
    }
    public bool GetRightClickHold()
    {
        return input.Player.MouseRightDown.ReadValue<float>() > 0;
    }
    public bool GetLeftClickDown()
    {
        return input.Player.MouseLeftDown.triggered;
    }
	public bool GetLeftClickHold()
	{
		return input.Player.MouseLeftDown.ReadValue<float>() > 0;
	}
    public bool GetLeftClickUp()
    {
        return input.Player.MouseLeftUp.triggered;
    }
	public Vector2 GetMousePosition()
    {
        return input.Player.MousePosition.ReadValue<Vector2>();
    }
    public bool GetShiftDown()
    {
        return input.Player.ShiftKey.triggered;
    }
	public bool GetShiftHold()
	{
        //return input.Player.ShiftKey.ReadValue<float>() > 0;
        return false;
	}
	public bool GetADown()
    {
        return input.Player.AKey.triggered;
    }
    public bool Get0Down()
    {
        return input.Player._0Key.triggered;
    }
	public bool Get1Down()
	{
		return input.Player._1Key.triggered;
	}
	public bool Get2Down()
	{
		return input.Player._2Key.triggered;
	}
	public bool Get3Down()
	{
		return input.Player._3Key.triggered;
	}
	public bool Get4Down()
	{
		return input.Player._4Key.triggered;
	}
	public bool Get5Down()
	{
		return input.Player._5Key.triggered;
	}
	public bool Get6Down()
	{
		return input.Player._6Key.triggered;
	}
	public bool Get7Down()
	{
		return input.Player._7Key.triggered;
	}
	public bool Get8Down()
	{
		return input.Player._8Key.triggered;
	}
	public bool Get9Down()
	{
		return input.Player._9Key.triggered;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFMBase : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject _player;

    [Header("Movement")]
    [SerializeField] private float _walkingAccelForce;
    [SerializeField] private float _walkingDeccelForce;
    [SerializeField] private float _walkingSpeed;
    [SerializeField] private float _runningSpeed;
    [SerializeField] private float _crouchingSpeed;
    [SerializeField] private float _jumpForce;
    private new CapsuleCollider collider;
    private Rigidbody rBody;

    [Header("Shotting")]
    [SerializeField] private float _mouseSensivity;
    [SerializeField] private Camera camera;

    private UnitState currentState;
    //player
    public GameObject Player { get => _player; private set => _player = value; }
    //movement
    public float WalkingAccelForce { get => _walkingAccelForce; private set => _walkingAccelForce = value; }
    public float WalkingDeccelForce { get => _walkingDeccelForce; private set => _walkingDeccelForce = value; }
    public float WalkingSpeed { get => _walkingSpeed; private set => _walkingSpeed = value; }
    public float RunningSpeed { get => _runningSpeed; private set => _runningSpeed = value; }
    public float CrouchingSpeed { get => _crouchingSpeed; private set => _crouchingSpeed = value; }
    public float JumpForce { get => _jumpForce; private set => _jumpForce = value; }
    //shotting
    public float MouseSensitivity { get => _mouseSensivity; private set => _mouseSensivity = value; }
    public UnitState IdleState { get; private set; }
    public UnitState WalkingState { get; private set; }
    public UnitState RunningState { get; private set; }
    public UnitState JumpingState { get; private set; }
    public UnitState CrouchingState { get; private set; }

    private void Start()
    {
        collider = Player.GetComponent<CapsuleCollider>();
        rBody = Player.GetComponent<Rigidbody>();
        //camera = Player.GetComponentInChildren<Camera>();

        IdleState = new IdleState(this, collider, rBody);
        WalkingState = new WalkingUnitState(this, collider, rBody);
        RunningState = new RunningState(this);
        JumpingState = new JumpState(this, rBody, collider);
        CrouchingState = new CrouchingState(this);

        currentState = IdleState;
        currentState.Start();
    }
    private void Update()
    {
        ApplyMouseRotation();
        currentState.Update();
    }
    private void FixedUpdate()
    {
        currentState.FixedUpdate();
    }
    public void SwitchState(UnitState state)
    {
        currentState.Leave();
        currentState = state;
        currentState.Start();
    }
    public bool IsGrounded()
    {
        Ray ray = new Ray(transform.TransformPoint(collider.center + new Vector3(0, -collider.height * 0.5f + 0.05f, 0)), Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * 0.2f, Color.red);
        return Physics.Raycast(ray, 0.2f);
    }
    private void ApplyMouseRotation()
    {
        Vector2 mouseInput = InputManager.Instance.GetMouseDelta();
        transform.rotation *= Quaternion.Euler(MouseSensitivity * Time.deltaTime * new Vector3(0, mouseInput.x, 0));
        camera.transform.rotation *= Quaternion.Euler(MouseSensitivity * Time.deltaTime * new Vector3(-mouseInput.y, 0, 0));
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rigid;
    private GameObject _camera;

    [Header("Movement")]
    [SerializeField] private float _moveForce; // force moving player
    [SerializeField] private float _maxSpeed; // limit max speed of player
    [SerializeField] private float _jumpForce; // force jumping
    [SerializeField] private float _groundDrag; // prevent slippery
    [SerializeField] private float _airMultiplier; // lower force when player is not grounded

    private float _horizontalInput;
    private float _verticalInput;
    private float _mouseInputX;
    private float _mouseInputY;

    [Header("Control")]
    // mouse sensetivity
    public float sensetivityX;
    public float sensetivityY;
    // determine camera rotation
    private float _accumRotationX = 0;
    private float _accumRotationY = 0;
    private float _rotationLimitY = 80;

    private bool _isGrounded = true;


    void Start()
    {
        _rigid = GetComponent<Rigidbody>();
        _camera = GameObject.Find("PlayerCamera");
    }

    private void FixedUpdate() {
        MovePlayer();
    }

    void Update()
    {
        HandleInput();
        ControlSpeed();
        GroundCheck();
        HandleDrag();
    }

    private void HandleInput() {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _mouseInputX = Input.GetAxis("Mouse X");
        _mouseInputY = Input.GetAxis("Mouse Y");

        // jump
        if(_isGrounded && Input.GetButtonDown("Jump")) {
            Jump();
        }
    
        // player rotation
        transform.eulerAngles = new Vector3(0, _accumRotationX, 0);
        _accumRotationX += Time.deltaTime * _mouseInputX * sensetivityX;
        // camera rotation
        _accumRotationY += Time.deltaTime * _mouseInputY * sensetivityY;
        _accumRotationY = Math.Clamp(_accumRotationY, -_rotationLimitY, _rotationLimitY);

        _camera.transform.eulerAngles = new Vector3(-_accumRotationY, _accumRotationX, 0);
    }
    private void ControlSpeed()
    {
        Vector3 flatVel = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > _maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _maxSpeed;
            _rigid.velocity = new Vector3(limitedVel.x, _rigid.velocity.y, limitedVel.z);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        Vector3 moveDirection = transform.forward * _verticalInput + transform.right * _horizontalInput;

        // on ground
        if(_isGrounded)
            _rigid.AddForce(moveDirection.normalized * _moveForce, ForceMode.Force);

        // in air
        else if(!_isGrounded)
            _rigid.AddForce(moveDirection.normalized * _moveForce * _airMultiplier, ForceMode.Force);
    }

    private void Jump() {
        // reset y velocity
        _rigid.velocity = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);
        _rigid.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        _isGrounded = false;
    }
    
    private void GroundCheck() {
        if(Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f)) {
            _isGrounded = true;
        } else {
            _isGrounded = false;
        }
    }
    
    private void HandleDrag() {
        if(_isGrounded) {
            _rigid.drag = _groundDrag;
        } else {
            _rigid.drag = 0;
        }
    }

    // private void OnCollisionEnter(Collision other) {
    //     if(other.contacts[0].normal.y > 0.7f) {
    //         _isGrounded = true;
    //     }
    // }
}
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
    [SerializeField] private float _jumpMultiplier; // adjust the jump force when the gravity low skill is active

	private float _horizontalInput;
    private float _verticalInput;
    private float _mouseInputX;
    private float _mouseInputY;

    [Header("Control")]
    // mouse sensetivity
    public float sensetivityX;
    public float sensetivityY;
	[SerializeField] private KeyCode _viewResetKey;
	private float _sensitivityMultiplier = 0.5f;
	[SerializeField] private float _sensitivityMultiplierMin = 0.5f;
	[SerializeField] private float _sensitivityMultiplierMax = 1.5f;

    // determine camera rotation
    private float _accumRotationX = 0;
    private float _accumRotationY = 0;
    private float _rotationLimitY = 80;

    private bool _isGrounded = true;
    private PlayerController _playerController;
    public float _moveSpeedGun;
    public float _maxSpeedGun;

	[Header("Footstep Audio Settings")]
	[SerializeField] private AudioSource _footstepAudioSource;
	[SerializeField] private AudioClip _footstepClip;

	// 이전 프레임에서 발소리 재생 조건을 충족하고 있었는지 추적하는 변수
	private bool _wasFootstepConditionMet = false;



	void Start() {
        _rigid = GetComponent<Rigidbody>();
        _camera = GameObject.Find("PlayerCamera");
        _playerController = GetComponent<PlayerController>();
        _maxSpeedGun = _maxSpeed;
    }

    private void FixedUpdate() {
        MovePlayer();
		Vector3 flatVelocity = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);
		_moveSpeedGun = flatVelocity.magnitude;
	}

    void Update() {
        HandleInput();
        ControlSpeed();
        GroundCheck();
        HandleDrag();
		HandleFootsteps();
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
    
		// reset view
		if (Input.GetKeyDown(_viewResetKey))
		{
			_accumRotationY = 0f;
		}

        // player rotation
        transform.eulerAngles = new Vector3(0, _accumRotationX, 0);
        _accumRotationX += Time.deltaTime * _mouseInputX * sensetivityX * _sensitivityMultiplier;
        // camera rotation
        _accumRotationY += Time.deltaTime * _mouseInputY * sensetivityY * _sensitivityMultiplier;
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
        float jumpMultiplier = (_playerController._isGravityLow) ? _jumpMultiplier : 1f;
        _rigid.velocity = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);
        _rigid.AddForce(transform.up * _jumpForce * jumpMultiplier, ForceMode.Impulse);
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

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
    }

    // private void OnCollisionEnter(Collision other) {
    //     if(other.contacts[0].normal.y > 0.7f) {
    //         _isGrounded = true;
    //     }
    // }

	public void SetSensitivityMultiplier(int percentage)
	{
		_sensitivityMultiplier = _sensitivityMultiplierMin + (_sensitivityMultiplierMax - _sensitivityMultiplierMin) * ((float)percentage) / 100;
	}

	private void HandleFootsteps()
	{
		Vector3 flatVel = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);
		bool hasInput = Mathf.Abs(_horizontalInput) > 0.1f || Mathf.Abs(_verticalInput) > 0.1f;
		bool isMoving = flatVel.magnitude > 0.1f;

		// 조건: 바닥에 있고, 플레이어가 이동 키 입력 중이며, 일정 속도 이상 이동 중일 때
		bool currentCondition = (_isGrounded && isMoving && hasInput);

		// 이전 프레임에는 조건을 만족하지 않았는데 이번 프레임에 조건을 만족하게 되었다면 한번 재생
		if (currentCondition && !_wasFootstepConditionMet)
		{
			if (_footstepAudioSource != null && _footstepClip != null)
			{
				_footstepAudioSource.PlayOneShot(_footstepClip);
			}
		}

		// 현재 프레임 조건 상태를 기록
		_wasFootstepConditionMet = currentCondition;
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
	private Rigidbody _rigid;
	private GameObject _camera;

	public float moveForce;
	public float maxSpeed;
	public float jumpForce;
	public float groundDrag;
	public float airMultiplier;

	private float _horizontalInput;
	private float _verticalInput;
	private float _mouseInputX;
	private float _mouseInputY;

	public float sensetivityX;
	public float sensetivityY;
	private float _accumRotationX = 0;
	private float _accumRotationY = 0;
	private float _rotationLimitY = 80;

	private bool _isGrounded = true;

	public TextMeshProUGUI text;

	public PlayerHpUI playerHpUI;

	void Start()
	{
		_rigid = GetComponent<Rigidbody>();
		_camera = GameObject.Find("PlayerCamera");
		playerHpUI = FindObjectOfType<PlayerHpUI>();
	}

		private void FixedUpdate()
	{
		MovePlayer();
	}

	void Update()
	{
		HandleInput();
		SpeedControl();

		// handle drag
		if (_isGrounded)
		{
			_rigid.drag = groundDrag;
		}
		else
		{
			_rigid.drag = 0;
		}
		text.text = "" + _rigid.velocity.magnitude;
	}

	public void OnHit()
	{
		playerHpUI.UpdateHP(playerHpUI.currentHp - 5);
		if (playerHpUI.currentHp <= 0)
		{
			Destroy(gameObject);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.contacts[0].normal.y > 0.7f)
		{
			_isGrounded = true;
		}
	}
	private void HandleInput()
	{
		_horizontalInput = Input.GetAxisRaw("Horizontal");
		_verticalInput = Input.GetAxisRaw("Vertical");
		_mouseInputX = Input.GetAxis("Mouse X");
		_mouseInputY = Input.GetAxis("Mouse Y");

		// jump
		if (_isGrounded && Input.GetButtonDown("Jump"))
		{
			Jump();
		}

		// player rotation
		transform.eulerAngles = new Vector3(0, _accumRotationX, 0);
		_accumRotationX += _mouseInputX * sensetivityX;
		// camera rotation
		_accumRotationY += _mouseInputY * sensetivityY;
		_accumRotationY = Math.Clamp(_accumRotationY, -_rotationLimitY, _rotationLimitY);

		_camera.transform.eulerAngles = new Vector3(-_accumRotationY, _accumRotationX, 0);
	}
	private void SpeedControl()
	{
		Vector3 flatVel = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);

		// limit velocity if needed
		if (flatVel.magnitude > maxSpeed)
		{
			Vector3 limitedVel = flatVel.normalized * maxSpeed;
			_rigid.velocity = new Vector3(limitedVel.x, _rigid.velocity.y, limitedVel.z);
		}
	}

	private void MovePlayer()
	{
		// calculate movement direction
		Vector3 moveDirection = transform.forward * _verticalInput + transform.right * _horizontalInput;

		// on ground
		if (_isGrounded)
			_rigid.AddForce(moveDirection.normalized * moveForce, ForceMode.Force);

		// in air
		else if (!_isGrounded)
			_rigid.AddForce(moveDirection.normalized * moveForce * airMultiplier, ForceMode.Force);
	}

	private void Jump()
	{
		// reset y velocity
		_rigid.velocity = new Vector3(_rigid.velocity.x, 0f, _rigid.velocity.z);
		_rigid.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		_isGrounded = false;
	}
}
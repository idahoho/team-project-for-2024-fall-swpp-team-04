using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rigid;
    private GameObject playerCamera;

    public float sensetivityX;
    public float sensetivityY;
    public float moveForce;
    public float maxSpeed;
    public float jumpForce;

    private float horizontalInput;
    private float verticalInput;
    private float mouseInputX;
    private float mouseInputY;
    public float mouseInputScroll;

    private float accumX = 0;
    private float accumY = 0;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        playerCamera = GameObject.Find("PlayerCamera");
    }

    private void FixedUpdate() {
        if(rigid.velocity.magnitude < maxSpeed) {
            rigid.AddForce((transform.forward * verticalInput + transform.right * horizontalInput).normalized * moveForce, ForceMode.Impulse);
        }
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        mouseInputX = Input.GetAxis("Mouse X");
        mouseInputY = Input.GetAxis("Mouse Y");
        mouseInputScroll = Input.GetAxis("Mouse ScrollWheel");
        if(Input.GetButtonDown("Jump")) {
            rigid.AddForce(new Vector3(0, -rigid.velocity.y, 0) * rigid.mass + Vector3.up * jumpForce, ForceMode.Impulse);
        }
        accumX += mouseInputX * Time.deltaTime * sensetivityX;
        accumY += mouseInputY * Time.deltaTime * sensetivityY;

        accumY = Math.Clamp(accumY, -80, 80);

        transform.eulerAngles = new Vector3(0, accumX, 0);
        playerCamera.transform.eulerAngles = new Vector3(-accumY, accumX, 0);
    }
}

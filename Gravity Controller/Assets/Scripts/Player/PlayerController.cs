using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //
    private Rigidbody _rigidbody;

    [SerializeField] Image _energyBar;

    // player status
    public int hp;
    [SerializeField] private float _energy;
    [SerializeField] private float _maxEnergy;
    [SerializeField] private float _energyRecover;
    private bool _isAlive;

    // to set direction of raycast
    private Transform _camera;
    // fire gun(left click)
    private bool _isShootable;
    private bool _isReloading;
    [SerializeField] private KeyCode _reloadKey;
    [SerializeField] private int _bulletLeft;
    [SerializeField] private int _maxBullet;
    [SerializeField] private float _shootCooldown;
    [SerializeField] private float _reloadCooldown;

    // local gravity control(right click)
    private bool _isTargetable;
    [SerializeField] private float _targetGravityCooldown;
    [SerializeField] private float _gravityForceTarget;

    // global gravity control(mouse wheel)
    [SerializeField] private float _mouseInputWheel;
    [SerializeField] private float _wheelInputThreshold;
    [SerializeField] private float _gravityForceHigh;
    [SerializeField] private float _gravityForceLow;
    private bool _isGravityLow;
    [SerializeField] private float _energyLow;
    

    void Start()
    {
        _camera = GameObject.Find("PlayerCamera").transform;
        _rigidbody = GetComponent<Rigidbody>();
        _isAlive = true;
        _bulletLeft = _maxBullet;
        _isShootable = true;
        _isReloading = false;
        _isTargetable = true;
        _energy = _maxEnergy;
        _isGravityLow = false;
    }
    private void FixedUpdate() {
        if(_isGravityLow) {
            _rigidbody.AddForce(Vector3.down * _gravityForceLow, ForceMode.Impulse);
        }
    }
    void Update()
    {
        _energyBar.fillAmount = _energy/_maxEnergy;
        // left click event
        if(Input.GetButtonDown("Fire1")) {
            Fire();
        }
        // right click event
        if(Input.GetButtonDown("Fire2")) {
            TargetGravity();
        }
        // reload key event
        if(Input.GetKeyDown(_reloadKey)) {
            Reload();
        }
        HandleWheelInput();
        UpdateEnergy();
    }

    private void Fire() {
        if(!_isShootable) {
            return;
        }
        if(_bulletLeft > 0) {
            _isShootable = false;
            Debug.Log("fire");
            RaycastHit hit;
            if(Physics.Raycast(_camera.position, _camera.transform.forward, out hit)) {
                if(hit.collider.gameObject.CompareTag("Enemy")) {
                    Debug.Log("Fire: enemy detected");
                    // hit.collider.gameObject.GetComponent<EnemyController>().OnHit();
                }
            }
            StartCoroutine(ReShootable());
        } else {
            Reload();
        }
    }

    private void Reload() {
        // Debug.Log("reloading...");
        _isReloading = true;
        _isShootable = false;
        StartCoroutine(ReShootable());
    }

    IEnumerator ReShootable() {
        if(_isReloading) {
            yield return new WaitForSeconds(_reloadCooldown);
            _isReloading = false;
            _bulletLeft = _maxBullet;
        } else {
            yield return new WaitForSeconds(_shootCooldown);
        }
        if(!_isReloading)
            _isShootable = true;
    }

    private void TargetGravity() {
        if(!_isTargetable) {
            return;
        }
        _isTargetable = false;
        RaycastHit hit;
        Rigidbody rigid;
        if(Physics.Raycast(_camera.position, _camera.transform.forward, out hit)) {
            if(rigid = hit.collider.gameObject.GetComponent<Rigidbody>()) {
                Debug.Log("TargetGravity: target detected");
                rigid.AddForce(Vector3.down * rigid.mass * _gravityForceTarget, ForceMode.Impulse);
                // hit.collider.gameObject.GetComponent<EnemyController>().OnHit();
            }
        }
        StartCoroutine(ReTargetable());
    }

    IEnumerator ReTargetable() {
        yield return new WaitForSeconds(_targetGravityCooldown);
        if(!_isTargetable)
            _isTargetable = true;
    }

    private void HandleWheelInput() {
        _mouseInputWheel = Input.GetAxis("Mouse ScrollWheel");
        if(_mouseInputWheel > _wheelInputThreshold) {
            _isGravityLow = true;
        } else if(_mouseInputWheel < -_wheelInputThreshold) {
            if(_isGravityLow) {
                _isGravityLow = false;
            }
            // TODO: gravity high mechanism
        }
    }

    private void UpdateEnergy() {
        if(_isGravityLow) {
            _energy -= _energyLow * Time.deltaTime;
            if(_energy < 0) {
                _energy = 0;
                _isGravityLow = false;
            }
        } else {
            _energy += _energyRecover * Time.deltaTime;
            if(_energy > _maxEnergy) _energy = _maxEnergy;
        }
    }

    public void OnHit() {
        if(--hp <= 0) _isAlive = false;
    }
}

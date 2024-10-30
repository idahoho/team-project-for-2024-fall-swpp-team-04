using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int hp;
    private bool _isAlive;

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
    [SerializeField] private float _gravityForce;

    void Start()
    {
        _camera = GameObject.Find("PlayerCamera").transform;
        _isAlive = true;
        _bulletLeft = _maxBullet;
        _isShootable = true;
        _isReloading = false;
        _isTargetable = true;
    }

    void Update()
    {
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
                rigid.AddForce(Vector3.down * rigid.mass * _gravityForce, ForceMode.Impulse);
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

    public void OnHit() {
        if(--hp <= 0) _isAlive = false;
    }
}

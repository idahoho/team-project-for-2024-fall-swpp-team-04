using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int hp;
    private bool _isAlive = true;

    private Transform _camera;
    [SerializeField] private KeyCode _reloadKey;
    [SerializeField] private int _bulletLeft;
    [SerializeField] private int _maxBullet;
    [SerializeField] private bool _shootable;
    [SerializeField] private float _shootCooldown;
    [SerializeField] private float _reloadCooldown;

    [SerializeField] AudioSource _audio;

    void Start()
    {
        _camera = GameObject.Find("PlayerCamera").transform;
        _bulletLeft = _maxBullet;
        _shootable = true;
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1") && _shootable && _bulletLeft > 0) {
            _bulletLeft--;
            Fire();
        }
        if(Input.GetKeyDown(_reloadKey)) {
            Reload();
        }
    }

    private void Fire() {
        _audio.Play();
        _shootable = false;
        Debug.Log("fire");
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.transform.forward, out hit)) {
            if(hit.collider.gameObject.CompareTag("Enemy")) {
                Debug.Log("enemy detected");
                // hit.collider.gameObject.GetComponent<EnemyController>().OnHit();
            }
        }
        Invoke("Reshootable_reload", _shootCooldown);
    }

    private void Reload() {
        Debug.Log("reloading...");
        _shootable = false;
        Invoke("Reshootable_reload", _reloadCooldown);
    }

    private void ReShootable_shoot() {
        _shootable = true;
    }

    private void ReShootable_reload() {
        _bulletLeft = _maxBullet;
        _shootable = true;
    }

    public void OnHit() {
        if(--hp <= 0) _isAlive = false;
    }
}

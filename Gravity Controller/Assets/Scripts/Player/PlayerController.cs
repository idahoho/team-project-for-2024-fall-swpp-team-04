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
    // for testing
    [SerializeField] private Image _energyBar;
    [SerializeField] private GameManager _gameManager;

    [Header("Player Status")]
    public int hp;
    [SerializeField] private float _energy;
    [SerializeField] private float _maxEnergy;
    [SerializeField] private float _energyRecover;
    private bool _isAlive;

    [Header("Attack")]
    [SerializeField] private KeyCode _reloadKey;
    [SerializeField] private int _bulletLeft;
    [SerializeField] private int _maxBullet;
    [SerializeField] private float _shootCooldown;
    [SerializeField] private float _reloadCooldown;
    // to set direction of raycast
    private Transform _camera;
    // fire gun(left click)
    private bool _isShootable;
    private bool _isReloading;

    [Header("Local Gravity Ability")]
    // local gravity control(right click)
    [SerializeField] private float _gravityCooldownLocal;
    [SerializeField] private float _gravityForceLocal;
    [SerializeField] private float _energyCostLocal;
    private bool _isTargetable;

    [Header("Global Gravity Ability")]
    // global gravity control(mouse wheel)
    [SerializeField] private float _mouseInputWheel;
    [SerializeField] private float _wheelInputThreshold;
    [SerializeField] private float _gravityForceHigh;
    [SerializeField] private float _gravityForceLow;
    [SerializeField] private float _energyCostLow;
    [SerializeField] private float _energyCostHigh;
    private bool _isGravityLow;
    

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
        // 낮아진 전체 중력에 대한 처리
        // 플레이어에게만 영향을 주도록 할 경우 이걸로 사용
        // 전체적으로 영향을 주도록 하려면 Physics.gravity 자체를 바꿔줘야 함
        if(_isGravityLow) {
            _rigidbody.AddForce(Physics.gravity * (_gravityForceLow - 1f), ForceMode.Impulse);
        }
    }
    void Update()
    {
        if(_isAlive) {
            _energyBar.fillAmount = _energy/_maxEnergy;
            HandleButtonInput();
            HandleWheelInput();
            UpdateEnergy();
        }
    }

    // 마우스 좌클릭, 우클릭, 재장전 입력 처리
    private void HandleButtonInput() {
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
    // 마우스 휠 입력 처리
    // _wheelInputThreshold 값을 기준으로 입력 판정 여부 판단
    private void HandleWheelInput() {
        _mouseInputWheel = Input.GetAxis("Mouse ScrollWheel");
        if(_mouseInputWheel > _wheelInputThreshold) {
            _isGravityLow = true;
        } else if(_mouseInputWheel < -_wheelInputThreshold) {
            if(_isGravityLow) {
                _isGravityLow = false;
            } else {
                GlobalGravity(_gameManager.GetActiveEnemies());
            }
        }
    }


    // 중력 능력을 사용 중이지 않을 때, 매 초마다 _energyRecover만큼 에너지를 회복한다.
    private void UpdateEnergy() {
        if(_isGravityLow) {
            _energy -= _energyCostLow * Time.deltaTime;
            if(_energy < 0) {
                _energy = 0;
                _isGravityLow = false;
            }
        } else {
            _energy += _energyRecover * Time.deltaTime;
            if(_energy > _maxEnergy) _energy = _maxEnergy;
        }
    }

    // 좌클릭 입력 처리(총)
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
                    // 여기에서 맞은 대상의 오브젝트 가져올 수 있음
                    hit.collider.gameObject.GetComponent<EnemyController>().OnHit();
                }
            }
            StartCoroutine(ReShootable());
        } else {
            Reload();
        }
    }

    // 재장전
    private void Reload() {
        // Debug.Log("reloading...");
        _isReloading = true;
        _isShootable = false;
        StartCoroutine(ReShootable());
    }

    // 발포 딜레이, 재장전 딜레이 처리
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

    // 우클릭 입력 처리(대상 지정 중력 강화)
    private void TargetGravity() {
        if(!_isTargetable) {
            return;
        } else if(_energy < _energyCostLocal) {
            return;
        }
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.transform.forward, out hit)) {
            Rigidbody rigid;
            if(rigid = hit.collider.gameObject.GetComponent<Rigidbody>()) {
                Debug.Log("TargetGravity: target detected");
                _isTargetable = false;
                StartCoroutine(ReTargetable());
                _energy -= _energyCostLocal;

                // 여기서 피격된 대상의 오브젝트를 불러올 수 있음
                rigid.AddForce(Physics.gravity * rigid.mass * (_gravityForceLocal - 1f), ForceMode.Impulse);
                // hit.collider.gameObject.GetComponent<EnemyController>().OnHit();
            }
        }
    }

    // 대상 지정 중력 강화 딜레이
    IEnumerator ReTargetable() {
        yield return new WaitForSeconds(_gravityCooldownLocal);
        if(!_isTargetable) {
            Debug.Log("targetable");
            _isTargetable = true;
        }
    }
    // 전체 중력 강화
    // 여러 오브젝트에 대해 루프를 돌며 작업을 수행해야 하므로 프레임 드랍을 막기 위해 코루틴으로 실행
    private void GlobalGravity(List<GameObject> gameObjects) {
        Debug.Log("enter");
        if(_energy < _energyCostHigh) {
            return;
        }
        _energy -= _energyCostHigh;
        foreach(var obj in gameObjects) {
            Rigidbody rigid;
            // 여기서 오브젝트 내 스크립트에 있는 함수를 호출해야 함
            if(rigid = obj.GetComponent<Rigidbody>()) {
                rigid.AddForce(Physics.gravity * rigid.mass * (_gravityForceHigh - 1f), ForceMode.Impulse);
                //
            }
        }
    }

    // 피격 시 호출(외부에서)
    public void OnHit() {
        if(--hp <= 0) _isAlive = false;
    }
}

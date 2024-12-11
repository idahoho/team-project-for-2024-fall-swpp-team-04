using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [Header("Player Status")]
    [SerializeField] private int _maxHP;
    [SerializeField] private float _maxEnergy;
    [SerializeField] private float _energyRecoveryPerSec;
    private int _currentHP;
    private float _currentEnergy;
    private bool _isAlive = true;
    [Header("UI")]

    [Header("Attack")]
    [SerializeField] private KeyCode _reloadKey;
    [SerializeField] private int _maxBullet;
    [SerializeField] private float _shootCooldown;
    [SerializeField] private float _reloadTime;

    [SerializeField] private GameObject _gunGameObject;
	// to set direction of raycast
	  private Transform _camera;
    
	  [SerializeField] private GameObject _sparkParticle;
	

    private int _currentBullet;
    private bool _isShootable = true;
    private bool _isReloading = false;

    [Header("Local Gravity Ability")]
    // local gravity control(right click)
    [SerializeField] private float _localGravityCooldown;
    [SerializeField] private float _localGravityForce;
    [SerializeField] private float _localEnergyCost;
    private bool _isTargetable = true;

    [Header("Global Gravity Ability")]
    // global gravity control(mouse wheel)
    [SerializeField] private float _mouseInputWheel;
    [SerializeField] private float _wheelInputThreshold;
    [SerializeField] private float _gravityForceHigh;
    [SerializeField] private float _gravityForceLow;
    [SerializeField] private float _globalLowEnergyCost;
    [SerializeField] private float _globalHighEnergyCost;
    public bool _isGravityLow = false;

    [Header("Interactive")]
    [SerializeField] private float _interactiveRange;
    [SerializeField] private KeyCode _interactKey;
    private HashSet<GameObject> _interactedObjects = new HashSet<GameObject>();
    private int _stage = 1;

	[SerializeField] private AudioClip _fireSound; 
	[SerializeField] private AudioClip _reloadSound;
	[SerializeField] private AudioClip _hitSound;
	[SerializeField] private AudioClip _globalGravityUpSound;
	[SerializeField] private AudioClip _globalGravityDownSound;
	[SerializeField] private AudioClip _localGravitySound;
	private AudioSource _audioSource;



	void Start() {
        _camera = GameObject.Find("PlayerCamera").transform;
        _rigidbody = GetComponent<Rigidbody>();
        _currentHP = _maxHP;
        _currentBullet = _maxBullet;
        UIManager.Instance.UpdateBullet(_currentBullet, _maxBullet);
        _currentEnergy = _maxEnergy;
		_audioSource = GetComponent<AudioSource>();

	}
	private void FixedUpdate() {
        // 낮아진 전체 중력에 대한 처리
        // 플레이어에게만 영향을 주도록 할 경우 이걸로 사용
        // 전체적으로 영향을 주도록 하려면 Physics.gravity 자체를 바꿔줘야 함
        if(_isGravityLow) {
            _rigidbody.AddForce(Physics.gravity * (_gravityForceLow - 1f), ForceMode.Impulse);
        }
    }
    void Update() {
        if(_isAlive) {
            HandleButtonInput();
            HandleWheelInput();
            UpdateEnergy();
            CheckInteractive();
        }
    }

    // 마우스 좌클릭, 우클릭, 재장전 입력 처리
    private void HandleButtonInput() {
        // left click event
        if(Input.GetButtonDown("Fire1")) {
            Fire();
        }
        // right click event
        if(Input.GetButtonDown("Fire2") && _stage >= 2) {
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
        if (_stage >= 3)
        {
	        if (_mouseInputWheel > _wheelInputThreshold)
	        {
		        _isGravityLow = true;
				_audioSource.clip = _globalGravityDownSound;
				_audioSource.loop = true;
				_audioSource.Play();
			}
	        else if (_mouseInputWheel < -_wheelInputThreshold)
	        {
		        if (_isGravityLow)
		        {
			        _isGravityLow = false;
					_audioSource.Stop();
				}
		        else if(_stage >= 4)
		        {
                    // 코루틴에서 리스트 처리 중 리스트에 변경이 가해질 수 있음
                    // 이로 인한 문제를 막기 위해 리스트의 요소를 모두 옮겨담아 새로운 리스트를 만들어 전달한다.
                    List<GameObject> copiedList = new List<GameObject>();
                    List<GameObject> activeEnemies = GameManager.Instance.GetActiveEnemies();
                    foreach (GameObject obj in activeEnemies) {
                        copiedList.Add(obj);
                    }
			        GlobalGravity(copiedList);
		        }
	        }
        }
    }

    public void UpdateStage(int stage)
    {
	    _stage = stage;
    }


    // 중력 능력을 사용 중이지 않을 때, 매 초마다 _energyRecover만큼 에너지를 회복한다.
    private void UpdateEnergy() {
        if(_isGravityLow) {
            _currentEnergy -= _globalLowEnergyCost * Time.deltaTime;
            if(_currentEnergy < 0) {
                _currentEnergy = 0;
                _isGravityLow = false;
            }
        } else {
            _currentEnergy += _energyRecoveryPerSec * Time.deltaTime;
            if(_currentEnergy > _maxEnergy) _currentEnergy = _maxEnergy;
        }
    }

    // 좌클릭 입력 처리(총)
    private void Fire() {
        if(!_isShootable) {
            return;
        }
        if(_currentBullet-- > 0) {
            _isShootable = false;
			_audioSource.PlayOneShot(_fireSound);
			Debug.Log("fire");
            _gunGameObject.SendMessage("HandleRecoil", SendMessageOptions.DontRequireReceiver);
			RaycastHit hit;
            if(Physics.Raycast(_camera.position, _camera.transform.forward, out hit)) {
                // 여기에서 맞은 대상의 오브젝트 가져올 수 있음
				Instantiate(_sparkParticle, hit.point, Quaternion.identity);
				var targetAttackReceiver = hit.collider.gameObject.GetComponent<IAttackReceiver>();
				if (targetAttackReceiver != null)
				{
					Debug.Log("Fire: enemy detected");
					targetAttackReceiver.OnHit();
                }
            }
            UIManager.Instance.UpdateBullet(_currentBullet, _maxBullet);
            UIManager.Instance.CrossHairFire();
            StartCoroutine(ReShootable());
        } else {
            Reload();
        }
    }

    // 재장전
    private void Reload() {
		// Debug.Log("reloading...");
		if (_isReloading) return;
		_audioSource.PlayOneShot(_reloadSound);
		_isReloading = true;
        _isShootable = false;
        if (_gunGameObject != null)
        {
	        _gunGameObject.SendMessage("HideGunOnReload", SendMessageOptions.DontRequireReceiver);
        }
		StartCoroutine(ReShootable());
    }

    // 발포 딜레이, 재장전 딜레이 처리
    private IEnumerator ReShootable() {
        if(_isReloading) {
            yield return new WaitForSeconds(_reloadTime);
            _isReloading = false;
            _currentBullet = _maxBullet;
            UIManager.Instance.UpdateBullet(_currentBullet, _maxBullet);
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
        } else if(_currentEnergy < _localEnergyCost) {
            return;
        }
        RaycastHit hit;
        if(Physics.Raycast(_camera.position, _camera.transform.forward, out hit)) {
            //Rigidbody rigid;
			ISkillReceiver targetSkillReceiver =  hit.collider.gameObject.GetComponent<ISkillReceiver>();

			if (targetSkillReceiver != null) {
                Debug.Log("TargetGravity: target detected");
                _isTargetable = false;
                StartCoroutine(ReTargetable());
                _currentEnergy -= _localEnergyCost;
				_audioSource.PlayOneShot(_localGravitySound);

				// 여기서 피격된 대상의 오브젝트를 불러올 수 있음
				//rigid.AddForce(Physics.gravity * rigid.mass * (_localGravityForce - 1f), ForceMode.Impulse);
				// hit.collider.gameObject.GetComponent<EnemyController>().OnHit();
				targetSkillReceiver.ReceiveSkill();
            }
        }
    }

    // 대상 지정 중력 강화 딜레이
    private IEnumerator ReTargetable() {
        yield return new WaitForSeconds(_localGravityCooldown);
        if(!_isTargetable) {
            Debug.Log("targetable");
            _isTargetable = true;
        }
    }
    // 전체 중력 강화
    // 여러 오브젝트에 대해 루프를 돌며 작업을 수행해야 하므로 프레임 드랍을 막기 위해 코루틴으로 실행
    private void GlobalGravity(List<GameObject> gameObjects) {
        Debug.Log("enter");
        if(_currentEnergy < _globalHighEnergyCost) {
            return;
        }
        _currentEnergy -= _globalHighEnergyCost;
		_audioSource.PlayOneShot(_globalGravityUpSound);
		_gunGameObject.SendMessage("HideGunOnSkill", SendMessageOptions.DontRequireReceiver);
		
        StartCoroutine(SendGravitySkill(gameObjects));
    }

    private IEnumerator SendGravitySkill(List<GameObject> gameObjects) {
        foreach (GameObject obj in gameObjects) {
            ISkillReceiver targetSkillReceiver;
            if(obj && obj.activeSelf && obj.TryGetComponent<ISkillReceiver>(out targetSkillReceiver)) {
                targetSkillReceiver.ReceiveSkill();
            }
            yield return null;
        }
    }

	private void CheckInteractive()
	{
		RaycastHit hit;
		if (Physics.Raycast(_camera.position, _camera.transform.forward, out hit, _interactiveRange))
		{
			GameObject targetObject = hit.collider.gameObject;

			if (targetObject.CompareTag("Lever"))
			{
				if (targetObject.TryGetComponent<DoorLever>(out DoorLever lever))
				{
					if (!lever._isInteractable)
					{
						UIManager.Instance.HideInteractionUi();
					}
					else
					{
						UIManager.Instance.EDoorInteractionUi();
					}
				}
			}
			else if (targetObject.CompareTag("Core"))
			{
				if (targetObject.TryGetComponent<IInteractable>(out IInteractable coreInteraction))
				{
					if (!coreInteraction.IsInteractable())
					{
						UIManager.Instance.HideInteractionUi();
						return;
					}
					UIManager.Instance.ECoreInteractionUi();
				}
			}

			if (targetObject.TryGetComponent<IInteractable>(out IInteractable interactable) && Input.GetKeyDown(_interactKey))
			{
				interactable.Interactive();
				UIManager.Instance.HideInteractionUi();
			}
		}
		else
		{
			UIManager.Instance.HideInteractionUi();
		}
	}



	// 피격 시 호출(외부에서)
	public void OnHit() {
        if(!_isAlive) {
            return;
        }
        Debug.Log("hit");
		_audioSource.PlayOneShot(_hitSound);
		_currentHP--;
        UIManager.Instance.UpdateHP(_currentHP, _maxHP);
        if(_currentHP <= 0) {
            _isAlive = false;
			SceneManager.LoadScene("GameOverScene");
		}
    }

    // UIManager에서 호출
    public float GetHPRatio() {
        return (float)_currentHP / _maxHP;
    }
    public float GetEnergyRatio() {
        return _currentEnergy / _maxEnergy;
    }
}

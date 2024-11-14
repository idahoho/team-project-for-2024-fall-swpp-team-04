using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour, IEnemy
{
	private Transform _body;
	private Transform _joint;
	private Transform _wing;
	private Transform _gun;

	[Header("Target")]
	private GameObject _player;

	[Header("Projectile")]
	[SerializeField] private GameObject _projectile;
	[SerializeField] private float _projectileSpeed;

	[Header("Wander")]
	[SerializeField] private float _wanderSpeed;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _verticalDirectionRange;
	[SerializeField] private float _changeDirectionIntervalMin;
	[SerializeField] private float _changeDirectionIntervalMax;
	[SerializeField] private float _obstacleDetectionRange;
	// [SerializeField] private float _hoveringRange;
	// [SerializeField] private float _hoveringInterval;
	private Vector3 _spawnPoint;
	private Vector3 _currentDirection;
	private float _timer;
	private float _changeDirectionInterval;
	private bool _isMoving = false;

	[Header("Hover")]
	[SerializeField] private float _angularSpeed;
	public float _phase = 0;
	[SerializeField] private float _amplitude;

	[Header("Chase")]
	[SerializeField] private float _chaseRange;

	[Header("Attack")]
	[SerializeField] private float _rotationSpeed;
	[SerializeField] private float _attackRangeHorizontal;
	[SerializeField] private float _attackRangeVertical;
	[SerializeField] private float _chargeTime;
	[SerializeField] private float _chargeCooldown;
	private bool _isCharging = false; // Indicates if the enemy is currently charging
	// issue: 총알 재장전 방식 등과 일관성을 위해 isChargable 같은 거 두고 관리하는 게 어떨까 싶음
	private float _chargeCooldownTimer; // Timer for charge cooldown

	[SerializeField] private int _maxHp;
	private int _hp;

	void Awake()
	{
		_hp = _maxHp;
		_spawnPoint = transform.position;
		SetRandomDirection();
		_chargeCooldownTimer = _chargeCooldown;
	}

	private void Start() {
		_body = transform.GetChild(0);
		_joint = _body.GetChild(1);
		_wing = _body.GetChild(2);
		_gun = _joint.GetChild(0);

		_player = GameObject.Find("Player");
	}

	private void Update() {
		float distanceHorizontal = Vector3.Scale(transform.position - _player.transform.position, new Vector3(1, 0, 1)).magnitude;
		float distanceVertical = transform.position.y - _player.transform.position.y;

		if(distanceHorizontal < _attackRangeHorizontal && distanceVertical < _attackRangeVertical) {
			DetectPlayer();
		}
		else {
			Wander();
		}

		// Update charge cooldown
		if(_chargeCooldownTimer < _chargeCooldown) {
			_chargeCooldownTimer += Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		// delta y = sin(2*pi*w*t), where w=_angularSpeed, w*t=_phase
		_phase += Time.fixedDeltaTime*_angularSpeed;
		_phase-=Mathf.Floor(_phase);
		_body.localPosition = Mathf.Sin(2 * Mathf.PI * _phase) * _amplitude*(Quaternion.Inverse(transform.rotation)*Vector3.up);
	}

	private void Wander() {
		if(_isCharging) {
			return;
		}
		_timer += Time.deltaTime;
		if(_timer > _changeDirectionInterval) {
			SetRandomDirection();
			SetRandomInterval();
			_timer = 0f;
		} else if(Vector3.Distance(_spawnPoint, transform.position) > _wanderRange) {
			_currentDirection = (_spawnPoint - transform.position).normalized;
			SetRandomInterval();
			_timer = 0f;
		} else if(Physics.Raycast(transform.position, _currentDirection, _obstacleDetectionRange)) {
			SetRandomDirection();
			SetRandomInterval();
		} 

		if(_isMoving) {
			// rotate
			Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
			Quaternion targetRotationHorizontal = Quaternion.LookRotation(Vector3.Scale(_currentDirection, new Vector3(1, 0, 1)));
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
			_body.rotation = Quaternion.Slerp(_body.rotation, targetRotationHorizontal, Time.deltaTime * _rotationSpeed);
			
			// move
			transform.Translate(Vector3.forward * Time.deltaTime * _wanderSpeed);
		}
	}

	private void SetRandomInterval() {
		_changeDirectionInterval = Random.Range(_changeDirectionIntervalMin, _changeDirectionIntervalMax);
	}

	private void SetRandomDirection() {
		if(_isMoving) {
			_currentDirection = Vector3.zero;
			_isMoving = false;
		} else {
			_isMoving = true;
			_currentDirection = RandomDirection();
		}
	}

	private Vector3 RandomDirection()
	{
		float angle = Random.Range(0, 360);
		return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),
													Random.Range(-_verticalDirectionRange, _verticalDirectionRange),
													Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}

	private void DetectPlayer() {
		// rotate
		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
		Vector3 gunDirection = (_player.transform.position - _gun.position).normalized;

		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		Quaternion targetRotationHorizontal = Quaternion.LookRotation(Vector3.Scale(directionToPlayer, new Vector3(1, 0, 1)));
		Quaternion targetRotationGun = Quaternion.LookRotation(gunDirection);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
		_body.rotation = Quaternion.Slerp(_body.rotation, targetRotationHorizontal, Time.deltaTime * _rotationSpeed);
		_gun.rotation = Quaternion.Slerp(_gun.rotation, targetRotationGun, Time.deltaTime * _rotationSpeed);

		if(_isCharging) {
			return;
		}
		if(_chargeCooldownTimer >= _chargeCooldown) {
			_isCharging = true;
			Debug.Log("charge");
			StartCoroutine(ChargeAndFire());
		}
	}

	private IEnumerator ChargeAndFire() {
		yield return new WaitForSeconds(_chargeTime);	

		FireProjectile();
		
		_chargeCooldownTimer = 0f;
		_isCharging = false;
	}

	private void FireProjectile() {
		GameObject proj = Instantiate(_projectile, transform.position + transform.forward * 2, Quaternion.identity);

		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;

		// proj.transform.rotation = Quaternion.LookRotation(directionToPlayer);
		Rigidbody rb = proj.GetComponent<Rigidbody>();
		rb.velocity = directionToPlayer * _projectileSpeed;

		// issue: 필요할까? Destroy를 많은 곳에서 시도하면 뭔가 문제가 생길 수도 있음
		// Destroy(proj, 5f); 
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, _currentDirection * _obstacleDetectionRange);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(_spawnPoint, _wanderRange);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, _attackRangeHorizontal);
	}

	public void OnHit()
	{
		if (--_hp <= 0)
		{
			OnDeath();
		}
		// hit effect goes here: particle, knockback, etc.
	}

	public void OnDeath()
	{
		// death animation goes here; must wait till the animation to be finished before destroying
		GameManager.Instance.UnregisterEnemy(gameObject);
		Destroy(gameObject);
	}
}

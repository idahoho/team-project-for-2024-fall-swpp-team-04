using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] private GameObject _player;

	[Header("Projectile")]
	[SerializeField] private GameObject _projectile;
	[SerializeField] private float _projectileSpeed;

	[Header("Wander")]
	[SerializeField] private float _wanderSpeed;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _changeDirectionInterval;
	[SerializeField] private float _obstacleDetectionRange;
	private Vector3 _spawnPoint;
	private Vector3 _currentDirection;
	private float _timer;

	[Header("Attack")]
	[SerializeField] private float _rotationSpeed;
	[SerializeField] private float _attackRange;
	[SerializeField] private float _chargeTime; // Charging time before firing
	[SerializeField] private float _chargeCooldown;
	private bool _isCharging = false; // Indicates if the enemy is currently charging
	// issue: 총알 재장전 방식 등과 일관성을 위해 isChargable 같은 거 두고 관리하는 게 어떨까 싶음
	private float _chargeCooldownTimer; // Timer for charge cooldown
	
	private void Start() {
		_spawnPoint = transform.position;
		SetRandomDirection();
		_chargeCooldownTimer = _chargeCooldown;
	}

	private void Update() {
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if(distanceToPlayer < _attackRange) {
			// issue: 코루틴을 이렇게 선언하면 위험할 것 같음
			// 코루틴 밖에서 _isCharging을 true로 바꿔줘야 이 함수가 여러 번 실행되는 걸 막을 수 있을 듯
			// fix: StartAndFire 외부에서 처리하기 위한 함수 한 단계를 추가
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

	private void Wander() {
		if(_isCharging) {
			return;
		}
		_timer += Time.deltaTime;
		if(_timer > _changeDirectionInterval) {
			SetRandomDirection();
			_timer = 0f;
		} else if(Vector3.Distance(_spawnPoint, transform.position) > _wanderRange) {
			_currentDirection = (_spawnPoint - transform.position).normalized;
			_timer = 0f;
		} else if(Physics.Raycast(transform.position, _currentDirection, _obstacleDetectionRange)) {
			SetRandomDirection();
		} 

		// rotate
		Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
		// move
		transform.Translate(Vector3.forward * Time.deltaTime * _wanderSpeed);
	}

	private void SetRandomDirection() {
		float angle = Random.Range(0, 360);
		_currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Random.Range(-1f, 1f), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}

	private void DetectPlayer() {
		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;

		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

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
		Gizmos.DrawWireSphere(transform.position, _attackRange);
	}
}

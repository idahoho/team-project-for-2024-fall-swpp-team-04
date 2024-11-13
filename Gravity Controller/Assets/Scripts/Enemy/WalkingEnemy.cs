using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingEnemy : MonoBehaviour, IEnemy
{
	private Animator _animator;
	[Header("Target")]
	private GameObject _player;

	[Header("Wander")]
	[SerializeField] private float _wanderSpeed;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _changeDirectionInterval;
	[SerializeField] private float _obstacleDetectionRange;
	private Vector3 _spawnPoint;
	private Vector3 _currentDirection;
	private float _timer = 0f;

	[Header("Chase")]
	[SerializeField] private float _chaseSpeed;
	[SerializeField] private float _chaseRange;
	[SerializeField] private float _attackRange;
	[SerializeField] private float _attackHitRange;
	private bool _isChasing = false;
	private bool _isAttacking = false;

	[SerializeField] private int _maxHp;
	private int _hp;

	void Awake()
	{
		_hp = _maxHp;
	}

	void Start() {
		_player = GameObject.Find("Player");

		_animator = GetComponent<Animator>();
		_spawnPoint = transform.position;
		SetRandomDirection();
	}

	// fix: 구조를 조금 더 깔끔하게 변경
	void Update() {
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if(!_isChasing) {
			if(distanceToPlayer < _chaseRange) { // player firstly detected
				_isChasing = true;
				_animator.SetBool("FollowPlayer", true);
			} else { // player not detected
				Wander();
			}
		} else { // player detected
			if(distanceToPlayer < _attackRange) { // if close enough
				Attack();
			} else {
				Chase();
			}
		}
	}

	private void Wander() {
		_timer += Time.deltaTime;
		if(_timer > _changeDirectionInterval) {
			// Issue: interval에도 약간의 랜덤성을 주면 좋을 것 같음
			SetRandomDirection();
			_timer = 0f;
		} else if(Vector3.Distance(_spawnPoint, transform.position) > _wanderRange) {
			// 일정 범위를 벗어나지 못하도록
			_currentDirection = (_spawnPoint - transform.position).normalized;
			_timer = 0f;
		} else if(Physics.Raycast(transform.position, _currentDirection, _obstacleDetectionRange)) {
			// 장애물을 만나면 다른 방향으로
			SetRandomDirection();
			_timer = 0f;
		}

		// rotate
		Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _wanderSpeed);
		// move
		transform.Translate(Vector3.forward * Time.deltaTime * _wanderSpeed);
	}

	private void SetRandomDirection() {
		float angle = Random.Range(0, 360);
		_currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}

	// fix: 플레이어의 높낮이의 변화가 있으면 문제가 생길 것 같아서 수정
	private void Chase() {
		if(_isAttacking) {
			return;
		}
		Vector3 direction = Vector3.Scale(_player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
		transform.position += direction * _chaseSpeed * Time.deltaTime;
		transform.LookAt(_player.transform.position);
	}

	private void Attack() {
		if(_isAttacking) {
			return;
		}

		_isAttacking = true;
		_animator.SetBool("AttackPlayer", true);

		StartCoroutine(ResetAttack());
	}

	// fix: Invoke 대신 Coroutine 사용하는 구조로 변경
	IEnumerator ResetAttack() {
		yield return new WaitForSeconds(1f);

		_isAttacking = false;
		_animator.SetBool("AttackPlayer", false);
	}

	// fix: player controller가 유효한지 검사하는 과정 삭제
	public void AttackHitCheck() {
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if(distanceToPlayer < _attackHitRange) {
			Debug.Log("Attack Successful");
			_player.GetComponent<PlayerController>().OnHit();
		} else {
			Debug.Log("Attack Fail");
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, _currentDirection * _obstacleDetectionRange);
		Gizmos.DrawWireSphere(transform.position, _attackRange);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(_spawnPoint, _wanderRange);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, _chaseRange);
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

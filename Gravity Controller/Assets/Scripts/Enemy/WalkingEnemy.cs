using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingEnemy : MonoBehaviour, IEnemy, IAttackReceiver
{
	private Animator _animator;
	[Header("Target")]
	private GameObject _player;
	[SerializeField] private float _heightOffset;
	[SerializeField] private float _playerHeightOffset;

	[Header("Wander")]
	[SerializeField] private float _wanderSpeed;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _rotationSpeed;
	[SerializeField] private float _changeDirectionInterval;
	[SerializeField] private float _obstacleDetectionRange;
	[SerializeField] private float _changeDirectionIntervalMin;
	[SerializeField] private float _changeDirectionIntervalMax;
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

	[SerializeField] private float _sightRange;
	[SerializeField] private float _awarenessCoolDown;
	private float _awarenessCoolDownTimer;
	private Vector3 _lastSeenPosition;

	[SerializeField] private float _attackAnimationLength = 1.25f;

	[SerializeField] private int _maxHp;
	private int _hp;

	public EnemyState State { get; private set; }

	void Awake()
	{
		_hp = _maxHp;

		State = EnemyState.Idle;
	}

	void Start() {
		_player = GameObject.Find("Player");

		_animator = transform.GetChild(0).GetComponent<Animator>();
		_spawnPoint = transform.position;

		SetRandomDirection();
		BeforeWander();
	}

	// fix: 구조를 조금 더 깔끔하게 변경
	/*
	void Update() {
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if(!_isChasing) {
			if(distanceToPlayer < _chaseRange) { // player firstly detected
				_isChasing = true;
				_animator.SetBool("IsRunning", true);
			} else { // player not detected
				_isChasing = false;
				_animator.SetBool("IsWalking", true);
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
	*/

	private void FixedUpdate()
	{
		var relativePosition = _player.transform.position + _playerHeightOffset * new Vector3(0, 1, 0) - transform.position - _heightOffset * new Vector3(0, 1, 0);
		float distanceHorizontal = Vector3.Scale(relativePosition, new Vector3(1, 0, 1)).magnitude;
		float distanceVertical = transform.position.y - _player.transform.position.y;
		RaycastHit hit;
		bool playerInSight = false;

		_awarenessCoolDownTimer -= Time.fixedDeltaTime;

		if (Physics.Raycast(transform.position + _heightOffset * new Vector3(0,1,0), Vector3.Scale(relativePosition, new Vector3(1, 0, 1)), out hit, _sightRange))
		{
			playerInSight = hit.collider.gameObject.CompareTag("Player");
			if (playerInSight)
			{
				_awarenessCoolDownTimer = _awarenessCoolDown;
				_lastSeenPosition = _player.transform.position;
			}
			Debug.Log("Hit:" + hit.collider.name);
		}

		switch (State)
		{
			case EnemyState.Idle:
				if ((distanceHorizontal <= _chaseRange) && playerInSight)
				//if (playerInSight)
					{
					// Idle -> Aware
					State = EnemyState.Aware;
					_animator.SetBool("IsRunning", true);
					_animator.SetBool("IsWalking", false);
					Chase();
					break;
				}
				Wander();
				break;
			case EnemyState.Aware:
				if (!playerInSight || !(distanceHorizontal <= _chaseRange))
				//if (!playerInSight)
					{
					// cannot see the player || player not in range
					if (_awarenessCoolDownTimer > 0)
					{
						// Aware -> Follow
						State = EnemyState.Follow;
						Follow();
						break;
					}
					// Aware -> Idle
					_animator.SetBool("IsRunning", false);
					State = EnemyState.Idle;
					BeforeWander();
					Wander();
					break;
				}

				Chase();
				break;
			case EnemyState.Follow:
				if (_awarenessCoolDownTimer <= 0 || _lastSeenPosition == transform.position)
				{
					// time's up || lost player
					// Follow -> Idle
					_animator.SetBool("IsRunning", false);
					State = EnemyState.Idle;
					BeforeWander();
					Wander();
					break;
				}
				if (distanceHorizontal <= _chaseRange)
				//if (playerInSight)
					{
					// gotcha
					// Follow -> Aware
					State = EnemyState.Aware;
					Chase();
					break;
				}

				Follow();
				break;
		}

		Debug.Log(State);
	}

	private void BeforeWander()
	{
		_animator.SetBool("IsWalking", true);
		SetRandomInterval();
		_timer = 0f;
	}

	private void Wander() {
		_timer += Time.deltaTime;
		RaycastHit hit;
		if (Physics.Raycast(transform.position + _heightOffset * new Vector3(0, 1, 0), _currentDirection, out hit, _obstacleDetectionRange))
		{
			// detected an obstacle while moving
			Debug.Log("detected an obstacle:" + hit.collider.name);
			SetRandomDirection();
			_timer = 0f;
		}
		else if (_timer > _changeDirectionInterval) {
			// phase transition: moving <-> rotating
			if (Vector3.Distance(_spawnPoint, transform.position) > _wanderRange)
			{
				// too far from origin
				_currentDirection = Vector3.Scale(_spawnPoint - transform.position, new Vector3(1, 0, 1)).normalized;
			}
			else
			{
				// okay to go
				SetRandomDirection();
			}
			SetRandomInterval();
			_timer = 0f;
		}
		/*
		else if(Vector3.Distance(_spawnPoint, transform.position) > _wanderRange) {
			// 일정 범위를 벗어나지 못하도록
			_currentDirection = (_spawnPoint - transform.position).normalized;
			_timer = 0f;
		} else if(Physics.Raycast(transform.position, _currentDirection, _obstacleDetectionRange)) {
			// 장애물을 만나면 다른 방향으로
			SetRandomDirection();
			_timer = 0f;
		}
		*/

		// rotate
		Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
		Quaternion targetRotationHorizontal = Quaternion.LookRotation(Vector3.Scale(_currentDirection, new Vector3(1, 0, 1)));
		var tempRotation = Quaternion.Slerp(transform.rotation, targetRotationHorizontal, Time.fixedDeltaTime * _rotationSpeed);
		transform.rotation = tempRotation;

		// move
		transform.Translate(Vector3.forward * Time.fixedDeltaTime * _wanderSpeed);
	}

	private void SetRandomInterval()
	{
		_changeDirectionInterval = Random.Range(_changeDirectionIntervalMin, _changeDirectionIntervalMax);
	}

	private void SetRandomDirection()
	{
		float angle = Random.Range(0, 360);
		_currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}

	// fix: 플레이어의 높낮이의 변화가 있으면 문제가 생길 것 같아서 수정
	private void Chase() {
		// not to be confused with FlyingEnemy.ChasePlayer(); this method is for EnemyState.Aware
		if(_isAttacking) {
			return;
		}

		float distanceToPlayer = Vector3.Scale(transform.position - _player.transform.position, new Vector3(1, 0, 1)).magnitude;

		if (distanceToPlayer < _attackRange)
		{
			Attack();
			return;
		}

		Vector3 direction = Vector3.Scale(_player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.fixedDeltaTime * _rotationSpeed);
		transform.Translate(Vector3.forward * _chaseSpeed * Time.deltaTime);
		_spawnPoint = transform.position;
	}

	private void Attack() {
		if(_isAttacking) {
			return;
		}

		_isAttacking = true;
		_animator.SetBool("Attack", true);

		StartCoroutine(ResetAttack());
	}

	// fix: Invoke 대신 Coroutine 사용하는 구조로 변경
	IEnumerator ResetAttack() {
		yield return new WaitForSeconds(_attackAnimationLength);

		_isAttacking = false;
		_animator.SetBool("Attack", false);
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

	private void Follow()
	{
		Vector3 directionToPlayer = _lastSeenPosition - transform.position;
		Vector3 directionToPlayerHorizontal = Vector3.Scale(directionToPlayer, new Vector3(1, 0, 1));

		// rotate
		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		Quaternion targetRotationHorizontal = Quaternion.LookRotation(directionToPlayerHorizontal);
		var tempRotation = Quaternion.Slerp(transform.rotation, targetRotationHorizontal, Time.fixedDeltaTime * _rotationSpeed);
		transform.rotation = tempRotation;

		// move
		Vector3 dir = Vector3.zero;
		Vector3 target = Vector3.zero;


		target += directionToPlayerHorizontal.normalized;
		
		dir = target.normalized * Time.fixedDeltaTime * _chaseSpeed;

		transform.Translate(Quaternion.Inverse(transform.rotation) * dir);
		_spawnPoint = transform.position;
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

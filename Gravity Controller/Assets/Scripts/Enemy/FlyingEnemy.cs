using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour, IEnemy, ISkillReceiver, IAttackReceiver
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
	[SerializeField] private float _homingInstinct;
	private Vector3 _spawnPoint;
	private Vector3 _currentDirection;
	private float _timer;
	private float _changeDirectionInterval;
	private bool _isMoving = false;

	[Header("Hover")]
	[SerializeField] private float _angularSpeed;
	private float _phase = 0;
	[SerializeField] private float _amplitude;

	[Header("Chase")]
	[SerializeField] private float _chaseRangeHorizontal;
	[SerializeField] private float _chaseRangeVertical;
	[SerializeField] private float _sightRange;
	[SerializeField] private float _awarenessCoolDown;
	private float _awarenessCoolDownTimer;
	private Vector3 _lastSeenPosition;

	[Header("Attack")]
	[SerializeField] private float _rotationSpeed;
	[SerializeField] private float _attackRangeHorizontal;
	[SerializeField] private float _attackRangeVertical;
	[SerializeField] private float _chargeTime;
	[SerializeField] private float _chargeCooldown;
	private bool _isCharging = false; // Indicates if the enemy is currently charging
	// issue: 총알 재장전 방식 등과 일관성을 위해 isChargable 같은 거 두고 관리하는 게 어떨까 싶음
	private float _chargeCooldownTimer; // Timer for charge cooldown
	[SerializeField] private float _minHorizontalDistance;
	[SerializeField] private float _minVerticalDistance;

	[Header("Aim")]
	// one must have  _minGunAngleDegree <= 90 <= _maxGunAngleDegree
	[SerializeField] private float _minGunAngleDegree;
	[SerializeField] private float _maxGunAngleDegree;
	private bool _adjustingAimAngle = false;
	[SerializeField] private float _adjustingAimAngleRatio = 0.7f;

	[SerializeField] private int _maxHp;
	private int _hp;

	[Header("ReceiveSkill")]
	[SerializeField] private float _fallingSpeed;
	[SerializeField] private float _minHeight;
	private bool _isFalling = false;
	private bool _isNeutralized = false;
	public EnemyState State { get; private set; }

	void Awake()
	{
		_hp = _maxHp;
		_spawnPoint = transform.position;
		SetRandomDirection();
		_chargeCooldownTimer = _chargeCooldown;

		BeforeWander();
		State = EnemyState.Idle;
	}

	private void Start() {
		_body = transform.GetChild(0);
		_joint = _body.GetChild(1);
		_wing = _body.GetChild(2);
		_gun = _joint.GetChild(0);

		_player = GameObject.Find("Player");
	}

	private void Update() {
		
	}

	private void FixedUpdate()
	{
		if(_isFalling)
		{
			// fall
			RaycastHit hitBelow;
			if(Physics.Raycast(transform.position, new Vector3(0,-1,0), out hitBelow, _minHeight)){
				Debug.Log("Hit below: " + hitBelow.collider.name);
				_isFalling = false;
			}
			else transform.Translate(new Vector3(0,- _fallingSpeed * Time.fixedDeltaTime,0));
		}
		if (_isNeutralized) return;

		var relativePosition = _player.transform.position - transform.position;
		float distanceHorizontal = Vector3.Scale(relativePosition, new Vector3(1, 0, 1)).magnitude;
		float distanceVertical = transform.position.y - _player.transform.position.y;
		RaycastHit hit;
		bool playerInSight = false;

		_awarenessCoolDownTimer -= Time.fixedDeltaTime;

		if (Physics.Raycast(transform.position, relativePosition, out hit, _sightRange))
		{
			playerInSight = hit.collider.gameObject.CompareTag("Player");
			if (playerInSight)
			{
				_awarenessCoolDownTimer = _awarenessCoolDown;
				_lastSeenPosition = _player.transform.position;
			}
			Debug.Log("Hit:" + hit.collider.name);
		}

		switch (State){
			case EnemyState.Idle:
				if ((distanceHorizontal <= _attackRangeHorizontal && 0 <= distanceVertical && distanceVertical <= _attackRangeVertical) && playerInSight)
				{
					// Idle -> Aware
					State = EnemyState.Aware;
					DetectPlayer();
				}
				else
				{
					Wander();
				}
				break;
			case EnemyState.Aware:
				if (!playerInSight || !(distanceHorizontal <= _attackRangeHorizontal && 0 <= distanceVertical && distanceVertical <= _attackRangeVertical))
				{
					// cannot see the player || player not in range
					if (_awarenessCoolDownTimer > 0)
					{
						// Aware -> Follow
						State = EnemyState.Follow;
						ChasePlayer();
						break;
					}
					// Aware -> Idle
					State = EnemyState.Idle;
					BeforeWander();
					Wander();
					break;
				}
				
				DetectPlayer();
				break;
			case EnemyState.Follow:
				if (_awarenessCoolDownTimer<=0)
				{
					// time's up
					// Follow -> Idle
					State = EnemyState.Idle;
					BeforeWander();
					Wander();
					break;
				}
				if (distanceHorizontal <= _attackRangeHorizontal && 0 <= distanceVertical && distanceVertical <= _attackRangeVertical)
				{
					// gotcha
					// Follow -> Aware
					State = EnemyState.Aware;
					DetectPlayer();
					break;
				}

				ChasePlayer();
				break;
		}

		Debug.Log(State);

		// Update charge cooldown
		if(_chargeCooldownTimer < _chargeCooldown) {
			_chargeCooldownTimer += Time.deltaTime;
		}

		// delta y = sin(2*pi*w*t), where w=_angularSpeed, w*t=_phase
		_phase += Time.fixedDeltaTime*_angularSpeed;
		_phase-=Mathf.Floor(_phase);
		_body.localPosition = Mathf.Sin(2 * Mathf.PI * _phase) * _amplitude*(Quaternion.Inverse(transform.rotation)*Vector3.up);
	}

	private void BeforeWander()
	{
		SetRandomInterval();
		_isMoving = false;
		_timer = 0f;
	}

	private void Wander() {
		if(_isCharging) {
			return;
		}
		// two-phase of moving and waiting
		_timer += Time.fixedDeltaTime;
		RaycastHit hit;

		if (Physics.Raycast(transform.position, _currentDirection, out hit, _obstacleDetectionRange))
		{
			// detected an obstacle while moving
			// phase transition: moving <-> waiting
			// wait, then move
			Debug.Log("detected an obstacle:" + hit.collider.name);
			if (!_isMoving) {
				if (_timer <= _changeDirectionInterval) return;
				_currentDirection = -_currentDirection + Mathf.Epsilon * RandomDirection();
			}
			SetRandomInterval();
			_timer = 0f;
			_isMoving = !_isMoving;
		}
		else if (_timer > _changeDirectionInterval)
		{
			// phase transition: moving <-> waiting
			if (Vector3.Distance(_spawnPoint, transform.position) > _wanderRange)
			{
				// too far from origin
				_currentDirection = (_spawnPoint - transform.position).normalized;
			}
			else
			{
				// okay to go
				SetRandomDirection();
			}
			SetRandomInterval();
			_timer = 0f;
			_isMoving = !_isMoving;
		}

		if(_isMoving) {
			// rotate
			Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
			Quaternion targetRotationHorizontal = Quaternion.LookRotation(Vector3.Scale(_currentDirection, new Vector3(1, 0, 1)));
			var tempRotation=Quaternion.Slerp(_body.rotation, targetRotationHorizontal, Time.fixedDeltaTime * _rotationSpeed);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed);
			_body.rotation = tempRotation;
			
			// move
			transform.Translate(Vector3.forward * Time.fixedDeltaTime * _wanderSpeed);
		}
		_gun.rotation = Quaternion.Slerp(_gun.rotation, _body.rotation, Time.fixedDeltaTime * _rotationSpeed);
	}

	private void SetRandomInterval() {
		_changeDirectionInterval = Random.Range(_changeDirectionIntervalMin, _changeDirectionIntervalMax);
	}

	private void SetRandomDirection() {
		if(_isMoving) {
			_currentDirection = Vector3.zero;
		} else {
			_currentDirection = (RandomDirection()+ _homingInstinct*(_spawnPoint - transform.position)/_wanderRange).normalized;
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

		// calculate aim angle
		float theta = 0;
		var gunDirectionHorizontal = Vector3.Scale(gunDirection, new Vector3(1, 0, 1)).normalized;
		var y = gunDirection.y;
		if (y != 0)
		{
			theta = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sqrt(1 - Mathf.Pow(y, 2)) / y);
			if (theta < 0) theta += 180;
			// Debug.Log("Aim angle: "+theta);
			if (theta<_minGunAngleDegree)
			{
				gunDirection = Deg2Vec(_minGunAngleDegree,gunDirectionHorizontal);
				_adjustingAimAngle = true;
			}
			else if(theta>_maxGunAngleDegree){
				gunDirection = Deg2Vec(_maxGunAngleDegree, gunDirectionHorizontal);
				_adjustingAimAngle = true;
			}
			else if (_adjustingAimAngle && ((theta >= _adjustingAimAngleRatio * _minGunAngleDegree) || (theta <= _adjustingAimAngleRatio * _maxGunAngleDegree)))
			{
				_adjustingAimAngle=false;
			}
		}

		Vector3 Deg2Vec(float theta, Vector3 horizontal)
		{
			theta = Mathf.Deg2Rad * theta;
			return (Mathf.Cos(theta) * new Vector3(0, 1, 0)) + (horizontal * Mathf.Sin(theta));
		}

		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		Quaternion targetRotationHorizontal = Quaternion.LookRotation(Vector3.Scale(directionToPlayer, new Vector3(1, 0, 1)));
		Quaternion targetRotationGun = Quaternion.LookRotation(gunDirection);
		var tempBodyRotation = Quaternion.Slerp(_body.rotation, targetRotationHorizontal, Time.fixedDeltaTime * _rotationSpeed);
		var tempGunRotation = Quaternion.Slerp(_gun.rotation, targetRotationGun, Time.fixedDeltaTime * _rotationSpeed);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed);
		_body.rotation = tempBodyRotation;
		_gun.rotation = tempGunRotation;

		Vector3 target = Vector3.zero;
		if (Vector3.Scale(_player.transform.position-transform.position, new Vector3(1, 0, 1)).magnitude < _minHorizontalDistance)
		{
			// too close
			target += tempBodyRotation * Vector3.back;
		}
		if (Mathf.Abs((_player.transform.position - transform.position).y) > _minVerticalDistance && _adjustingAimAngle)
		{
			target += tempBodyRotation * ((theta > 90 ? 1 : -1) * Vector3.down);
		}
		var dir = target.normalized * Time.fixedDeltaTime * _wanderSpeed;
		transform.Translate(Quaternion.Inverse(transform.rotation) * dir);
		_spawnPoint += dir;

		if (_isCharging) {
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
		GameObject proj = Instantiate(_projectile, _gun.GetChild(0).position, Quaternion.identity);

		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;

		// proj.transform.rotation = Quaternion.LookRotation(directionToPlayer);
		Rigidbody rb = proj.GetComponent<Rigidbody>();
		rb.velocity = directionToPlayer * _projectileSpeed;

		// issue: 필요할까? Destroy를 많은 곳에서 시도하면 뭔가 문제가 생길 수도 있음
		// Destroy(proj, 5f); 
	}

	private void ChasePlayer()
	{
		Vector3 directionToPlayer = _lastSeenPosition - transform.position;
		Vector3 directionToPlayerHorizontal = Vector3.Scale(directionToPlayer,new Vector3(1,0,1));
		//Vector3 directionToPlayerVertical = directionToPlayer - directionToPlayerHorizontal;
		
		// rotate
		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		Quaternion targetRotationHorizontal = Quaternion.LookRotation(directionToPlayerHorizontal);
		var tempRotation = Quaternion.Slerp(_body.rotation, targetRotationHorizontal, Time.fixedDeltaTime * _rotationSpeed);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed);
		_body.rotation = tempRotation;

		// move
		Vector3 dir = Vector3.zero;
		Vector3 target = Vector3.zero;
		
		// calculate target point to reach (relative position)
		if (directionToPlayerHorizontal.magnitude > _attackRangeHorizontal)
		{
			var diff = directionToPlayerHorizontal.magnitude - _attackRangeHorizontal;
			target += diff * directionToPlayerHorizontal.normalized;
		}
		if (- directionToPlayer.y > _attackRangeVertical)
		{
			var diff = - directionToPlayer.y - _attackRangeVertical;
			target += diff * new Vector3(0,-1,0);
		}
		else if(- directionToPlayer.y < 0f){
			target += directionToPlayer.y * new Vector3(0, 1, 0);
		}

		// calculate the actual translation vector
		dir = target.normalized * Time.fixedDeltaTime * _wanderSpeed;

		transform.Translate(Quaternion.Inverse(transform.rotation) * dir);
		_spawnPoint += dir;
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

	public void ReceiveSkill()
	{
		_isFalling = true;
		_isNeutralized = true;
	}
}

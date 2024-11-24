using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour, IEnemy, ISkillReceiver
{
	private Transform _body;
	private Transform _column;
	private Transform _joint1;
	private Transform _joint2;
	private Transform _head;
	private Transform _gunpoint;

	[Header("Geometry")]
	/* inverse kinematics
	 *            (_d,h)
	 *  |        ↗/|
	 *  |      ↗ / |
	 *  |  c ↗  /  | h
	 *  |  ↗   /   | 
	 *  |↗____/____|
	 *   ↘   /  _d
	 *   _a↘/_b
	 *   
	 *   theta denotes the angle between _a and the vertical line
	 */
	private float _a;
	private float _b;
	private float _d;
	private float _thetaNaught;
	private float _gammaNaught;
	private float _hLimit;

	[SerializeField]
	private float _headVerticalLimitRate;

	[Header("Target")]
	private GameObject _player;
	[Header("Projectile")]
	[SerializeField] private GameObject _projectile;
	[SerializeField] private float _projectileSpeed;

	[Header("Looking")]
	[SerializeField] private float _rotationDirection;
	[SerializeField] private float _rotationSpeedHorizontal;
	[SerializeField] private float _rotationSpeedVertical;
	[SerializeField] private float _rotationLimit;
	[SerializeField] private float _surveilPeriod;
	private float _height;
	private float _surveilElapsedTime = 0;
	private ISurveilStrategy _surveilStrategy;
	// default rotation; regard it as FlyingEnemy._spawnPoint
	private Quaternion _centralRotation = Quaternion.identity;
	private bool _isInitiatingSurveil = false;
	[SerializeField] private float _playerHeightOffset;

	[Header("Attack")]
	[SerializeField] private float _viewAngle;
	[SerializeField] private float _detectionRange;
	[SerializeField] private float _detectionHeight;
	[SerializeField] private float _chargeTime;
	[SerializeField] private float _chargeCooldown;
	private bool _isChargable = true;

	[Header("Chase")]
	[SerializeField] private float _awarenessCoolDown;
	private float _awarenessCoolDownTimer;
	private Vector3 _lastSeenPosition;

	[Header("Neutralized")]
	[SerializeField] private float _neutralizedHeadDegree;
	[SerializeField] private float _neutralizeCoolDown;
	[SerializeField] private float _headDropSpeed;
	[SerializeField] private float _headRestoreSpeed;
	[SerializeField] private float _quaternionEqualityThreshold;
	//private float _neutralizeCoolDownTimer = 0;

	private bool _isCharging = false;
	private bool _headDetached = false;
	private bool _isRestoring = false;

	[SerializeField] private int _maxHp;
	private int _hp;

	public EnemyState State { get; private set; }

	void Awake()
	{
		_hp = _maxHp;

		State = EnemyState.Idle;

		/*
		/* /
		_strategy = new LinearSurveil();
		/* /
		_strategy = new RectangularSurveil();
		/*/
		_surveilStrategy = new CircularSurveil();
		/**/
		_surveilStrategy.SetScale(Mathf.Deg2Rad * _viewAngle/2, _detectionHeight/2);
	}

	private void Start() {
		_body = transform.GetChild(0);
		_column = _body.GetChild(0);
		_joint2 = _column.GetChild(0);
		_joint1 = _joint2.GetChild(0);
		_head = _joint1.GetChild(0);
		_gunpoint = _head.GetChild(0);

		_a = Vector3.Distance(_joint2.position, _joint1.position);
		_b = Vector3.Distance(_joint1.position, _head.position);
		_d = Vector3.Scale(_joint2.position-_head.position,new Vector3(1, 0, 1)).magnitude;
		//Debug.Log("_a: " +_a + ", _b: " +_b + ", _d:" +_d);

		_thetaNaught = Theta(_head.position.y - _joint2.position.y);
		_gammaNaught = Theta(_head.position.y - _joint2.position.y);

		if(_headVerticalLimitRate > 1) _headVerticalLimitRate = 1;
		if(_headVerticalLimitRate < 0) _headVerticalLimitRate = 0;
		_hLimit = Mathf.Sqrt(Mathf.Pow(_a + _b, 2) - Mathf.Pow(_d, 2)) * _headVerticalLimitRate;

		_player = GameObject.Find("Player");

		_centralRotation = _column.rotation;

		State = EnemyState.Idle;
		BeforeSurveil();
	}

	private void Update() {
		if (_headDetached && !_isRestoring)
		{
			_head.rotation = Quaternion.Slerp(_head.rotation,  _column.rotation * Quaternion.Euler(_neutralizedHeadDegree, 0, 0), Time.deltaTime * _headDropSpeed);
		}

		/*
		if(_headDetached) {
			transform.rotation = Quaternion.Euler(0, 90, 0);
			return;
		}

		if(IsPlayerDetected()) {
			DetectPlayer();
		} else {
			RotateTurret();
		}
		*/
	}

	private void FixedUpdate()
	{
		if (_headDetached) return;

		bool isPlayerDetected = IsPlayerDetected();
		Debug.Log("isPlayerDetected: " + isPlayerDetected);

		_awarenessCoolDownTimer -= Time.fixedDeltaTime;
		if (isPlayerDetected) { 
			_awarenessCoolDownTimer = _awarenessCoolDown; 
		}

		switch (State)
		{
			case EnemyState.Idle:
				if (isPlayerDetected)
				{
					// Idle -> Aware
					State = EnemyState.Aware;
					DetectPlayer();
					break;
				}
				Surveil();
				break;
			case EnemyState.Aware:
				if (!isPlayerDetected)
				{
					// cannot see the player
					if(_awarenessCoolDownTimer > 0)
					{
						// Aware -> Follow
						State = EnemyState.Follow;
						ChasePlayer();
						break;
					}
					// Aware -> Idle
					State = EnemyState.Idle;
					BeforeSurveil();
					Surveil();
					break;
				}
				DetectPlayer();
				break;
			case EnemyState.Follow:
				if (_awarenessCoolDownTimer <= 0)
				{
					// time's up
					// Follow -> Idle
					State = EnemyState.Idle;
					BeforeSurveil();
					Surveil();
					break;
				}
				if (isPlayerDetected)
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
	}

	private void BeforeSurveil()
	{
		_surveilElapsedTime = 0;
		_isInitiatingSurveil = true;
	}

	private void Surveil()
	{
		if (_headDetached) return;

		if (_isInitiatingSurveil)
		{
			// move to the starting point
			Vector2 planarTargetRotation = _surveilStrategy.Route(_surveilElapsedTime);
			Vector2 planarTargetRotationHorizontal = Vector2.Scale(planarTargetRotation, new Vector2(1, 0));
			Quaternion horizontalRelativeRotation = _column.rotation * Quaternion.Inverse(_centralRotation);
			Vector2 planarHorizontalRelativeRotation = CylindricalConverter.Cylinder2Plane(horizontalRelativeRotation * new Vector3(0, 0, 1));

			var planarTargetRelativeHorizontal = planarTargetRotationHorizontal - Vector2.Scale(planarHorizontalRelativeRotation, new Vector2(1, 0));
			var planarTargetRelativeVertical = new Vector2(0, planarTargetRotation.y - _height);

			var cos = Vector3.Dot(CylindricalConverter.Plane2Cylinder(planarTargetRelativeHorizontal + planarTargetRelativeVertical), new Vector3(0, 0, 1));
			if (cos > _quaternionEqualityThreshold)
			{
				_isInitiatingSurveil = false;
			}
			else
			{

				// horizontal
				var temp = Time.deltaTime * _rotationSpeedHorizontal * planarTargetRelativeHorizontal;
				LookHorizontal(_column.rotation * CylindricalConverter.Plane2Cylinder(temp));

				// vertical
				float verticalUnit = Time.fixedDeltaTime * _rotationSpeedVertical;
				LookVertical(_height + verticalUnit * planarTargetRelativeVertical.y);

				return;
			}
		}

		_surveilElapsedTime += Time.fixedDeltaTime;
		if (_surveilElapsedTime > _surveilPeriod) _surveilElapsedTime -= _surveilPeriod;
		Look(_centralRotation * CylindricalConverter.Plane2Cylinder(_surveilStrategy.Route(_surveilElapsedTime / _surveilPeriod)));
	}

	private bool IsPlayerDetected() {
		Vector3 directionToPlayerHorizontal = Vector3.Scale(_player.transform.position - _column.position, new Vector3(1, 0, 1));
		float directionToPlayerVertical = (_player.transform.position - _column.position).y + _playerHeightOffset;

		float angleToPlayer = Vector3.Angle(_centralRotation * transform.forward, directionToPlayerHorizontal.normalized);
		
		if (directionToPlayerVertical > _detectionHeight / 2 || directionToPlayerVertical < -_detectionHeight / 2) return false;
		
		//if (directionToPlayerVertical > _headVerticalLimitRate * _hLimit) directionToPlayerVertical = _headVerticalLimitRate * _hLimit;
		//if (directionToPlayerVertical < -_headVerticalLimitRate * _hLimit) directionToPlayerVertical = -_headVerticalLimitRate * _hLimit;

		if (angleToPlayer < _viewAngle / 2) {
			var startingPoint = _column.position + directionToPlayerVertical * new Vector3(0, 1, 0);
			if (Physics.Raycast(startingPoint, directionToPlayerHorizontal, out RaycastHit hit, _detectionRange)) {
				Debug.Log("Hit:" + hit.collider.name);
				if (hit.collider.gameObject == _player)
				{
					_lastSeenPosition = _player.transform.position;
					return true;
				}
			}
		}
		return false;
	}

	private void DetectPlayer() {
		if (_headDetached) return;

		Vector3 directionToPlayer = _player.transform.position - _column.position;
		if ((_column.rotation * new Vector3(0, 0, 1)) == -directionToPlayer.normalized)
		{
			// opposite
			directionToPlayer += new Vector3(Mathf.Epsilon, 0, Mathf.Epsilon);
		}

		Vector3 directionToPlayerHorizontal = Vector3.Scale(directionToPlayer, new Vector3(1, 0, 1));
		float directionToPlayerVertical = directionToPlayer.y + _playerHeightOffset;
		Quaternion targetRotationHorizontal = Quaternion.LookRotation(directionToPlayerHorizontal) * Quaternion.Inverse(_column.rotation);
		Vector2 planarTargetRotationHorizontal = CylindricalConverter.Cylinder2Plane(targetRotationHorizontal * new Vector3(0, 0, 1));

		// horizontal
		var temp = Time.deltaTime * _rotationSpeedHorizontal * planarTargetRotationHorizontal;
		LookHorizontal(_column.rotation * CylindricalConverter.Plane2Cylinder(temp));
		_centralRotation = _column.rotation;

		// vertical
		float verticalUnit = Time.fixedDeltaTime * _rotationSpeedVertical;
		LookVertical(_height + verticalUnit * (directionToPlayerVertical - _height));

		if(_isCharging) {
			return;
		}
		if(_isChargable) {
			_isCharging = true;
			_isChargable = false;
			Debug.Log("charge");
			StartCoroutine(ChargeAndFire());
		}
	}

	private IEnumerator ChargeAndFire() {
		yield return new WaitForSeconds(_chargeTime);

		FireProjectile();
		
		_isCharging = false;
		StartCoroutine(ReChargable());
	}

	private IEnumerator ReChargable() {
		yield return new WaitForSeconds(_chargeCooldown);

		_isChargable = true;
	}

	private void FireProjectile() {
		if (_headDetached) return;

		GameObject proj = Instantiate(_projectile, _gunpoint.position, Quaternion.identity);

		Vector3 directionToPlayer = _column.rotation * new Vector3(0, 0, 1);

		// proj.transform.rotation = Quaternion.LookRotation(directionToPlayer);
		Rigidbody rb = proj.GetComponent<Rigidbody>();
		rb.velocity = directionToPlayer * _projectileSpeed;

		// issue: 필요할까? Destroy를 많은 곳에서 시도하면 뭔가 문제가 생길 수도 있음
		// Destroy(proj, 5f); 
	}

	private void ChasePlayer()
	{
		return;
		/*
		if (_headDetached) return;

		// move to the starting point
		var lookRotation = Quaternion.LookRotation(Vector3.Scale(_lastSeenPosition - _column.position, new Vector3(1, 0, 1)));
		Vector2 planarTargetRotation = CylindricalConverter.Cylinder2Plane(lookRotation * Quaternion.Inverse(_centralRotation) * new Vector3(0,0,1));
		Vector2 planarTargetRotationHorizontal = Vector2.Scale(planarTargetRotation, new Vector2(1, 0));
		Quaternion horizontalRelativeRotation = _column.rotation * Quaternion.Inverse(_centralRotation);
		Vector2 planarHorizontalRelativeRotation = CylindricalConverter.Cylinder2Plane(horizontalRelativeRotation * new Vector3(0, 0, 1));

		var planarTargetRelativeHorizontal = planarTargetRotationHorizontal - Vector2.Scale(planarHorizontalRelativeRotation, new Vector2(1, 0));
		var planarTargetRelativeVertical = new Vector2(0, planarTargetRotation.y - _height);

		// horizontal
		var temp = Time.deltaTime * _rotationSpeedHorizontal * planarTargetRelativeHorizontal;
		LookHorizontal(_column.rotation * CylindricalConverter.Plane2Cylinder(temp));

		// vertical
		float verticalUnit = Time.fixedDeltaTime * _rotationSpeedVertical;
		LookVertical(_height + verticalUnit * planarTargetRelativeVertical.y);
		*/
	}

	private void OnDrawGizmosSelected()
	{
		if (_column == null) return;
		Gizmos.color = Color.red;
		Gizmos.DrawRay(_column.position, _detectionRange * (_centralRotation * new Vector3(0, 0, 1)));
		Gizmos.color = Color.green;
		Gizmos.DrawRay(_head.position, _detectionRange * (_column.rotation * new Vector3(0, 0, 1)));
	}

	private float Alpha(float h)
	{
		// h stands for vertical coordinate
		float c = new Vector2(_d, h).magnitude;
		return JCos.Alpha(_a,_b,c);
	}

	private float Beta(float h)
	{
		// h stands for vertical coordinate
		float c = new Vector2(_d, h).magnitude;
		return JCos.Beta(_a, _b, c);
	}

	private float Gamma(float h)
	{
		// h stands for vertical coordinate
		float c = new Vector2(_d, h).magnitude;
		return JCos.Gamma(_a, _b, c);
	}

	private float Theta(float h) {
		return JCos.Acos(h / new Vector2(_d, h).magnitude) + Beta(h);
	}

	private void Look(Vector3 v)
	{
		// cylindrical; y - axis stands for h
	
		if(_headDetached) return;

		LookHorizontal(v);

		var h = v.y;
		LookVertical(h);
	}

	private void LookHorizontal(Vector3 v)
	{
    if (_headDetached) return;
    
		_column.rotation = Quaternion.LookRotation(Vector3.Scale(v, new Vector3(1, 0, 1)));
	}

	private void LookVertical(float h)
	{
		if (_headDetached) return;

		if (h > _hLimit) h = _hLimit;
		if (h < - _hLimit) h = - _hLimit;
		var theta = Theta(h);
		var dTheta = theta - _thetaNaught;
		var gamma = Gamma(h);
		var dGamma = gamma - _gammaNaught;
		//Debug.Log("dTheta: " + dTheta + ", dGamma: " + dGamma);
		_joint2.localRotation = Quaternion.Euler( Mathf.Rad2Deg * dTheta, 0, 0);
		_joint1.localRotation = Quaternion.Euler( Mathf.Rad2Deg * dGamma, 0, 0);
		_head.localRotation = Quaternion.Euler(- Mathf.Rad2Deg * (dTheta + dGamma), 0, 0);

		_height = h;
	}

	public void ReceiveSkill() {
		if (_headDetached) return;
		_headDetached = true;
		StartCoroutine(Restore());
		//_head.rotation = Quaternion.Euler(_neutralizedHeadDegree, 0, 0) * _column.rotation;
	}

	public IEnumerator Restore()
	{
		yield return new WaitForSeconds(_neutralizeCoolDown);
		_isRestoring = true;
		var relativeAngle = Quaternion.Dot(_head.rotation, _column.rotation);
		while (relativeAngle < _quaternionEqualityThreshold)
		{
			relativeAngle = Quaternion.Dot(_head.rotation, _column.rotation);
			_head.rotation = Quaternion.Slerp(_head.rotation, _column.rotation, Time.deltaTime * _headRestoreSpeed);
			yield return null;
		}
		Debug.Log("Restored");
		_isRestoring = false;
		_headDetached = false;
		_head.rotation = _column.rotation;
	}

	public void OnHit()
	{
		// indestructible
		/*
		if (--_hp <= 0)
		{
			OnDeath();
		}
		*/
		// hit effect goes here: particle, knockback, etc.
	}

	public void OnDeath()
	{
		// death animation goes here; must wait till the animation to be finished before destroying
		GameManager.Instance.UnregisterEnemy(gameObject);
		Destroy(gameObject);
	}
}

public class JCos
{
	public static float Clamp(float x)
	{
		if (x > 1) return 1;
		if(x < -1) return -1;
		return x;
	}

	public static float Acos(float x)
	{
		return Mathf.Acos(Clamp(x));
	}

	public static float Alpha(float a, float b, float c)
	{
		// law of cosines: a^2 = b^2 + c^2 - 2bc cos(alpha)
		// a, b, c stands for lengths (of sides)
		// alpha stands for the angle BAC

		// returns alpha = acos((b^2 + c^2 - a^2) / 2bc) (0 < alpha < pi)
		return Acos((Mathf.Pow(b, 2) + Mathf.Pow(c, 2) - Mathf.Pow(a, 2)) / (2 * b * c));
	}
	public static float Beta(float a, float b, float c)
	{
		// returns beta = acos((a^2 + c^2 - b^2) / 2ac) (0 < beta < pi)
		return Acos((Mathf.Pow(a, 2) + Mathf.Pow(c, 2) - Mathf.Pow(b, 2)) / (2 * a * c));
	}

	public static float Gamma(float a, float b, float c)
	{
		// returns gamma = acos((a^2 + b^2 - c^2) / 2ab) (0 < gamma < pi)
		return Acos((Mathf.Pow(a, 2) + Mathf.Pow(b, 2) - Mathf.Pow(c, 2)) / (2 * a * b));
	}

	public static float Alpha(Vector3 v)
	{
		return Alpha(v.x,v.y,v.z);
	}

	public static float Beta(Vector3 v)
	{
		return Beta(v.x, v.y, v.z);
	}

	public static float Gamma(Vector3 v)
	{
		return Gamma(v.x, v.y, v.z);
	}
}
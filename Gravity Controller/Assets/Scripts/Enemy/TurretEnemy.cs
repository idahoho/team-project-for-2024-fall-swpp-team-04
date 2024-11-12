using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour, IEnemy
{
	[Header("Target")]
	[SerializeField] private GameObject _player;
	[Header("Projectile")]
	[SerializeField] private GameObject _projectile;
	[SerializeField] private float _projectileSpeed;

	[Header("Looking")]
	[SerializeField] private float _rotationDirection;
	[SerializeField] private float _rotationSpeed;
	[SerializeField] private float _rotationLimit;

	[Header("Attack")]
	[SerializeField] private float _viewAngle;
	[SerializeField] private float _detectionRange;
	[SerializeField] private float _chargeTime;
	[SerializeField] private float _chargeCooldown;
	private bool _isChargable = true;

	private bool _isCharging = false;
	private bool _headDetached = false;

	[SerializeField] private int _maxHp;
	private int _hp;

	void Awake()
	{
		_hp = _maxHp;
	}

	private void Start() {

	}

	private void Update() {
		if(_headDetached) {
			transform.rotation = Quaternion.Euler(0, 90, 0);
			return;
		}

		if(IsPlayerDetected()) {
			DetectPlayer();
		} else {
			RotateTurret();
		}
	}

	private void RotateTurret() {
		transform.Rotate(0, _rotationDirection * _rotationSpeed * Time.deltaTime, 0);

		if (Mathf.Abs(transform.localEulerAngles.y) >= _rotationLimit) {
			_rotationDirection *= -1;
		}
	}

	private bool IsPlayerDetected() {
		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
		float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

		if (angleToPlayer < _viewAngle / 2) {
			if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, _detectionRange)) {
				return hit.collider.gameObject == _player;
			}
		}
		return false;
	}

	private void DetectPlayer() {
		Vector3 directionToPlayer = Vector3.Scale(_player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;

		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

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
		GameObject proj = Instantiate(_projectile, transform.position + transform.forward * 2, Quaternion.identity);

		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;

		// proj.transform.rotation = Quaternion.LookRotation(directionToPlayer);
		Rigidbody rb = proj.GetComponent<Rigidbody>();
		rb.velocity = directionToPlayer * _projectileSpeed;

		// issue: 필요할까? Destroy를 많은 곳에서 시도하면 뭔가 문제가 생길 수도 있음
		// Destroy(proj, 5f); 
	}

	public void ReceiveSkill() {
		_headDetached = true;
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

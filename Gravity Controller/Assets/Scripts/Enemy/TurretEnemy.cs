using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
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

	private bool _isCharging = false;
	private bool _headDetached = false;

	private void Start() {

	}

	private void Update() {
		if(_headDetached) {
			transform.rotation = Quaternion.Euler(0, 90, 0);
			return;
		}

		if(IsPlayerInSight()) {
			if (!_isCharging) {
				StartCoroutine(ChargeAndFire());
			}
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

	private bool IsPlayerInSight() {
		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
		float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
		// issue: 이거 제대로 감지가 되나?
		// 아니면 얘도 그냥 일정 범위 내에 들어오면 조준하는 걸로 하는 것도 괜찮을 듯
		if (angleToPlayer < _viewAngle / 2) {
			if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, _detectionRange)) {
				return hit.collider.gameObject == _player;
			}
		}
		return false;
	}

	

	private IEnumerator ChargeAndFire() {
		_isCharging = true;

		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		float elapsedTime = 0f;

		while(elapsedTime < _chargeTime) {
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / _chargeTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		FireProjectile();
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

	public void ReceiveSkill() {
		_headDetached = true;
	}
}

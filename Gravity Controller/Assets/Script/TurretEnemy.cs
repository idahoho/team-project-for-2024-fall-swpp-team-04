using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
	[SerializeField] private GameObject _player;
	[SerializeField] private float _detectionRange = 10f;
	[SerializeField] private float _viewAngle = 50f;
	[SerializeField] private float _chargeTime = 3f;
	[SerializeField] private float _rotationSpeed = 30f;
	private bool _isCharging = false;
	private bool _headDetached = false;

	[SerializeField] private float _rotationDirection = 1f;
	[SerializeField] private float _rotationLimit = 100f;
	[SerializeField] private GameObject _projectile;

	private void Start()
	{
		_player = GameObject.FindWithTag("Player");
	}

	private void Update()
	{
		if (_headDetached)
		{
			transform.rotation = Quaternion.Euler(0, 90, 0);
			return;
		}

		if (IsPlayerInSight())
		{
			if (!_isCharging)
			{
				StartCoroutine(ChargeAndFire());
			}
		}
		else
		{
			RotateTurret();
		}
	}

	private void RotateTurret()
	{
		transform.Rotate(0, _rotationDirection * _rotationSpeed * Time.deltaTime, 0);

		
		if (Mathf.Abs(transform.localEulerAngles.y) >= _rotationLimit)
		{
			_rotationDirection *= -1;
		}
	}

	private bool IsPlayerInSight()
	{
		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
		float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

		if (angleToPlayer < _viewAngle / 2)
		{
			if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, _detectionRange))
			{
				return hit.collider.gameObject == _player;
			}
		}
		return false;
	}

	private IEnumerator ChargeAndFire()
	{
		_isCharging = true;

		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		float elapsedTime = 0f;

		while (elapsedTime < _chargeTime)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / _chargeTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		FireProjectile(directionToPlayer);
		_isCharging = false;
	}

	private void FireProjectile(Vector3 direction)
	{
		GameObject _proj = Instantiate(_projectile, transform.position + Vector3.up * 2f, Quaternion.identity);
		Rigidbody rb = _proj.GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = _proj.AddComponent<Rigidbody>();
		}
		rb.velocity = direction.normalized * 500f;

		Destroy(_proj, 5f);
	}

	public void ReceiveSkill()
	{
		_headDetached = true;
	}
}

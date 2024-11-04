using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
	[SerializeField] private GameObject _player;
	[SerializeField] private float _speed;
	[SerializeField] private float _attackRange;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _changeDirectionInterval;
	[SerializeField] private GameObject _projectile;
	[SerializeField] private float _chargeTime = 2f; // Charging time before firing
	[SerializeField] private float _obstacleDetectionRange = 200f;
	[SerializeField] private float _projectileSpeed = 500f;
	private Vector3 _spawnPoint;
	private Vector3 _currentDirection;
	private float _timer;
	private bool _isCharging = false; // Indicates if the enemy is currently charging
	private float _chargeCooldown = 0f; // Timer for charge cooldown
	
	private void Start()
	{
		_player = GameObject.FindWithTag("Player");

		_spawnPoint = transform.position;
		
		SetRandomDirection();
		_timer = _changeDirectionInterval;
	}

	private void Update()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if (distanceToPlayer < _attackRange && !_isCharging && _chargeCooldown <= 0)
		{
			StartCoroutine(ChargeAndFire());
		}
		else
		{
			Wander();
		}

		// Update charge cooldown
		if (_chargeCooldown > 0)
		{
			_chargeCooldown -= Time.deltaTime; // Reduce the cooldown timer
		}
	}

	private void Wander()
	{
		_timer += Time.deltaTime;
		if (_timer >= _changeDirectionInterval)
		{
			SetRandomDirection();
			_timer = 0f;
		}

		if (Vector3.Distance(_spawnPoint, transform.position) > _wanderRange)
		{
			Vector3 directionToSpawn = (_spawnPoint - transform.position).normalized;
			_currentDirection = directionToSpawn;
		}

		if (!_isCharging)
		{
			if (Physics.Raycast(transform.position, _currentDirection, _obstacleDetectionRange))
			{
				SetRandomDirection();
			}
			else
			{
				transform.Translate(_currentDirection * _speed * Time.deltaTime, Space.World);

				if (_currentDirection != Vector3.zero)
				{
					Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
					transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _speed);
				}
			}
		}
		
	}

	private IEnumerator ChargeAndFire()
	{
		_isCharging = true;

		Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;

		Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _speed);

		yield return new WaitForSeconds(_chargeTime);	

		FireProjectile(directionToPlayer);
		
		_chargeCooldown = 2f;	

		_isCharging = false;
	}

	private void FireProjectile(Vector3 directionToPlayer)
	{
		GameObject _proj = Instantiate(_projectile, transform.position, Quaternion.identity);

		_proj.transform.rotation = Quaternion.LookRotation(directionToPlayer);

		Rigidbody rb = _proj.GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = _proj.AddComponent<Rigidbody>();
		}

		rb.velocity = directionToPlayer * _projectileSpeed; 

		Destroy(_proj, 5f); 
	}


	private void SetRandomDirection()
	{
		float angle = Random.Range(0, 360);
		_currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Random.Range(-1f, 1f), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}
}

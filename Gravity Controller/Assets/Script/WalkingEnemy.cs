using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingEnemy : MonoBehaviour
{
	[SerializeField] private GameObject _player;
	[SerializeField] private float _speed;
	[SerializeField] private float _attackRange;
	[SerializeField] private float _followingRange;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _changeDirectionInterval;
	[SerializeField] private float _obstacleDetectionRange = 200f;

	private Animator _animator;
	private Vector3 _spawnPoint;
	private Vector3 _currentDirection;
	private float _timer;
	private bool _isFollowingPlayer = false;
	private bool _isAttacking = false;

	void Start()
	{
		_animator = GetComponent<Animator>();
		_spawnPoint = transform.position;
		SetRandomDirection();
		_timer = _changeDirectionInterval;
	}

	void Update()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if (!_isFollowingPlayer && distanceToPlayer < _followingRange)
		{
			_isFollowingPlayer = true;
			_animator.SetBool("FollowPlayer", true);
			_speed += 50;
		}

		if (_isFollowingPlayer && distanceToPlayer < _attackRange)
		{
			AttackPlayer();
		}
		else if (_isFollowingPlayer && distanceToPlayer >= _attackRange)
		{
			FollowPlayer();
		}
		else if (!_isFollowingPlayer)
		{
			Wander();
		}
	}

	private void FollowPlayer()
	{
		if (_isAttacking) return;

		Vector3 direction = (_player.transform.position - transform.position).normalized;
		transform.position += direction * _speed * Time.deltaTime;
		transform.LookAt(_player.transform.position);
	}

	private void AttackPlayer()
	{
		if (_isAttacking) return;

		_isAttacking = true;
		_animator.SetBool("AttackPlayer", true);

		Invoke("ResetAttack", 1f);
	}

	private void ResetAttack()
	{
		_isAttacking = false;
		_animator.SetBool("AttackPlayer", false);
	}

	public void AttackHitCheck()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if (distanceToPlayer <= _attackRange)
		{
			Debug.Log("Attack Successful");

			PlayerMovement playerController = _player.GetComponent<PlayerMovement>();
			if (playerController != null)
			{
				// playerController.OnHit(); 
			}
		}
		else
		{
			Debug.Log("Attack Fail");
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

	private void SetRandomDirection()
	{
		float angle = Random.Range(0, 360);
		_currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, _currentDirection * _obstacleDetectionRange);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(_spawnPoint, _wanderRange);
	}
}

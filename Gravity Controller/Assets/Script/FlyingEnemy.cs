using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
	[SerializeField] private GameObject _player;
	[SerializeField] private float _speed;
	[SerializeField] private float _attackRange;
	[SerializeField] private float _followingRange;
	[SerializeField] private float _wanderRange;
	[SerializeField] private float _wanderTime;

	private Animator _animator;
	private Vector3 _wanderTarget;
	private float _wanderTimer;
	private bool _isFollowingPlayer = false;
	private bool _isAttacking = false;

	void Start()
	{
		_animator = GetComponent<Animator>();
		SetNewWanderTarget();
	}

	void Update()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		// Check if within following range
		if (!_isFollowingPlayer && distanceToPlayer < _followingRange)
		{
			_isFollowingPlayer = true;
			_animator.SetBool("FollowPlayer", true);
			_speed += 50;
		}

		// Attack if within attack range
		if (_isFollowingPlayer && distanceToPlayer < _attackRange)
		{
			AttackPlayer();
		}
		// Follow player if within following range but outside attack range
		else if (_isFollowingPlayer && distanceToPlayer >= _attackRange)
		{
			FollowPlayer();
		}
		// Wander if not following player
		else if (!_isFollowingPlayer)
		{
			Wander();
		}
	}

	private void FollowPlayer()
	{
		if (_isAttacking) return; // Stop moving if attacking

		// Calculate direction without moving closer than attack range
		Vector3 direction = (_player.transform.position - transform.position).normalized;
		float distanceToStop = _attackRange - 1.0f; // Keep some distance before attack range
		if (Vector3.Distance(transform.position, _player.transform.position) > distanceToStop)
		{
			transform.position += direction * _speed * Time.deltaTime;
			transform.LookAt(_player.transform.position);
		}
	}

	private void AttackPlayer()
	{
		if (_isAttacking) return;

		_isAttacking = true;
		_animator.SetBool("AttackPlayer", true);

		PlayerMovement playerController = _player.GetComponent<PlayerMovement>();
		if (playerController != null)
		{
			Debug.Log("Hit");
			//playerController.OnHit(); // Call player's OnHit method
		}

		Invoke("ResetAttack", 1f);
	}

	private void ResetAttack()
	{
		_isAttacking = false;
		_animator.SetBool("AttackPlayer", false);
	}

	private void Wander()
	{
		if (_wanderTimer <= 0)
		{
			_wanderTimer = _wanderTime;
			SetNewWanderTarget();
		}

		Vector3 direction = (_wanderTarget - transform.position).normalized;
		transform.position += direction * _speed * Time.deltaTime;
		_wanderTimer -= Time.deltaTime;

		if (Vector3.Distance(transform.position, _wanderTarget) < 0.5f)
		{
			SetNewWanderTarget();
		}
	}

	private void SetNewWanderTarget()
	{
		Vector3 randomPoint = new Vector3(
			Random.Range(-_wanderRange, _wanderRange),
			Random.Range(-_wanderRange, _wanderRange), // Random Y movement for flying
			Random.Range(-_wanderRange, _wanderRange)
		);

		_wanderTarget = transform.position + randomPoint;
	}
}

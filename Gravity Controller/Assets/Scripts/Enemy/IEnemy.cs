using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
	public EnemyState State { get; }
	public void OnHit();
	public void OnDeath();
}

public enum EnemyState
{
	Idle,
	Aware,
	Follow,
}
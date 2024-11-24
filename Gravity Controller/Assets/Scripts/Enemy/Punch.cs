using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punch : MonoBehaviour
{
	private WalkingEnemy _walkingEnemy;

	void Start()
	{
		_walkingEnemy = transform.parent.GetComponent<WalkingEnemy>();
	}

	public void Attack()
	{
		if( _walkingEnemy != null ) _walkingEnemy.AttackHitCheck();
	}
}

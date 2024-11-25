using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchCollider : MonoBehaviour
{
	private WalkingEnemy _walkingEnemy;

	// Start is called before the first frame update
	void Start()
    {
		_walkingEnemy = transform.parent.parent.parent.GetComponent<WalkingEnemy>();
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			_walkingEnemy.SetAttackSuccess(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			_walkingEnemy.SetAttackSuccess(false);
		}
	}
}

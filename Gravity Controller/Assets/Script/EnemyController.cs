using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
	[SerializeField] private int _hp;
	
	public void OnHit()
	{
		_hp--;
		if (_hp <= 0)
		{
			GameManager.Instance.UnregisterEnemy(gameObject);
			Destroy(gameObject);
		}
	}


}

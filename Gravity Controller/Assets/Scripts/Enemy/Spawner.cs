using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject _player;
	public GameObject[] _enemies;
	private int _count = 0;
    void Start()
    {
	
	}

    // Update is called once per frame
    void Update()
    {
        if(_count == 0 && _player.transform.position.x < 2500)
		{
			ActivateEnemiesInTrigger(_enemies[_count]);
			_count++;
		}
		if (_count == 1 && _player.transform.position.x < 0)
		{
			ActivateEnemiesInTrigger(_enemies[_count]);
			_count++;
		}
		

	}

	private void ActivateEnemiesInTrigger(GameObject trigger)
	{
		trigger.SetActive(true);

		foreach (Transform enemyTransform in trigger.transform)
		{
			GameObject enemy = enemyTransform.gameObject;
			GameManager.Instance.RegisterEnemy(enemy);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
	[SerializeField] private List<GameObject> _spawnerObjects;
	[SerializeField] private List<float> _spawnTimes;
	[SerializeField] private List<int> _enemyCounts; 
	[SerializeField] private List<float> _customDelays; 
	private List<IEnemyFactory> _spawners = new List<IEnemyFactory>();
	private int _currentSpawnerIndex = 0;

	void Start()
	{
		foreach (var obj in _spawnerObjects)
		{
			var spawner = obj.GetComponent<IEnemyFactory>();
			if (spawner != null)
			{
				_spawners.Add(spawner);
			}
		}

		if (_spawners.Count > 0)
		{
			Invoke(nameof(SpawnEnemies), _spawnTimes[_currentSpawnerIndex]);
		}
	}

	private void SpawnEnemies()
	{
		int enemyCount = _enemyCounts[_currentSpawnerIndex];
		float customDelay = _customDelays[_currentSpawnerIndex];

		for (int i = 0; i < enemyCount; i++)
		{
			if (_spawners[_currentSpawnerIndex] is DelayedSpawner delayedSpawner)
			{
				delayedSpawner.SetTimer(i * customDelay);
			}
			else
			{
				_spawners[_currentSpawnerIndex].SpawnEnemy();
			}
		}

		_currentSpawnerIndex = (_currentSpawnerIndex + 1) % _spawners.Count;

		Invoke(nameof(SpawnEnemies), _spawnTimes[_currentSpawnerIndex]);
	}
}

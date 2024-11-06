using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	private List<GameObject> _activeEnemies = new List<GameObject>();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void RegisterEnemy(GameObject enemy)
	{
		if (!_activeEnemies.Contains(enemy))
		{
			_activeEnemies.Add(enemy);
		}
	}

	public void UnregisterEnemy(GameObject enemy)
	{
		if (_activeEnemies.Contains(enemy))
		{
			_activeEnemies.Remove(enemy);
		}
	}

	public List<GameObject> GetActiveEnemies()
	{
		return _activeEnemies;
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSpawnerContainer : MonoBehaviour, IEnemyFactory
{
	// For batch-spawning objects: put Spawner objects as children

	List<IEnemyFactory> _spawners=new List<IEnemyFactory>();
	private GameObject _player;
	[SerializeField] private float _detectionRadius;
	// Start is called before the first frame update
	void Start()
    {
		_player = GameObject.Find("Player");
		foreach (Transform child in transform)
		{
			_spawners.Add(child.gameObject.GetComponent<IEnemyFactory>());
		}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		// used FixedUpdate to detect player more accurately:
		// if player moves so quickly w/ respect to the framerate that it moves through the detection range in a frame,
		// Update() might not detect the player 
		if (Vector3.Magnitude(_player.transform.position - transform.position) < _detectionRadius)
		{
			// player detected
			SpawnEnemy();
			// destroy itself to prevent spamming enemies
			Destroy(gameObject);
		}
	}

	public void SpawnEnemy()
	{
		foreach (var spawner in _spawners)
		{
			spawner.SpawnEnemy();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSpawner : MonoBehaviour, IEnemyFactory
{
	public GameObject toSpawn;
    private GameObject _player;
	[SerializeField] private float _detectionRadius;

    void Start()
    {
		_player =GameObject.Find("Player");
		transform.localScale = Vector3.zero;
	}

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
		GameManager.Instance.RegisterEnemy(Instantiate(toSpawn,transform.position, transform.rotation));
	}
}

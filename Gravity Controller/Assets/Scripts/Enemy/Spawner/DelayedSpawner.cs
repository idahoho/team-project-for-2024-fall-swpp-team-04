using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class DelayedSpawner : MonoBehaviour
{
	public GameObject toSpawn;
	[SerializeField] float _delay;

	void Start()
	{
		transform.localScale = Vector3.zero;
	}

	public void SetTimer()
	{
		Invoke("SpawnEnemy", _delay);
	}

	public void SetTimer(float customDelay)
	{
		Invoke("SpawnEnemy", customDelay);
	}

	public void SpawnEnemy()
	{
		GameManager.Instance.RegisterEnemy(Instantiate(toSpawn, transform.position, transform.rotation));
	}
}

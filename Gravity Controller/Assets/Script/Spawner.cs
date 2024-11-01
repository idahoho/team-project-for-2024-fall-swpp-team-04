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
        if(_count<1 && _player.transform.position.x < 2500)
		{
			_enemies[_count].SetActive(true);
			_count++;
		}
		if (_count<2 && _player.transform.position.x < 0)
		{
			_enemies[_count].SetActive(true);
			_count++;
		}
		

	}
}

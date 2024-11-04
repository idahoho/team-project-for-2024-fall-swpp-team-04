using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
	[SerializeField] private int _hp;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			PlayerMovement playerController = collision.collider.GetComponent<PlayerMovement>();
			if (playerController != null)
			{
				playerController.OnHit(); 
				Debug.Log("HitPlayer");
				Destroy(gameObject);
			}
		}
	}
}

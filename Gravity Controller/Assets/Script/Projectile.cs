using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player")) // 태그로 비교
		{
			PlayerMovement playerController = collision.collider.GetComponent<PlayerMovement>();
			if (playerController != null)
			{
				// playerController.OnHit(); // 주석 해제하여 공격 시 호출
				Debug.Log("HitPlayer");
				Destroy(gameObject); // 프로젝타일 삭제
			}
		}
	}
}

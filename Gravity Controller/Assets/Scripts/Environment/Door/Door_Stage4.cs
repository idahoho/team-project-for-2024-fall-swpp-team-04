using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Stage4 : MonoBehaviour, IDoor
{
	public void Open()
	{
		// deleteCeils 안의 모든 자식 오브젝트 가져오기
		foreach (Transform child in transform)
		{
			// 자식 오브젝트의 자식 탐색
			foreach (Transform grandchild in child)
			{
				// BoxCollider가 있는 경우 제거
				BoxCollider boxCollider = grandchild.GetComponent<BoxCollider>();
				if (boxCollider != null)
				{
					Destroy(boxCollider); // BoxCollider 삭제
				}
			}
		}
	}

	public void Close()
	{
		return;
	}
}
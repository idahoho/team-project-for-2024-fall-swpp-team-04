using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentAttackReceiver : MonoBehaviour, IAttackReceiver
{
	[SerializeField] private int _depth;
	public void OnHit()
	{
		Transform target = transform;
		for (int i = 0; i < _depth; i++)
		{
			target = target.parent;
		}
		var targetAttackReceiver = target.GetComponent<IAttackReceiver>();
		if (targetAttackReceiver == null) return;
		targetAttackReceiver.OnHit();
	}
}

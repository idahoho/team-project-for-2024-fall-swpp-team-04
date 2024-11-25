using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentSkillReceiver : MonoBehaviour, ISkillReceiver
{
	[SerializeField] private int _depth;
	public void ReceiveSkill()
	{
		Transform target = transform;
		for(int i=0; i<_depth; i++)
		{
			target = target.parent;
		}
		var targetSkillReceiver = target.GetComponent<ISkillReceiver>();
		if (targetSkillReceiver == null) return;
		targetSkillReceiver.ReceiveSkill();
	}
}

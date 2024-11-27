using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreInteraction : MonoBehaviour, IInteractable
{
	[SerializeField] private Material _coreDeactivatedMaterial; // 코어 비활성화 시의 머티리얼
	[SerializeField] private Light _coreLight; // 코어 내부의 조명
	[SerializeField] private GameObject _coreUI; 

	void Start()
	{
	}

	public void Interactive()
	{
		// 조명을 비활성화하여 코어의 상태를 시각적으로 표시
		if (_coreLight != null)
		{
			_coreLight.enabled = false;
		}


		// 코어와 상호작용 시 UIManager를 통해 UI를 제어
		UIManager.Instance.TriggerCoreInteractionUi();
	}
}
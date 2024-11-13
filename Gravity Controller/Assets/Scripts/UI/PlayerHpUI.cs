using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHpUI : MonoBehaviour
{
	[SerializeField] private Slider _hpSlider; // HP ������
	public float currentHp;
	private float _targetHp;
	private float _maxHp = 100f;

	void Start()
	{
		currentHp = _maxHp;
		_targetHp = _maxHp;
		_hpSlider.maxValue = _maxHp;
		_hpSlider.value = currentHp;
	}

	public void UpdateHP(float newHp)
	{
		_targetHp = newHp;
		StartCoroutine(HPDecrease());
	}

	private IEnumerator HPDecrease()
	{
		while (currentHp > _targetHp)
		{
			currentHp = Mathf.Lerp(currentHp, _targetHp, 0.2f);
			_hpSlider.value = currentHp;

			// ��ǥġ�� ����� ��������� ���� ����
			if (Mathf.Abs(currentHp - _targetHp) < 0.1f)
			{
				currentHp = _targetHp;
				_hpSlider.value = currentHp;
				yield break;
			}

			yield return new WaitForSeconds(0.02f);
		}
	}

}
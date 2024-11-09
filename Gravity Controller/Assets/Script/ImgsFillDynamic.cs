using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImgsFillDynamic : MonoBehaviour
{
	private float _factor = 0F;
	private bool _isFilling = false;
	public Image ImgFacterTarget;
	public Text TxtValue;
	public float MultifyText = 100F;
	public string TailText = "%";

	public float Factor { get { return this._factor; } }

	void Start()
	{
		_factor = 0F;
		StartFilling();
	}

	void Update()
	{
		if (_isFilling)
		{
			_factor += Time.deltaTime * 0.1f;
			if (_factor >= 1F)
			{
				_factor = 1F;
				_isFilling = false;
			}
			UpdateGauge();
		}

		if (Input.GetKeyDown(KeyCode.R) && _factor >= 1F)
		{
			ResetGauge();
		}
	}

	void StartFilling()
	{
		_isFilling = true;
	}

	void ResetGauge()
	{
		_factor = 0F;
		StartFilling();
	}

	void UpdateGauge()
	{
		SetImageFillAmount(_factor);
		SetTextFactor();
		SetImageColor();
	}

	void SetTextFactor()
	{
		float textFactor = Mathf.Clamp01(_factor) * MultifyText;
		if (TxtValue != null)
			TxtValue.text = string.Format("{0}{1}", textFactor.ToString("0"), TailText);
	}

	void SetImageFillAmount(float facter)
	{
		ImgFacterTarget.fillAmount = facter;
	}

	void SetImageColor()
	{
		Color currentColor = Color.Lerp(Color.cyan, Color.magenta, _factor);
		ImgFacterTarget.color = currentColor;
	}

	public bool IsFull()
	{
		return _factor >= 1F;
	}
}

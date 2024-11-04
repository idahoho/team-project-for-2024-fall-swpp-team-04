using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImgsFillDynamic : MonoBehaviour
{
	private float factor = 0F;
	private bool isFilling = false;
	public Image ImgFacterTarget;
	public Text TxtValue;
	public float MultifyText = 100F;
	public string TailText = "%";

	public float Factor { get { return this.factor; } }

	void Start()
	{
		factor = 0F;
		StartFilling();
	}

	void Update()
	{
		if (isFilling)
		{
			factor += Time.deltaTime * 0.1f;
			if (factor >= 1F)
			{
				factor = 1F;
				isFilling = false;
			}
			UpdateGauge();
		}

		if (Input.GetKeyDown(KeyCode.R) && factor >= 1F)
		{
			ResetGauge();
		}
	}

	void StartFilling()
	{
		isFilling = true;
	}

	void ResetGauge()
	{
		factor = 0F;
		StartFilling();
	}

	void UpdateGauge()
	{
		SetImageFillAmount(factor);
		SetTextFactor();
		SetImageColor();
	}

	void SetTextFactor()
	{
		float textFactor = Mathf.Clamp01(factor) * MultifyText;
		if (TxtValue != null)
			TxtValue.text = string.Format("{0}{1}", textFactor.ToString("0"), TailText);
	}

	void SetImageFillAmount(float facter)
	{
		ImgFacterTarget.fillAmount = facter;
	}

	void SetImageColor()
	{
		Color currentColor = Color.Lerp(Color.cyan, Color.magenta, factor);
		ImgFacterTarget.color = currentColor;
	}

	public bool IsFull()
	{
		return factor >= 1F;
	}
}

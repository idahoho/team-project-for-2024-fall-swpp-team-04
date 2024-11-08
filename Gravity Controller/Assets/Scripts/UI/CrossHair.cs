using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
	private RectTransform _crossHair;

	private float _defaultSize = 100f;
	private float _fireSize = 300f;

	void Start()
	{
		_crossHair = GetComponent<RectTransform>();
		_crossHair.sizeDelta = new Vector2(_defaultSize, _defaultSize);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			SetCrosshairSize(_fireSize);
		}
		else if (Input.GetMouseButtonUp(0))
		{
			SetCrosshairSize(_defaultSize);
		}
	}

	private void SetCrosshairSize(float size)
	{
		_crossHair.sizeDelta = new Vector2(size, size);
	}
}

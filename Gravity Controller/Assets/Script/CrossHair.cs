using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
	private RectTransform _crossHair;

	private float defaultSize = 100f;
	private float fireSize = 300f;

	void Start()
	{
		_crossHair = GetComponent<RectTransform>();
		_crossHair.sizeDelta = new Vector2(defaultSize, defaultSize);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			SetCrosshairSize(fireSize);
		}
		else if (Input.GetMouseButtonUp(0))
		{
			SetCrosshairSize(defaultSize);
		}
	}

	private void SetCrosshairSize(float size)
	{
		_crossHair.sizeDelta = new Vector2(size, size);
	}
}

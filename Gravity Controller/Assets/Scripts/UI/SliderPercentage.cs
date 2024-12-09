using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderPercentage : MonoBehaviour
{
    public void UpdateText()
	{
		GetComponent<Text>().text = transform.parent.GetComponent<Slider>().value + "%";
	}
}

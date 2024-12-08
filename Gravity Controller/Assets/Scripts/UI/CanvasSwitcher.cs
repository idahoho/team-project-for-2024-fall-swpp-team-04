using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasSwitcher : MonoBehaviour
{
	public CanvasGroup currentCanvas; // 현재 활성화된 캔버스
	public CanvasGroup newCanvas; // 활성화할 새로운 캔버스
	public float fadeDuration = 0.5f; // 페이드 효과 지속 시간
	public GameObject[] panels; // 버튼과 연결된 패널들을 배열로 저장.

	[SerializeField] private GameObject[] _sliders;
	[SerializeField] private float[] _defaultValues;

	// 버튼 클릭 시 호출되는 함수
	public void ShowPanel(int index)
	{
		// 모든 패널 비활성화
		foreach (GameObject panel in panels)
		{
			panel.SetActive(false);
		}

		// 선택된 패널만 활성화
		if (index >= 0 && index < panels.Length)
		{
			panels[index].SetActive(true);
		}
	}

	// 캔버스를 스위치하는 첫 번째 버튼 함수
	public void SwitchToNewCanvas()
	{
		// Load settings
		LoadSettingsSave();
		StartCoroutine(FadeOutAndIn(currentCanvas, newCanvas));
	}

	// 캔버스를 스위치하는 두 번째 버튼 함수
	public void SwitchToCurrentCanvas()
	{
		// Save settings
		SaveSettingsSave();
		StartCoroutine(FadeOutAndIn(newCanvas, currentCanvas));
	}

	// 페이드 아웃과 페이드 인을 동시에 수행
	private IEnumerator FadeOutAndIn(CanvasGroup canvasToFadeOut, CanvasGroup canvasToFadeIn)
	{
		// 기존 캔버스를 페이드 아웃
		if (canvasToFadeOut != null)
		{
			yield return StartCoroutine(FadeOut(canvasToFadeOut));
			canvasToFadeOut.gameObject.SetActive(false); // 페이드 아웃 후 비활성화
		}

		// 새 캔버스를 활성화
		if (canvasToFadeIn != null)
		{
			canvasToFadeIn.gameObject.SetActive(true);
			yield return StartCoroutine(FadeIn(canvasToFadeIn));
		}
	}

	// 페이드 아웃 구현
	private IEnumerator FadeOut(CanvasGroup canvasGroup)
	{
		float elapsedTime = 0f;

		while (elapsedTime < fadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		canvasGroup.alpha = 0f; // 페이드 아웃 완료
	}

	// 페이드 인 구현
	private IEnumerator FadeIn(CanvasGroup canvasGroup)
	{
		float elapsedTime = 0f;

		while (elapsedTime < fadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		canvasGroup.alpha = 1f; // 페이드 인 완료
	}

	private void LoadSettingsSave()
	{
		string json;
		if (!FileManager.LoadFromFile("settings.dat", out json))
		{
			// No file; Set to default
			SetSliders(_defaultValues);
			return;
		}
		var settingsSave = SettingsSave.Restore(json);
		if(settingsSave == null)
		{
			// Invalid file; Set to default
			Debug.Log("Invalid file: " + "settings.dat");
			SetSliders(_defaultValues);
			return;
		}
		var values = new float[3];
		values[0] = settingsSave.backGroundVolume;
		values[1] = settingsSave.effectVolume;
		values[2] = settingsSave.sensitivity;
		SetSliders(values);
	}
	private void SaveSettingsSave()
	{
		var save = new SettingsSave();
		save.backGroundVolume = _sliders[0].GetComponent<Slider>().value;
		save.effectVolume = _sliders[1].GetComponent<Slider>().value;
		save.sensitivity = _sliders[2].GetComponent<Slider>().value;

		FileManager.WriteToFile("settings.dat", JsonUtility.ToJson(save));
	}

	private void SetSliders(float[] values)
	{
		for(int i=0; i< values.Length; i++)
		{
			if (values[i] < 0) values[i] = 0;
			if (values[i] > 100) values[i] = 100;
			_sliders[i].GetComponent<Slider>().value = values[i];
		}
	}
}

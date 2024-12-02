using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
	public CanvasGroup currentCanvas; // 현재 활성화된 캔버스
	public CanvasGroup newCanvas; // 활성화할 새로운 캔버스
	public float fadeDuration = 0.5f; // 페이드 효과 지속 시간
	public GameObject[] panels; // 버튼과 연결된 패널들을 배열로 저장.

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
		StartCoroutine(FadeOutAndIn(currentCanvas, newCanvas));
	}

	// 캔버스를 스위치하는 두 번째 버튼 함수
	public void SwitchToCurrentCanvas()
	{
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
}

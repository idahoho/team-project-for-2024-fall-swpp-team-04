using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoEndHandler : MonoBehaviour
{
	public VideoPlayer videoPlayer;
	public GameObject uiGroup;
	private CanvasGroup canvasGroup;
	public float fadeDuration = 1.0f;
	void Start()
	{
		canvasGroup = uiGroup.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = uiGroup.AddComponent<CanvasGroup>();
		}

		canvasGroup.alpha = 0f;
		uiGroup.SetActive(false);

		videoPlayer.loopPointReached += OnVideoEnd;
	}

	void OnVideoEnd(VideoPlayer vp)
	{
		uiGroup.SetActive(true);
		StartCoroutine(FadeInCanvasGroup(canvasGroup, fadeDuration));
	}

	private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
	{
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			cg.alpha = Mathf.Clamp01(elapsed / duration);
			yield return null;
		}

		cg.alpha = 1f; 
	}
}

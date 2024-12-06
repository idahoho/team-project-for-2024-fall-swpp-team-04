using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
	public VideoPlayer _videoPlayer;
	public BossStageController _bossStageController;
	[SerializeField] private CanvasGroup _videoCanvasGroup;
	[SerializeField] private float _fadeDuration = 1.5f;

	void Start()
	{
		_videoPlayer.loopPointReached += HandleVideoEnded;
		_videoPlayer.playOnAwake = false;
	}

	public void PlayVideo()
	{
		if (_videoPlayer != null)
		{
			StartCoroutine(FadeInAndPlay());
		}
	}

	private IEnumerator FadeInAndPlay()
	{
		yield return StartCoroutine(FadeCanvasGroup(_videoCanvasGroup, 0f, 1f, _fadeDuration));

		_videoPlayer.Play();
	}

	private void HandleVideoEnded(VideoPlayer vp)
	{
		StartCoroutine(FadeOutAndDeactivate());
	}

	private IEnumerator FadeOutAndDeactivate()
	{
		yield return StartCoroutine(FadeCanvasGroup(_videoCanvasGroup, 1f, 0f, _fadeDuration));

		if (gameObject.activeInHierarchy)
			gameObject.SetActive(false);

		BossCore core = FindObjectOfType<BossCore>();
		if (core != null)
			core.ShowTextAfterVideoEnded();
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
	{
		float elapsed = 0f;
		canvasGroup.alpha = startAlpha;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
			yield return null;
		}

		canvasGroup.alpha = endAlpha;
	}
}

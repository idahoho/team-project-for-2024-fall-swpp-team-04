using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
	public VideoPlayer _videoPlayer;
	public BossStageController _bossStageController;
	[SerializeField] private CanvasGroup _videoCanvasGroup;
	[SerializeField] private float _fadeDuration = 1.5f;
	[SerializeField] private GameObject _bossStage;

	void Start()
	{
		_videoPlayer.loopPointReached += HandleVideoEnded;
		_videoPlayer.playOnAwake = false;
	}

	// 스테이지 이동 완료 후 BossStageController에서 호출할 예정
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
		_bossStage.gameObject.SetActive(false);

		BossStageController stage = FindObjectOfType<BossStageController>();
		if (stage != null && stage._finalCanvas != null)
		{
			stage._finalCanvas.SetActive(true);
			CanvasGroup finalCanvasGroup = stage._finalCanvas.GetComponent<CanvasGroup>();
			StartCoroutine(FadeCanvasGroup(finalCanvasGroup, 0f, 1f, _fadeDuration));
		}
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

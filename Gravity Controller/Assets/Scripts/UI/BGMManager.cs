using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
	public static BGMManager Instance { get; private set; }

	[Header("Audio Settings")]
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private float _fadeDuration = 1.0f;

	[Header("BGM Clips")]
	[SerializeField] private AudioClip _lobbyBGM;
	[SerializeField] private AudioClip _stage1BGM;
	[SerializeField] private AudioClip _stage2BGM;
	[SerializeField] private AudioClip _stage3BGM;
	[SerializeField] private AudioClip _stage4BGM;
	[SerializeField] private AudioClip _bossBGM;

	private Coroutine _currentFadeCoroutine;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	/// stage 0 = lobby, 1~4 = stage1~4, 5 = boss
	public void SetStageBGM(int stage)
	{
		AudioClip clipToPlay = null;

		switch (stage)
		{
			case 0:
				clipToPlay = _lobbyBGM;
				break;
			case 1:
				clipToPlay = _stage1BGM;
				break;
			case 2:
				clipToPlay = _stage2BGM;
				break;
			case 3:
				clipToPlay = _stage3BGM;
				break;
			case 4:
				clipToPlay = _stage4BGM;
				break;
			case 5:
				clipToPlay = _bossBGM;
				break;
			default:
				clipToPlay = _lobbyBGM;
				break;
		}

		PlayBGM(clipToPlay);
	}

	private void PlayBGM(AudioClip clip)
	{
		if (_audioSource == null)
		{
			Debug.LogWarning("BGMManager: AudioSource is not assigned.");
			return;
		}

		if (clip == null)
		{
			Debug.LogWarning("BGMManager: Attempting to play null AudioClip.");
			return;
		}

		if (_currentFadeCoroutine != null)
		{
			StopCoroutine(_currentFadeCoroutine);
		}

		_currentFadeCoroutine = StartCoroutine(FadeBGM(clip));
	}

	private IEnumerator FadeBGM(AudioClip newClip)
	{
		float initialVolume = _audioSource.volume;

		// fade out
		float elapsed = 0f;
		while (elapsed < _fadeDuration)
		{
			float t = elapsed / _fadeDuration;
			_audioSource.volume = Mathf.Lerp(initialVolume, 0f, t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		_audioSource.volume = 0f;

		_audioSource.clip = newClip;
		_audioSource.loop = true;
		_audioSource.Play();

		// fade in
		elapsed = 0f;
		while (elapsed < _fadeDuration)
		{
			float t = elapsed / _fadeDuration;
			_audioSource.volume = Mathf.Lerp(0f, 1f, t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		_audioSource.volume = 1f;

		_currentFadeCoroutine = null;
	}
}

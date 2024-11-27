using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlitchUI : MonoBehaviour
{
	public Text _gameOverText;
	[SerializeField] private float _glitchDuration = 0.1f;
	[SerializeField] private float _glitchInterval = 0.5f;

	private bool _isGlitching = false;

	void Start()
	{
		StartGlitchEffect();
	}

	public void StartGlitchEffect()
	{
		if (!_isGlitching)
		{
			_isGlitching = true;
			StartCoroutine(GlitchRoutine());
		}
	}

	private IEnumerator GlitchRoutine()
	{
		while (_isGlitching)
		{
			Color originalColor = _gameOverText.color;
			Vector3 originalPosition = _gameOverText.rectTransform.localPosition;
			Vector3 originalScale = _gameOverText.rectTransform.localScale;

			for (float t = 0; t < _glitchDuration; t += Time.deltaTime)
			{
				_gameOverText.color = new Color(Random.value, Random.value, Random.value);
				_gameOverText.rectTransform.localPosition = originalPosition + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
				_gameOverText.rectTransform.localScale = originalScale * Random.Range(0.9f, 1.1f);
				//_gameOverText.rectTransform.localEulerAngles = new Vector3(0, 0, Random.Range(-15f, 15f));
				yield return null;
			}

			_gameOverText.color = originalColor;
			_gameOverText.rectTransform.localPosition = originalPosition;
			_gameOverText.rectTransform.localScale = originalScale;
			//_gameOverText.rectTransform.localEulerAngles = Vector3.zero;

			yield return new WaitForSeconds(_glitchInterval);
		}
	}

	public void StopGlitchEffect()
	{
		_isGlitching = false;
		StopCoroutine(GlitchRoutine());
	}
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
	public CanvasGroup _buttonsCanvasGroup;
	public float _fadeInDuration = 1.0f;
	public float _delayBeforeFade = 2.0f;

	void Start()
	{
		_buttonsCanvasGroup.alpha = 0f;
		_buttonsCanvasGroup.interactable = false;
		_buttonsCanvasGroup.blocksRaycasts = false;

		StartCoroutine(FadeInButtons());
	}

	private IEnumerator FadeInButtons()
	{
		yield return new WaitForSeconds(_delayBeforeFade);

		float elapsedTime = 0f;
		while (elapsedTime < _fadeInDuration)
		{
			elapsedTime += Time.deltaTime;
			_buttonsCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / _fadeInDuration);
			yield return null;
		}

		_buttonsCanvasGroup.interactable = true;
		_buttonsCanvasGroup.blocksRaycasts = true;
	}

	public void RestartGame()
	{
		SceneManager.LoadScene("FinalGameScene");
	}

	public void GoToMainMenu()
	{
		SceneManager.LoadScene("MainSettingUI");
	}
}

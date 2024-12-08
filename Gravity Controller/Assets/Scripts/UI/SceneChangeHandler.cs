using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeHandler : MonoBehaviour
{
	public Text start;
	public Button startButton;

	public void LoadGame(string scene)
	{
		StartCoroutine(LoadGameCoroutine(scene));
	}

	public void ContinueGame(string scene)
	{
		string json;
		if (!FileManager.LoadFromFile("save.dat", out json))
		{
			// TODO alert
			return;
		}

		StartCoroutine(ContinueGameCoroutine(scene));
	}

	private void InitGameSave()
	{
		// TODO Initial settings
	}

	private void InitSettingsSave()
	{
		// TODO Initial settings
		var settingsSave = CanvasSwitcher.Instance.settingsSave;
		var player = GameObject.Find("Player");
		player.GetComponent<PlayerMovement>().SetSensitivityMultiplier(settingsSave.sensitivity);
	}

	IEnumerator LoadGameCoroutine(string scene)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		var mainMenu = SceneManager.GetActiveScene();
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
		InitSettingsSave();
		SceneManager.UnloadSceneAsync(mainMenu);
	}

	IEnumerator ContinueGameCoroutine(string scene)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		var mainMenu = SceneManager.GetActiveScene();
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
		InitGameSave();
		InitSettingsSave();
		SceneManager.UnloadSceneAsync(mainMenu);
	}
}

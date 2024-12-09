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
		if (CanvasSwitcher.Instance.gameSave.HasProgressed())
		{
			// savefile exists
			// TODO alert
			return;
		}
		
		StartCoroutine(LoadGameCoroutine(scene));
	}

	public void ContinueGame(string scene)
	{
		if (!CanvasSwitcher.Instance.gameSave.HasProgressed())
		{
			// save is empty
			// TODO alert
			return;
		}

		StartCoroutine(ContinueGameCoroutine(scene));
	}

	private void InitGameSave()
	{
		// TODO Initial settings
		var gameSave = CanvasSwitcher.Instance.gameSave;
		var player = GameObject.Find("Player");
		GameObject summonPoint = null;
		if (gameSave.atLobby) { 
			summonPoint = GameObject.Find("Summon Point").transform.GetChild(0).gameObject;
		}
		else {
			summonPoint = GameObject.Find("Summon Point").transform.GetChild(1).GetChild(gameSave.stage - 1).gameObject;
		}
		player.transform.position = summonPoint.transform.position;
		player.transform.rotation = summonPoint.transform.rotation;

		// deactivate spawners
		// activate cores -- _coreLight.enabled, RestoreCore
		// light
		// unlock doors
		// unlock abilities
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

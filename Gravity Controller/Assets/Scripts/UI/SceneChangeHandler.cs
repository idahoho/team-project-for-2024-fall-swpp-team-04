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
		SceneManager.LoadScene(scene);
	}
}

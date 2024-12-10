using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autosave
{
	public static void SaveGameSave(bool atLobby, int stage)
	{
		var save = new GameSave();
		save.atLobby = atLobby;
		save.stage = stage;

		FileManager.WriteToFile("save.dat", JsonUtility.ToJson(save));
	}
}

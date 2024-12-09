using UnityEngine;

[SerializeField]
public class GameSave
{
	public bool atLobby;
	public int stage;
	public static GameSave Restore(string json)
	{
		// reverts JSON string
		try
		{
			var save = JsonUtility.FromJson<GameSave>(json);
			return save;
		}
		catch
		{
			return null;
		}
	}
	
	public bool HasProgressed()
	{
		return (atLobby == true) || (stage > 1);
	}
}
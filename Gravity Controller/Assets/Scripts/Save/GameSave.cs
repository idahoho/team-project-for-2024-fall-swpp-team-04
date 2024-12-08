using UnityEngine;

[SerializeField]
public class GameSave
{
	public int progress;
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
}
using UnityEngine;

[SerializeField]
public class SettingsSave
{
	public int backGroundVolume;
	public int effectVolume;
	public int sensitivity;

	public static SettingsSave Restore(string json)
	{
		// reverts JSON string
		try
		{
			var save = JsonUtility.FromJson<SettingsSave>(json);
			return save;
		}
		catch
		{
			return null;
		} 
	}
}
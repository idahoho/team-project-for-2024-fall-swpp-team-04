using UnityEngine;

[SerializeField]
public class SettingsSave
{
	public float backGroundVolume;
	public float effectVolume;
	public float sensitivity;

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
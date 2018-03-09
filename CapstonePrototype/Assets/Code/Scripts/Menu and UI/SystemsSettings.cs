using UnityEngine;

public class SystemsSettings : MonoBehaviour {

	private void Awake()
	{
		NotificationCenter.Default.AddObserver("SaveWindowed", SaveWindowed);
		NotificationCenter.Default.AddObserver("GetWindowed", LoadWindowed);

		NotificationCenter.Default.AddObserver("SaveInverted", SaveInverted);
		NotificationCenter.Default.AddObserver("GetInverted", LoadInverted);

		NotificationCenter.Default.AddObserver("SaveResolution", SaveResolution);
		NotificationCenter.Default.AddObserver("GetResolution", LoadResolution);

		NotificationCenter.Default.AddObserver("SaveSensitvity", SaveSensitivity);
		NotificationCenter.Default.AddObserver("GetSensitivity", LoadSensitivity);

		NotificationCenter.Default.AddObserver("SaveVolumeMaster", SaveVolumeMaster);
		NotificationCenter.Default.AddObserver("GetVolumeMaster", LoadVolumeMaster);

		NotificationCenter.Default.AddObserver("SaveVolumeGame", SaveVolumeGame);
		NotificationCenter.Default.AddObserver("GetVolumeGame", LoadVolumeGame);

		NotificationCenter.Default.AddObserver("SaveVolumeMusic", SaveVolumeMusic);
		NotificationCenter.Default.AddObserver("GetVolumeMusic", LoadVolumeMusic);
	}

	void SaveWindowed(object value)
	{
		PlayerPrefs.SetInt("Windowed",(bool)value ? 1 : 0);
		PlayerPrefs.Save();
	}

	void LoadWindowed(object value)
	{
		bool ourSetting = PlayerPrefs.GetInt("Value", 1) == 1;
		NotificationCenter.Default.PostNotification("PassWindowed", ourSetting);
	}

	void SaveInverted(object value)
	{
		PlayerPrefs.SetInt("Inverted",(bool)value ? 1 : 0);
		PlayerPrefs.Save();
	}

	void LoadInverted(object value)
	{
		bool ourSetting = PlayerPrefs.GetInt("Inverted", 1) == 1;
		NotificationCenter.Default.PostNotification("PassInverted", ourSetting);
	}

	void SaveResolution(object value)
	{
		PlayerPrefs.SetInt("Resolution", (int)value);
		PlayerPrefs.Save();

	}

	void LoadResolution(object value)
	{
		int ourSetting = PlayerPrefs.GetInt("Resolution");
		NotificationCenter.Default.PostNotification("PassResolution", ourSetting);
	}

	void SaveSensitivity(object value)
	{
		PlayerPrefs.SetFloat("Sensitivity", (float)value);
		PlayerPrefs.Save();
	}

	void LoadSensitivity(object value)
	{
		float ourSetting = PlayerPrefs.GetFloat("Sensitivity");
		NotificationCenter.Default.PostNotification("PassSensitivity", ourSetting);
	}

	void SaveVolumeMaster(object value)
	{
		PlayerPrefs.SetFloat("VolumeMaster", (float)value);
		PlayerPrefs.Save();

	}

	void LoadVolumeMaster(object value)
	{
		float ourSetting = PlayerPrefs.GetFloat("VolumeMaster");
		NotificationCenter.Default.PostNotification("PassVolumeMaster", ourSetting);
	}

	void SaveVolumeGame(object value)
	{
		PlayerPrefs.SetFloat("VolumeGame", (float)value);
		PlayerPrefs.Save();

	}

	void LoadVolumeGame(object value)
	{
		float ourSetting = PlayerPrefs.GetFloat("VolumeGame");
		NotificationCenter.Default.PostNotification ("PassVolumeGame", ourSetting);
	}

	void SaveVolumeMusic(object value)
	{
		PlayerPrefs.SetFloat("VolumeMusic", (float)value);
		PlayerPrefs.Save();
	}

	void LoadVolumeMusic(object value)
	{
		float ourSetting = PlayerPrefs.GetFloat("VolumeMusic");
		NotificationCenter.Default.PostNotification("PassVolumeMusic", ourSetting);
	}
}

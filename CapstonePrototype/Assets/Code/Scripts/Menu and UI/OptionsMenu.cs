using UnityEngine;
using UnityEngine.UI;

public delegate void SaveValueEvent(string name, float value);

public class OptionsMenu : MonoBehaviour {

	public GameObject mainMenu;
	public Toggle isWindowed;
	public Toggle isInverted;
	public Dropdown resolution;
	public Slider mouseSensitivity;
	public Slider volumeMaster;
	public Slider volumeGame;
	public Slider volumeMusic;

	public void BackPressed()
	{
		ReturnToMainMenu();
	}

	public void ConfirmPressed()
	{
		NotificationCenter.Default.PostNotification("SaveWindowed", isWindowed.isOn);
		NotificationCenter.Default.PostNotification("SaveInverted", isInverted.isOn);
		NotificationCenter.Default.PostNotification("SaveResolution", resolution.value);
		NotificationCenter.Default.PostNotification("SaveSensitvity", mouseSensitivity.value);
		NotificationCenter.Default.PostNotification("SaveVolumeMaster", volumeMaster.value);
		NotificationCenter.Default.PostNotification("SaveVolumeGame", volumeGame.value);
		NotificationCenter.Default.PostNotification("SaveVolumeMusic", volumeMusic.value);
		ReturnToMainMenu();
	}

	void ReturnToMainMenu()
	{
		mainMenu.SetActive(true);
		gameObject.SetActive(false);
	}

	void OnEnable()
	{
		NotificationCenter.Default.AddObserver("PassWindowed", LoadWindowed);
		NotificationCenter.Default.PostNotification("GetWindowed");

		NotificationCenter.Default.AddObserver("PassInverted", LoadInverted);
		NotificationCenter.Default.PostNotification("GetInverted");

		NotificationCenter.Default.AddObserver("PassResolution", LoadResolution);
		NotificationCenter.Default.PostNotification("GetResolution");

		NotificationCenter.Default.AddObserver("PassSensitivity", LoadMouseSensitvity);
		NotificationCenter.Default.PostNotification("GetSensitivity");

		NotificationCenter.Default.AddObserver("PassVolumeMaster", LoadVolumeMaster);
		NotificationCenter.Default.PostNotification("GetVolumeMaster");

		NotificationCenter.Default.AddObserver("PassVolumeGame", LoadVolumeGame);
		NotificationCenter.Default.PostNotification("GetVolumeGame");

		NotificationCenter.Default.AddObserver("PassVolumeMusic", LoadVolumeMusic);
		NotificationCenter.Default.PostNotification("GetVolumeMusic");
	}

	void OnDisable()
	{
		NotificationCenter.Default.RemoveObserver("PassWindowed", LoadWindowed);
		NotificationCenter.Default.RemoveObserver("PassInverted", LoadInverted);
		NotificationCenter.Default.RemoveObserver("PassResolution", LoadResolution);
		NotificationCenter.Default.RemoveObserver("PassSensitivity", LoadMouseSensitvity);
		NotificationCenter.Default.RemoveObserver("PassVolumeMaster", LoadVolumeMaster);
		NotificationCenter.Default.RemoveObserver("PassVolumeGame", LoadVolumeGame);
		NotificationCenter.Default.RemoveObserver("PassVolumeMusic", LoadVolumeMusic);
	}

	void LoadWindowed(object setting)
	{
		isWindowed.isOn = (bool)setting;
	}

	void LoadInverted(object setting)
	{
		isInverted.isOn = (bool)setting;
	}

	void LoadResolution(object setting)
	{
		resolution.value = (int)setting;
	}

	void LoadMouseSensitvity(object setting)
	{
		mouseSensitivity.value = (float)setting;
	}

	void LoadVolumeMaster(object setting)
	{
		volumeMaster.value = (float)setting;
	}

	void LoadVolumeGame(object setting)
	{
		volumeGame.value = (float)setting;
	}

	void LoadVolumeMusic(object setting)
	{
		volumeMusic.value = (float)setting;
	}
}

using UnityEngine;

public class MainMenu : MonoBehaviour {

	public GameObject newGameMenu;
	public GameObject optionsMenu;
	public GameObject creditsMenu;

	public void NewGamePressed()
	{
		SwapToMenu(newGameMenu);
	}

	public void OptionPressed()
	{
		SwapToMenu(optionsMenu);
	}

	public void CreditsPressed()
	{
		SwapToMenu(creditsMenu);
	}

	void SwapToMenu(GameObject otherMenu)
	{
		otherMenu.SetActive(true);
		gameObject.SetActive(false);
	}

	public void ExitPressed()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}

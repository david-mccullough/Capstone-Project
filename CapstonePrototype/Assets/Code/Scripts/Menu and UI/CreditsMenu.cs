using UnityEngine;

public class CreditsMenu : MonoBehaviour {

	public GameObject mainMenu;

	public void BackPressed()
	{
		mainMenu.SetActive(true);
		gameObject.SetActive(false);
	}

	//TODO
}

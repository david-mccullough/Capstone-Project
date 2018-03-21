using UnityEngine;
using UnityEngine.UI;

public class NewGameMenu : MonoBehaviour {

	public GameObject mainMenu;
    //public SlotPanel slotPanels = { null, null, null, null };

    private void Awake() {
        
    }

    public void BackPressed()
	{
		mainMenu.SetActive(true);
		gameObject.SetActive(false);
	}

}

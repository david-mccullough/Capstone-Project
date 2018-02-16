using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableTile : MonoBehaviour {

	public int tileX;
	public int tileY;
	public TileMap map;
    public GameObject valueText;

	void OnMouseUp() {
		Debug.Log ("Click!");

		if(EventSystem.current.IsPointerOverGameObject())
			return;

		map.GeneratePathTo(tileX, tileY);
	}

    public void SetValueText(int value) {
        valueText.GetComponent<Text>().text = "" + value;
    }

}

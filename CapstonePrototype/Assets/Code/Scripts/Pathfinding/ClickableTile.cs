using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableTile : MonoBehaviour {

    public Map.Coord pos;
	public Map map;
    public GameObject valueText;
    

    private int value = 2;
    private bool isWalkable = true;
    private bool isHighlighted = false;

	void OnMouseUp() {
		Debug.Log ("Click!");

		if(EventSystem.current.IsPointerOverGameObject())
			return;

		map.GeneratePathTo(pos);
        map.breadthFirst(map.graph[pos.x, pos.y], 1);
	}


    public void SetValue(int value) {
        this.value = value;
        //valueText.GetComponent<Text>().text = "" + value;
    }

    public int GetValue() {
        return value;
    }

    public bool IsWalkable() {
        return isWalkable;
    }

    public void Highlight(bool boolean) {

        if (boolean != isHighlighted) {
            Renderer rend = this.gameObject.GetComponent<Renderer>();
            if (boolean) {
                //highlight
                rend.enabled = false;
            }
            else {
                //un-highlight
                rend.enabled = true;
            }
        }
        else {
            return;
        }        
    }
}

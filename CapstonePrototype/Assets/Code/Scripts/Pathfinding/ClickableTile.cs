using UnityEngine;
using UnityEngine.UI;

public class ClickableTile : MonoBehaviour {

    public Map.Coord pos;
	public Map map;
    public GameObject valueText;
    
    [SerializeField]
    private int value = 2;
    private bool isWalkable = true;
    private bool isAvailable = false;
    private bool isHighlighted = false;


    //TODO this has been moved controller using mousecast (still need to also make it impossible to generate path to tiles out of range)
	/*void OnMouseUp() {
		Debug.Log ("Click!");

		if(EventSystem.current.IsPointerOverGameObject())
			return;

		map.GeneratePathTo(pos);
	}*/


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

    public bool IsAvailable() {
        return isAvailable;
    }

    public void SetAvailability(bool boolean) {

        if (!boolean) {
            isAvailable = false;
            Highlight(false);
        }
        else if (!isAvailable) {
            //make available if walkable!
            if (isWalkable) {
                isAvailable = true;
                Highlight(true);
            }
        }
    }

    public void Highlight(bool boolean) {

        if (boolean != isHighlighted) {
            Renderer rend = this.gameObject.GetComponent<Renderer>();
            if (boolean) {
                //highlight
                rend.material.SetColor("_Color", new Color(1f, .34f, .1f));
                isHighlighted = true;
            }
            else {
                //un-highlight
                rend.material.SetColor("_Color", new Color(1f, 1f, 1f));
                isHighlighted = false;
            }
        }
        else {
            return;
        }        
    }
}

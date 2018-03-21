using System.Collections;
using UnityEngine;

public class ClickableTile : MonoBehaviour {

    public Map.Coord pos;
	public Map map;
    public TextMesh valueText;
    
    [SerializeField]
    private int value = 2;
    private bool isOccupied = false;
    private bool isWalkable = true;
    private bool isAvailable = false;
    private bool isHighlighted = false;
    private Faction owner = new Faction("NULL");

    [SerializeField]
    private GameObject top;
    private Renderer[] renderers = new Renderer[2];

    void Start() {
        renderers[0] = GetComponent<Renderer>();
        renderers[1] = top.GetComponent<Renderer>();
    }

    public bool CheckForCapture(Faction faction) {
        
        if (GetValue() % 10 == 0) {
            int radius = (GetValue() / 10) - 1;
            Map.Coord[] coords = map.GetCellsInCircle(this.pos, radius, true).ToArray();
            foreach (Map.Coord c in coords) {
                ClickableTile tempTile = map.tiles[c.x, c.y];
                tempTile.SetOwner(faction);
                tempTile.SetValue(1);
            }
            return true;
        }
        else {
            return false;
        }
    }

    public void SetValueText(int value) {
        valueText.text = "" + value;
    }

    public void SetValue(int value) {
        this.value = value;
        SetValueText(value);
    }

    public void AddToValue(int value) {
        SetValue(this.value + value);
    }

    public int GetValue() {
        return value;
    }

    public Faction GetOwner() {
        return owner;
    }

    public bool IsWalkable() {
        return isWalkable;
    }

    public bool IsAvailable() {
        return isAvailable;
    }

    public bool IsOccupied() {
        return isOccupied;
    }

    public void SetOwner(Faction faction) {
        owner = faction;
        StartCoroutine(FadeToColor(faction.color, .03f));
    }

    public void SetAvailability(bool boolean) {
        if (!boolean) {
            isAvailable = false;
            Highlight(false);
        }
        else if (!isAvailable) {
            // Check if there other reasons we shoudnt be made available
            if (isWalkable & !isOccupied) {
                isAvailable = true;
                Highlight(true);
            }
        }
    }

    public void SetWalkability(bool boolean) {
        isWalkable = boolean;
    }

    public void SetOccupationStatus(bool boolean) {
        isOccupied = boolean;
    }

    public void Highlight(bool boolean) {

        if (boolean != isHighlighted) {
            Renderer rend = top.gameObject.GetComponent<Renderer>();
            if (boolean) {
                //highlight
                //rend.material.shader.
                rend.material.SetFloat("_Outline", 0.1f);
                //rend.material.SetColor("_Color", new Color(1f, .34f, .1f));
                isHighlighted = true;
            }
            else {
                //un-highlight
                //rend.material.SetColor("_Color", new Color(1f, 1f, 1f));
                rend.material.SetFloat("_Outline", 0.0f);
                isHighlighted = false;
            }
        }
        else {
            return;
        }        
    }

    IEnumerator FadeToColor(Color c, float time) {
        for (var f = 0f; f < 1f; f += 0.1f) {

            foreach (Renderer r in renderers) {
                r.material.color = Color.Lerp(r.material.color, c, f);
            }
            yield return new WaitForSeconds(time);
        }
    }
}



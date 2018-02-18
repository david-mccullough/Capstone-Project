using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController instance = null;
    public Faction[] factions;

    [SerializeField]
    private Text uiText;
    [SerializeField]
    private Text uiUnitText;

    private int turnIndex = 0;

    private MapGenerator mapGen;
    private Map map;

    private Faction currentFaction;
    private Unit selectedUnit;


    void Awake() {

        // Enforce singleton pattern for GameController
        // Check if instance already exists
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        mapGen = GetComponent<MapGenerator>();
        InitGame();
    }

    void InitGame() {
        map = mapGen.GenerateMap();
        currentFaction = factions[turnIndex];
        uiText.text = currentFaction.name + " turn (" + turnIndex + ").";

    }

    void Update() {

        CheckForSelection();
        if (selectedUnit != null)
        uiUnitText.text = selectedUnit.GetMoveSpeed() + "," + selectedUnit.GetRemainingMoves();
    }

    public Faction NextTurn() {
        selectedUnit.ForceFinishTurn();

        turnIndex++;
        int maxIndex = factions.Length;
        turnIndex = turnIndex % maxIndex;

        Debug.Log("TURN INDEX: " + turnIndex);

        currentFaction = factions[turnIndex];
        uiText.text = currentFaction.name + " turn (" + turnIndex + ").";
        return currentFaction;
    }

    void CheckForSelection() {
        if (Input.GetButtonDown("Fire1")) {
            RaycastHit hit;
            if (MouseCast(out hit)) {
                //If hit unit, select it, else deselects current unit
                if (hit.transform.tag == "Tile") {
                    ClickableTile tile = hit.transform.GetComponent<ClickableTile>();
                    map.GeneratePathTo(tile.pos);
                }
                else if (hit.transform.tag == "Unit") {
                    SelectUnit(hit.transform.GetComponent<Unit>());
                }
            }
            else {
                DeselectUnit();
            }
        }
    }

    public Unit GetSelectedUnit() {
        return selectedUnit;
    }

    #region Helper Functions

    bool MouseCast(out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, out hit, 9999f);
    }

    void SelectUnit(Unit u) {
        if (u != null) {
            if (u.faction != currentFaction) {
                Debug.Log("Attempted to select unit that belongs to another faction");
            }
            else {
                selectedUnit = u;
                map.HighlightTiles(u.myCoords);
            }
        }
    }

    void DeselectUnit() {
        selectedUnit = null;
    }

    #endregion


}

[System.Serializable]
public struct Faction {
    public string name;
    public Color color;
    private List<Unit> myUnits;
    private int minUnits;
    private int maxUnits;
    

    public Faction(string name, Color color) {
        this.name = name;
        this.color = color;
        this.minUnits = 1;
        this.maxUnits = 1;
        myUnits = new List<Unit>();
    }

    public Faction(string name, Color color, int maxUnits, int minUnits) {
        this.name = name;
        this.color = color;
        this.minUnits = 1;
        this.maxUnits = 1;
        myUnits = new List<Unit>();
    }

    public void AddUnit(Unit unit) {
        myUnits.Add(unit);
    }

    public Unit[] GetUnits() {
        return myUnits.ToArray();
    }

    public static bool operator ==(Faction f1, Faction f2) {
        return f1.Equals(f2);
    }

    public static bool operator !=(Faction f1, Faction f2) {
        return !f1.Equals(f2);
    }

}

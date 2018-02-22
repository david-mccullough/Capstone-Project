using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController instance = null;
    public Faction[] factions;
    public Unit[] allUnits;
    private Faction currentFaction;
    private Unit selectedUnit;

    [SerializeField]
    private Text uiText;
    [SerializeField]
    private Text uiUnitText;
    private GameUI ui;

    private int turnIndex = 0;

    private MapGenerator mapGen;
    private Map map;


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
                
        InitGame();

        // Get references for event listening
        allUnits = FindObjectsOfType<Unit>();
        // Subscribe to relevant events
        foreach (Unit u in allUnits) {
            u.turnCompleteEvent += OnUnitTurnComplete;
        }

        ui.InitUI();
    }

    void InitGame() {

        mapGen = GetComponent<MapGenerator>();
        map = mapGen.GenerateMap();
        currentFaction = factions[turnIndex];
        uiText.text = currentFaction.name + " turn (" + turnIndex + ").";

        ui = FindObjectOfType<GameUI>();
    }

    void Update() {

        CheckForSelection();
        if (selectedUnit != null)
        uiUnitText.text = selectedUnit.GetMoveSpeed() + "," + selectedUnit.GetRemainingMoves();
    }

    // Advances game to next faction's turn, returns new current faction
    public Faction NextFaction() {

        // Ensure we cycle back to start of factions
        turnIndex++;
        int maxIndex = factions.Length;
        turnIndex = turnIndex % maxIndex;

        currentFaction = factions[turnIndex];
        uiText.text = currentFaction.name + " turn (" + turnIndex + ").";
        return currentFaction;
    }

    private void OnUnitTurnComplete() {
        // TODO a unit has completed its turn
        // we must either yield control the next faction's turn
        // or update the current unit (actaully just deselect it for expediency sake)
        if (true) {
            DeselectUnit();
            NextFaction();
        }
    }

    #region Accessors and Mutators

    public Unit GetSelectedUnit() {
        return selectedUnit;
    }

    #endregion

    #region Helper Functions

    bool MouseCast(out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, out hit, 9999f);
    }

    void CheckForSelection() {
        if (Input.GetButtonDown("Fire1")) {
            RaycastHit hit;
            if (MouseCast(out hit)) {
                //If hit unit, select it, else deselects current unit
                if (hit.transform.tag == "Tile") {
                    ClickableTile tile = hit.transform.GetComponent<ClickableTile>();
                    if (tile.IsAvailable()) {
                        map.GeneratePathTo(tile.pos);
                    }
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

    void SelectUnit(Unit u) {
        if (u != null) {
            if (u.faction != currentFaction) {
                Debug.Log("Attempted to select unit that belongs to another faction");
            }
            // Select the unit
            else {
                // hold refernece for current unit
                selectedUnit = u;
                // Get the frontier of available move options...
                Map.Coord[] coordOptions = u.GetAvailableTileOptions(map.graph[u.myCoords.x,u.myCoords.y], u.GetRemainingMoves()).ToArray();
                // ...and make those tiles walkable
                map.MakeTilesAvailable(coordOptions);
            }
        }
    }

    void DeselectUnit() {
        map.ResetTileAvailability();
        selectedUnit = null;
    }

    #endregion


}

//TODO turn factions into a scriptable object
#region Structs

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

#endregion
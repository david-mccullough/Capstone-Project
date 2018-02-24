using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static Faction[] factions;
    public List<Faction> activeFactions;
    private Queue<Faction> factionQueue = new Queue<Faction>();
    public Unit[] allUnits; //TODO could we just access units in our faction array?
    private Faction currentFaction;
    private Unit selectedUnit;
    private int numActiveFactions = 0;

    [SerializeField]
    private Text uiText;
    [SerializeField]
    private Text uiUnitText;
    private GameUI ui;

    private int turnIndex = 0;
    private bool gameOver = false;

    private MapGenerator mapGen;
    private Map map;

    // Create event actions
    public event Action<Faction> nextTurnEvent;
    public event Action<Faction> winEvent;

    void Start() {
                
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
        System.Random prng = new System.Random();
        map = mapGen.GenerateMap(activeFactions.ToArray(), prng.Next(100,200));
        currentFaction = activeFactions[turnIndex];
        uiText.text = currentFaction.name + " turn (" + turnIndex + ").";

        ui = FindObjectOfType<GameUI>();

        /*foreach (Faction f in activeFactions) {
            f.FactionDeactivateEvent += OnFactionDeactivate;
            numActiveFactions++;
            if (f!=null)
            factionQueue.Enqueue(f);
        }*/
        numActiveFactions = 2;
        activeFactions[0].FactionDeactivateEvent += OnFactionDeactivate;
        activeFactions[1].FactionDeactivateEvent += OnFactionDeactivate;
    }

    public void RestartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void EndGame() {
        Application.Quit();
    }

    void Update() {

        CheckForSelection();
        if (selectedUnit != null)
        uiUnitText.text = selectedUnit.GetMoveSpeed() + "," + selectedUnit.GetRemainingMoves();
    }

    // Advances game to next faction's turn, returns new current faction
    public Faction NextFaction() {
        /*Faction newFaction;
        if (numActiveFactions > 2) {

            //requeu curernt faction
            if (currentFaction.IsActive()) {
                factionQueue.Enqueue(factionQueue.Dequeue());
            }
            else {
                factionQueue.Dequeue();
            }
            
            
            //next faction active, return it
            if (factionQueue.Peek().IsActive()) {
                newFaction = factionQueue.Dequeue();
            }
            //next faction inactive, dequeue until we find one that's not
            else {
                while (!factionQueue.Peek().IsActive()) {
                    factionQueue.Dequeue();
                    //dont bother requing deactivated factions
                }
                newFaction = factionQueue.Dequeue();
            }
           
        }
        else {
            return currentFaction;
        }

        */
        // Ensure we cycle back to start of factions
        turnIndex++;
        int maxIndex = numActiveFactions;
        if (maxIndex < 2) {
            return currentFaction;
        }
        else {
            turnIndex = turnIndex % maxIndex;

            while (!activeFactions[turnIndex].IsActive()) {
                turnIndex++;
            }
            currentFaction = activeFactions[turnIndex];
            uiText.text = currentFaction.name + " turn (" + turnIndex + ").";
        }

        // Announce we have changed turns
        //currentFaction = newFaction;
        nextTurnEvent(currentFaction);
        Debug.Log("FACTION" + currentFaction.name);
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

    void OnFactionDeactivate() {
        numActiveFactions--;
        NextFaction();
        //Check for end game
        if (numActiveFactions == 1) {
            //Game over : last faction remains
            turnIndex = turnIndex++ % 2; //TODO remove this hardcoded foolery
            currentFaction = activeFactions[turnIndex];
            Debug.Log(currentFaction.name + " wins!");
            winEvent(currentFaction);
            gameOver = true;
        }
        else {
            NextFaction();
        }
    }

    void CheckForSelection() {
        if (Input.GetButtonDown("Fire1")) {
            RaycastHit hit;
            if (MouseCast(out hit)) {
                //If hit unit, select it, else deselects current unit
                if (hit.transform.tag == "Tile") {
                    ClickableTile tile = hit.transform.GetComponent<ClickableTile>();
                    if (tile.IsAvailable()) {
                        selectedUnit.SetPath(map.GeneratePathTo(selectedUnit.pos, tile.pos, selectedUnit));
                    }
                }
                else if (hit.transform.tag == "Unit" && gameOver != true) {
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
                Map.Coord[] coordOptions = u.GetAvailableTileOptions(map.graph[u.pos.x, u.pos.y], u.GetRemainingMoves()).ToArray();
                // ...and make those tiles walkable
                if (coordOptions.Length <= 0) {
                    selectedUnit.Elimnate();
                }
                else {
                    map.MakeTilesAvailable(coordOptions);
                }
            }
        }
    }


    #region Accessors and Mutators

    public Unit GetSelectedUnit() {
        return selectedUnit;
    }

    public Faction GetCurrentFaction() {
        return currentFaction;
    }

    #endregion

    #region Helper Functions

    bool MouseCast(out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, out hit, 9999f);
    }

    void DeselectUnit() {
        map.ResetTileAvailability();
        selectedUnit = null;
    }

    #endregion


}

//TODO turn factions into a scriptable object
#region Structs



#endregion
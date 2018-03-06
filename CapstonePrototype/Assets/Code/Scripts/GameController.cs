﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum ControllerState {
    idle = 0,
    drawing = 1,
}

public class GameController : MonoBehaviour {

    ControllerState state = ControllerState.idle;

    public static Faction[] factions;
    public List<Faction> activeFactions;
    private Queue<Faction> factionQueue = new Queue<Faction>();
    public Unit[] allUnits; //TODO could we just access units in our faction array?
    private Faction currentFaction;
    private Unit selectedUnit;
    private int numActiveFactions = 0;
    private Stack<Node> drawPathStack;

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

        //create path queue object
        drawPathStack = new Stack<Node>();
    }

    public void RestartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void EndGame() {
        Application.Quit();
    }

    void Update() {

        switch (state) {
            case ControllerState.idle:
                CheckForSelection();
            break;

            case ControllerState.drawing:
                DrawPath();
            break;
        }
        

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

    void DrawPath() {
        if (Input.GetButtonUp("Fire1")) {
            //We have let go, return to idle state
            state = ControllerState.idle;

            //All moves used?
             if (drawPathStack.Count-1 == selectedUnit.GetMoveSpeed()) {
                //Is this a valid path that leads to a destination tile?
                Node finalNode = drawPathStack.Peek();
                if (map.tiles[finalNode.pos.x, finalNode.pos.y].GetComponent<ClickableTile>().IsAvailable()) {
                    //Feed our unit this path
                    Debug.Log("sending drawn path to unit");
                    List<Node> path = new List<Node>(drawPathStack);
                    path.Reverse();
                    selectedUnit.SetPath(path);
                }
            }
            ClearDrawPath();
            return;
        }

        Node currentNode = drawPathStack.Peek();
        RaycastHit hit;
        //Check if we are hovering over tile
        if (MouseCast(out hit)) {
            if (hit.transform.tag == "Tile") {
                ClickableTile tempTile = hit.transform.GetComponent<ClickableTile>();
                Node tempNode = map.graph[tempTile.pos.x, tempTile.pos.y];

                //Is the tile an walkable neighbor tile?
                if (NodeIsNeighbour(currentNode, tempNode) && map.UnitCanEnterTile(tempNode.pos, selectedUnit)) {
                    //Is it the previous path node?
                    if (drawPathStack.Contains(tempNode)) {
                        //pop current position
                        Node popped = drawPathStack.Pop();
                        ClickableTile tile = map.tiles[popped.pos.x, popped.pos.y].GetComponent<ClickableTile>();
                        Debug.Log(tile.GetValue());
                        if (!tile.IsAvailable()) {
                            tile.Highlight(false);
                        }
                        tile.SetValueText(tile.GetValue());
                    }
                    //If not, we can push this neighbor is we have remianing moves, one step closer to destination
                    else if (drawPathStack.Count - 1 < selectedUnit.GetMoveSpeed()) {
                        {
                            drawPathStack.Push(tempNode);
                            //highlight tempTile? show its part of path
                            tempTile.Highlight(true);
                            tempTile.SetValueText(tempTile.GetValue() + drawPathStack.Count-1);
                        }
                    }
                }
            } //end MouseCast
        }
    }

    void ClearDrawPath() {
        while (drawPathStack.Count > 0) {
            Node popped = drawPathStack.Pop();
            ClickableTile tile = map.tiles[popped.pos.x, popped.pos.y].GetComponent<ClickableTile>();
            if (!tile.IsAvailable()) {
                tile.Highlight(false);
            }
            tile.SetValueText(tile.GetValue());
        }
    }

    void CheckForSelection() {
        if (Input.GetButtonDown("Fire1")) {
            RaycastHit hit;
            if (MouseCast(out hit)) {

                //If hit unit, select it, else deselects current unit
                if (hit.transform.tag == "Tile") {
                    ClickableTile tile = hit.transform.GetComponent<ClickableTile>();
                    //Tile is available, generate path to it and give the path to our unit
                    if (tile.IsAvailable()) {
                        //selectedUnit.SetPath(map.GeneratePathTo(selectedUnit.pos, tile.pos, selectedUnit));
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

    bool NodeIsNeighbour(Node source, Node neighbour) {
        if (source.neighbours.Contains(neighbour)) {
            return true;
        }
        return false;
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
                    // Kill unit if no desitnation options!
                    selectedUnit.Elimnate();
                }
                else {
                    map.MakeTilesAvailable(coordOptions);
                }

                //queue unit's position to drawPath
                drawPathStack.Clear();
                drawPathStack.Push(map.graph[u.pos.x,u.pos.y]);
                //map.tiles[u.pos.x, u.pos.y].GetComponent<ClickableTile>().Highlight(true);
                state = ControllerState.drawing;
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

    public int GetState() {
        return (int)state;
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
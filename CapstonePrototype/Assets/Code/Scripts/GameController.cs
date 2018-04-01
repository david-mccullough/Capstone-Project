using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum ControllerState {
    idle = 0,
    drawing = 1,
    ai = 2
}

public class GameController : MonoBehaviour {

    ControllerState state = ControllerState.idle;

    public AIController.AISkill aiSkill = AIController.AISkill.medium;
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
    private bool isThinking = false;

    private MapGenerator mapGen;
    private Map map;

    // Create event actions
    public event Action<Faction> nextTurnEvent;
    public event Action<Faction> winEvent;
    public event Action pauseEvent;

    public AudioSource audioSource;
    [SerializeField]
    private AudioClip sndClickUp;
    [SerializeField]
    private AudioClip sndClickDown;

    void Start() {
        InitGame();

        // Get references for event listening
        allUnits = FindObjectsOfType<Unit>();
        // Subscribe to relevant events
        foreach (Unit u in allUnits) {
            u.turnCompleteEvent += OnUnitTurnComplete;
        }

        if (ui != null) {
            ui.InitUI();
        }

        
    }

    void InitGame() {

        mapGen = GameObject.Find("MapManager").GetComponent<MapGenerator>();
        System.Random prng = new System.Random();
        map = mapGen.GenerateMap(activeFactions.ToArray(), prng.Next(100,200));
        currentFaction = activeFactions[turnIndex];
        uiText.text = currentFaction.name + " turn (" + turnIndex + ").";

        ui = FindObjectOfType<GameUI>();
        AIController.Default.SetMap(map);
        

        /*foreach (Faction f in activeFactions) {
            f.FactionDeactivateEvent += OnFactionDeactivate;
            numActiveFactions++;
            if (f!=null)
            factionQueue.Enqueue(f);
        }*/
        numActiveFactions = 2;
        activeFactions[0].FactionDeactivateEvent += OnFactionDeactivate;
        activeFactions[1].FactionDeactivateEvent += OnFactionDeactivate;

        activeFactions[1].SetAI(PlayerPrefs.GetInt("IS_AI", 1) == 1);

        //create path queue object
        drawPathStack = new Stack<Node>();

        //Load sounds
        /*AudioClip sndClickDown = (AudioClip)Resources.Load<AudioClip>("clickDown");
        AudioClip sndClickUp = (AudioClip)Resources.Load<AudioClip>("clickUp");*/
    }

    public void RestartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void EndGame() {
        //Application.Quit();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void PauseGame() {
        pauseEvent();
    }

    void Update() {

        switch (state) {
            case ControllerState.idle:
                CheckForSelection();
            break;

            case ControllerState.drawing:
                if (selectedUnit == null) {
                    state = ControllerState.idle;
                }
                else {
                    DrawPath();
                }
            break;

            case ControllerState.ai:
                if (!isThinking) {
                    state = ControllerState.idle;
                }
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
        Debug.Log("Next faction: " + currentFaction.name);

        if (currentFaction.IsAI()) {
            StartCoroutine(ProcessAI());
        }

        return currentFaction;
    }
    
    IEnumerator ProcessAI() {
        isThinking = true;
        state = ControllerState.ai;
        yield return new WaitForSeconds(.5f);
        Unit u = currentFaction.GetUnits()[0]; //For now, just select one and only unit
        SelectUnit(u);
        AIController.Default.SetUnit(u);
        
        yield return new WaitForSeconds(UnityEngine.Random.Range(.75f, 1.5f));

        var path = AIController.Default.MakeDecision(aiSkill);
        selectedUnit.SetPath(path);
        isThinking = false;
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
                if (map.tiles[finalNode.pos.x, finalNode.pos.y].IsAvailable()) {
                    //Feed our unit this path
                    Debug.Log("sending drawn path to unit");
                    List<Node> path = new List<Node>(drawPathStack);
                    path.Reverse();
                    selectedUnit.SetPath(path);
                }
            }
            else {
                map.ResetTileDrawPath();
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
                Node tempNode = map.nodes[tempTile.pos.x, tempTile.pos.y];

                //Is the tile a walkable neighbor tile?
                if (NodeIsNeighbour(currentNode, tempNode) && map.UnitCanEnterTile(tempNode.pos, selectedUnit)) {
                    //Is it the previous path node?
                    if (drawPathStack.Contains(tempNode)) {
                        //pop current position
                        Node popped = drawPathStack.Pop();
                        ClickableTile tile = map.tiles[popped.pos.x, popped.pos.y].GetComponent<ClickableTile>();
                        Debug.Log(tile.GetValue());
                        tile.DrawHighlight(false, .01f);
                        tile.SetValueText(tile.GetValue());

                        audioSource.volume = .4f;
                        audioSource.pitch = 1.9f;
                        audioSource.clip = sndClickUp;
                        audioSource.Play();                        

                        //adjust available tiles
                        var coordOptions = selectedUnit.GetAvailableTileOptions(tempNode, selectedUnit.GetMoveSpeed() - drawPathStack.Count+ 1);
                        map.ResetTileAvailability();
                        List<Map.Coord> coordList = new List<Map.Coord>();
                        for (int i = 0; i < selectedUnit.coordOptions.Length; i++) {
                            if (coordOptions.Contains(selectedUnit.coordOptions[i])) {
                                coordList.Add(selectedUnit.coordOptions[i]);
                            }
                        }
                        map.MakeTilesAvailable(coordList.ToArray());
                    }
                    //If not, we can push this neighbor is we have remianing moves, one step closer to destination
                    else if (drawPathStack.Count - 1 < selectedUnit.GetMoveSpeed()) {
                        {
                            //adjust available tiles
                            var coordOptions = selectedUnit.GetAvailableTileOptions(tempNode,  selectedUnit.GetMoveSpeed() - drawPathStack.Count);
                            map.ResetTileAvailability();
                            List<Map.Coord> coordList = new List<Map.Coord>();
                            for (int i = 0; i < selectedUnit.coordOptions.Length; i++) {
                                if (coordOptions.Contains(selectedUnit.coordOptions[i])) {
                                    coordList.Add(selectedUnit.coordOptions[i]);
                                }
                            }
                            map.MakeTilesAvailable(coordList.ToArray());

                            drawPathStack.Push(tempNode);
                            //highlight tempTile? show its part of path
                            tempTile.DrawHighlight(true, .02f);
                            tempTile.SetValueText(tempTile.GetValue() + drawPathStack.Count-1);

                            audioSource.volume = .95f;
                            audioSource.pitch = (((float)drawPathStack.Count - 1f) / (float)selectedUnit.GetMoveSpeed())*.1f + .9f;
                            audioSource.clip = sndClickDown;
                            audioSource.Play();
                        }
                    }
                }
            } //end MouseCast
        }
    }

    void ClearDrawPath() {
        while (drawPathStack.Count > 0) {
            Node popped = drawPathStack.Pop();
            ClickableTile tile = map.tiles[popped.pos.x, popped.pos.y];
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
                u.UpdateMoveSpeed();
                selectedUnit.coordOptions = u.GetAvailableTileOptions(map.nodes[u.pos.x, u.pos.y], u.GetRemainingMoves()).ToArray();

                // ...and make those tiles walkable
                if (u.coordOptions.Length <= 0) {
                    // Kill unit if no desitnation options!
                    selectedUnit.Elimnate();
                }
                else {
                    map.MakeTilesAvailable(u.coordOptions);
                }

                if (state != ControllerState.ai) {
                    //queue unit's position to drawPath
                    drawPathStack.Clear();
                    drawPathStack.Push(map.nodes[u.pos.x, u.pos.y]);
                    //map.tiles[u.pos.x, u.pos.y].Highlight(true);
                    state = ControllerState.drawing;
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
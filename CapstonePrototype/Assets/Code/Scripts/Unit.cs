using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    // pos represent the correct map-tile position
    // for this unit. Note that this doesn't necessarily mean
    // the world-space coordinates, because our map might be scaled
    // and during movement animations, we are going to be
    // somewhere in between tiles.
    public Map.Coord pos;
    public Map map;
    public Faction faction;

    // Our pathfinding info.  Null if we have no destination ordered.
    public List<Node> currentPath = null;

    // How far this unit can move in one turn
    [SerializeField]
    int moveSpeed = 2;
    int remainingMovement = 2;
    bool isReadyToAdvance = true;
    Map.Coord turnStartPos;
    float Y_POS = 1.25f;

    private Renderer rend;

    // Declare Delegates
    public delegate void PathCompleteDelegate();
    public delegate void TurnCompleteDelegate();

    // Create event delegate instances
    public event PathCompleteDelegate pathCompleteEvent;
    public event TurnCompleteDelegate turnCompleteEvent;

    void Start() {

        turnStartPos = pos;
        SetMoveSpeed(map.GetTileValue(pos));
        remainingMovement = moveSpeed;
    }

    public void Init(Map map, Map.Coord pos, Faction faction) {
        this.map = map;
        this.pos = pos;
        this.faction = faction;

        faction.AddUnit(this);

        pos.x = (int)transform.position.x;
        pos.y = (int)transform.position.z;

        // Set our color to our faction color
        rend = gameObject.GetComponent<Renderer>();
        //Material tempMaterial = new Material(rend.sharedMaterial);
        //tempMaterial.color = faction.color;
        rend.material.color = faction.color;

        Debug.Log("inited");

        map.tiles[pos.x, pos.y].GetComponent<ClickableTile>().SetOccupationStatus(true);
    }

    void Update() {

        // Draw our debug line showing the pathfinding!
        // NOTE: This won't appear in the actual game view.
        if (currentPath != null) {
            int currNode = 0;

            while (currNode < currentPath.Count - 1) {

                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].pos, Y_POS) +
                    new Vector3(0, 0, -0.5f);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].pos, Y_POS) +
                    new Vector3(0, 0, -0.5f);

                Debug.DrawLine(start, end, Color.red);

                currNode++;
            }

            // Have we moved our visible piece close enough to the target tile that we can
            // advance to the next step in our pathfinding?
            isReadyToAdvance = (Vector3.Distance(transform.position, map.TileCoordToWorldCoord(currentPath[0].pos, Y_POS)) < 0.1f);
            if (isReadyToAdvance) {
                AdvancePathing();
            }

        }

        // Smoothly animate towards the correct map tile.
        transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord(pos, Y_POS), 5f * Time.deltaTime);
    }

    public List<Map.Coord> GetAvailableTileOptions(Node pos, int distance) {
        // This is pretty inefficent... doing BFS twice and subtracting one from the other
        // to get the outer ring of available node coords
        HashSet<Map.Coord> outer = new HashSet<Map.Coord>(map.BreadthFirst(pos, distance, this));
        if (outer.Count < 1) {
            return new List<Map.Coord>();
        }
        List<Map.Coord> inner = new List<Map.Coord>(map.BreadthFirst(pos, distance - 1, this));

        var C = outer.Subtract(inner);


        //step through inner nodes, remove them from outer node list
        /*for (int i = 0; i < inner.Count; i++) {

            if (outer.Contains(inner.)) {
                Debug.Log("Found match: " + inner[i]);
                outer.Remove(inner[i]);
            }
        }*/

        // TODO this is bad...
        List<Map.Coord> tilesToRemove = new List<Map.Coord>();
        foreach (Map.Coord c in C) {
            if (!map.UnitCanEnterTile(c, this)) {
                tilesToRemove.Add(c);
            }
        }

        foreach (Map.Coord c in tilesToRemove) {
            if (C.Contains(c)) {
                C.Remove(c);
            }
        }

        Debug.Log("returning available options");
        return new List<Map.Coord>(C);
    }

    public void SetPath(List<Node> newPath) {
        map.tiles[pos.x, pos.y].GetComponent<ClickableTile>().SetOccupationStatus(false);

        map.ResetTileAvailability();
        currentPath = newPath;    

    }

    // Advances our pathfinding progress by one tile.
    void AdvancePathing() {

        if (remainingMovement <= 0) {
            return;
        }

        if (currentPath == null || currentPath.Count <= 1) {
            return;
        }

        // Teleport us to our correct "current" position, in case we
        // haven't finished the animation yet.
        transform.position = map.TileCoordToWorldCoord(pos, Y_POS);

        //Debug.Log(remainingMovement + " on tile " + currentPath[0].pos + ". Moving to " + currentPath[1].pos);

        // Move us to the next tile in the sequence
        pos.x = currentPath[1].pos.x;
        pos.y = currentPath[1].pos.y;

        // Add our remaining movement to current tile
        ClickableTile currentTile = map.tiles[pos.x, pos.y].GetComponent<ClickableTile>();
        currentTile.AddToValue(moveSpeed - remainingMovement + 1);
        if (currentTile.CheckForCapture(faction)) {
            // This is the last tile, so set out movespeed to one
            if (currentTile.pos == currentPath[currentPath.Count - 1].pos) {
                SetMoveSpeed(1);
            }
        }

        // Subtract cost from current tile to next tile
        remainingMovement -= 1;

        // Remove the old "current" tile from the pathfinding list
        currentPath.RemoveAt(0);

        if (currentPath.Count == 1) {
            // We only have one tile left in the path, and that tile MUST be our ultimate
            // destination -- and we are standing on it!

            // Unit arrived at destination. Sending out event message!
            if (pathCompleteEvent != null) {
                pathCompleteEvent();
            }

            // We are at our destinationa and we are out of moves, so end our turn
            if (remainingMovement <= 0) {
                Debug.Log("Unit out of path and moves! Ending turn.");
                FinishTurn();
            }
        }
    }

    public void FinishTurn() {

        // clear our pathfinding info
        currentPath = null;

        // Reset our available movement points.
        SetMoveSpeed(map.GetTileValue(pos));
        remainingMovement = moveSpeed;
        turnStartPos = pos;

        //Let our resting tile know we have occupied it
        var currentTile = map.tiles[pos.x, pos.y].GetComponent<ClickableTile>();
        currentTile.SetOccupationStatus(true);

        // Send out event message that our turn is complete
        if (turnCompleteEvent != null) {
            turnCompleteEvent();
        }
    }

    // TODO maybe remove? ForcePathCompletion() never gets called?
    public void ForcePathCompletion() {
        Debug.Log("Forced finish.");
        // Make sure to wrap-up any outstanding movement left over.
        if (remainingMovement > 0)
            return;

        while (currentPath != null && remainingMovement > 0) {
            Debug.Log("Force advance path");
            AdvancePathing();
        }
    }

    public void Elimnate() {
        Debug.Log("Eliminated unit from " + faction.name);
        StartCoroutine(FadeOut(0.1f));
        faction.RemoveUnit(this); //this should trigger faction to report that its lost on the following turn

        var currentTile = map.tiles[pos.x, pos.y].GetComponent<ClickableTile>();
        currentTile.SetOccupationStatus(false);
        rend.enabled = false;
    }

    public int GetMoveSpeed() {
        return moveSpeed;
    }

    public void SetMoveSpeed(int value) {
        moveSpeed = value;
        //uiText.GetComponent<Text>().text = "" + value;
    }

    public int GetRemainingMoves() {
        return remainingMovement;
    }

    IEnumerator FadeOut(float time) {
        
        for (float f = 1f; f >= 0; f -= 0.1f) {
            Color c = rend.material.color;
            c.a = f;
            rend.material.SetColor("_Emission", c);
            //rend.material.color = c;
            yield return new WaitForSeconds(time);
        }
    }
}

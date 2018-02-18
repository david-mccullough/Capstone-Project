﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    // myCoords represent the correct map-tile position
    // for this unit. Note that this doesn't necessarily mean
    // the world-space coordinates, because our map might be scaled
    // and during movement animations, we are going to be
    // somewhere in between tiles.
    public Map.Coord myCoords;
	public Map map;
    public Faction faction;

    // Our pathfinding info.  Null if we have no destination ordered.
    public List<Node> currentPath = null;

	// How far this unit can move in one turn. Note that some tiles cost extra.
    [SerializeField]
	int moveSpeed = 2;
    int remainingMovement=2;
    bool isReadyToAdvance = true;
    Map.Coord turnStartPos;

    public GameObject uiText;

    // Constructor TODO needed?
    /*public Unit (Map m, Faction fac) {
        map = m;
        faction = fac;
    }*/

    void Awake() {


        turnStartPos = myCoords;
    }

	void Update() {

        // Draw our debug line showing the pathfinding!
        // NOTE: This won't appear in the actual game view.
        if (currentPath != null) {
			int currNode = 0;

			while( currNode < currentPath.Count-1 ) {

				Vector3 start = map.TileCoordToWorldCoord( currentPath[currNode].pos ) + 
					new Vector3(0, 0, -0.5f) ;
				Vector3 end   = map.TileCoordToWorldCoord( currentPath[currNode+1].pos )  + 
					new Vector3(0, 0, -0.5f) ;

				Debug.DrawLine(start, end, Color.red);

				currNode++;
			}

            // Have we moved our visible piece close enough to the target tile that we can
            // advance to the next step in our pathfinding?
            isReadyToAdvance = (Vector3.Distance(transform.position, map.TileCoordToWorldCoord(currentPath[0].pos)) < 0.1f);
            if (isReadyToAdvance) {
                AdvancePathing();
            }

        }

        // Smoothly animate towards the correct map tile.
        transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord(myCoords), 5f * Time.deltaTime);
	}

    // Advances our pathfinding progress by one tile.
    void AdvancePathing() {

        if (remainingMovement <= 0) {
            return;
        }

        if (currentPath == null) {
            return;
        }


        // Teleport us to our correct "current" position, in case we
        // haven't finished the animation yet.
        transform.position = map.TileCoordToWorldCoord(myCoords);

        // Get cost from current tile to next tile
        remainingMovement -= 1;//map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y );
        foreach (Node n in currentPath) {
            Debug.Log(remainingMovement + " on tile " + currentPath[0].pos + ". Path is: " + n.pos);
        }
        if (uiText != null) {
            uiText.GetComponent<Text>().text = "" + remainingMovement;
        }


        // Move us to the next tile in the sequence
        myCoords.x = currentPath[1].pos.x;
        myCoords.y = currentPath[1].pos.y;
		
		// Remove the old "current" tile from the pathfinding list
		currentPath.RemoveAt(0);

		
		if(currentPath.Count == 1) {
            // We only have one tile left in the path, and that tile MUST be our ultimate
            // destination -- and we are standing on it!
            // So just reset
            if (remainingMovement <= 0) {
                Debug.Log("No more path.");
                FinishTurn();
            }
		}
	}

    public void FinishTurn() {

        // clear our pathfinding info
        currentPath = null;

        // Reset our available movement points.
        SetMoveSpeed(map.GetTileValue(myCoords));
        remainingMovement = moveSpeed;
        turnStartPos = myCoords;
        GameController.instance.NextTurn();
    }

    public void ForceFinishTurn() {
        // Make sure to wrap-up any outstanding movement left over.
        if (remainingMovement > 0)
            return;

        while (currentPath != null && remainingMovement > 0) {
            Debug.Log("Force advance path");
            AdvancePathing();
        }
    }

    public int GetMoveSpeed() {
        return moveSpeed;
    }

    public void SetMoveSpeed(int value) {
        Debug.Log("moveSpeed = " + value + "\nremaining = " + value);
        moveSpeed = value;
        //uiText.GetComponent<Text>().text = "" + value;
    }

    public int GetRemainingMoves() {
        return remainingMovement;
    }

}

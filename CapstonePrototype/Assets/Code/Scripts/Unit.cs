using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    // myCoords represent the correct map-tile position
    // for this piece.  Note that this doesn't necessarily mean
    // the world-space coordinates, because our map might be scaled
    // or offset or something of that nature.  Also, during movement
    // animations, we are going to be somewhere in between tiles.
    public Map.Coord myCoords;

	public Map map;

	// Our pathfinding info.  Null if we have no destination ordered.
	public List<Node> currentPath = null;

	// How far this unit can move in one turn. Note that some tiles cost extra.
    [SerializeField]
	int moveSpeed = 2;
    [SerializeField]
    int remainingMovement=2;

    public GameObject uiText;

    // Constructor
    public Unit (Map m) {
        map = m;

    }


	void Update() {
		// Draw our debug line showing the pathfinding!
		// NOTE: This won't appear in the actual game view.
		if(currentPath != null) {
			int currNode = 0;

			while( currNode < currentPath.Count-1 ) {

				Vector3 start = map.TileCoordToWorldCoord( currentPath[currNode].pos ) + 
					new Vector3(0, 0, -0.5f) ;
				Vector3 end   = map.TileCoordToWorldCoord( currentPath[currNode+1].pos )  + 
					new Vector3(0, 0, -0.5f) ;

				Debug.DrawLine(start, end, Color.red);

				currNode++;
			}
		}

		// Have we moved our visible piece close enough to the target tile that we can
		// advance to the next step in our pathfinding?
		if(Vector3.Distance(transform.position, map.TileCoordToWorldCoord(myCoords)) < 0.1f)
			AdvancePathing();

        // Smoothly animate towards the correct map tile.
        transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord(myCoords), 5f * Time.deltaTime);
	}

    // Advances our pathfinding progress by one tile.
    void AdvancePathing() {
        if (currentPath == null) {
            if (remainingMovement == 0) {
            }
            return;
        }

        if (remainingMovement <= 0) {
            Reset();
            return;
        }

        // Teleport us to our correct "current" position, in case we
        // haven't finished the animation yet.
        transform.position = map.TileCoordToWorldCoord(myCoords);

        // Get cost from current tile to next tile
        remainingMovement -= 1;//map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y );
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
			// So let's just clear our pathfinding info.
			currentPath = null;
		}
	}

    /*
	// The "Next Turn" button calls this.
	public void NextTurn() {
        // Make sure to wrap-up any outstanding movement left over.
        if (remainingMovement > 0)
            return;

        while (currentPath!=null && remainingMovement > 0) {
			AdvancePathing();
		}

        // Reset our available movement points.
        SetMoveSpeed(map.GetTileValue(myCoords));
        remainingMovement = moveSpeed;
	}
    */

    public void SetMoveSpeed(int value) {
        Debug.Log("moveSpeed = " + value);
        moveSpeed = value;
        //uiText.GetComponent<Text>().text = "" + value;
    }

    public int GetRemainingMoves() {
        return remainingMovement;
    }

    //TODO better method name
    public void Reset() {
        // Reset our available movement points.
        SetMoveSpeed(map.GetTileValue(myCoords));
        remainingMovement = moveSpeed;
    }

    void OnMouseUp() {
        Debug.Log("Click Unit!");

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        map.SelectUnit(this);
        foreach (Map.Coord tile in map.breadthFirst(map.graph[myCoords.x, myCoords.y], map.selectedUnit.GetRemainingMoves())) {
            map.tiles[tile.x, tile.y].GetComponent<ClickableTile>().Highlight(true);
        }
    }
}

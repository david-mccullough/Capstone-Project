using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map {

    public Coord size;
    public int seed;
    public int maxTileValue;
    public Node[,] graph;
    public Transform[,] tiles; //tilePrefab with ClickableTile script

    public Unit selectedUnit;

    [Range(0, 1)]
    public float obstaclePercent;
    public float minObstacleHeight;
    public float maxObstacleHeight;
    public Color foregroundColor;
    public Color backgroundColor;

    #region Structs and classes
    [System.Serializable]
    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator !=(Coord c1, Coord c2) {
            return !(c1.x == c2.x && c1.y == c2.y);
        }

        public static Coord operator +(Coord c1, Coord c2) {
            return new Coord(c1.x + c2.x, c1.y + c2.y);
        }
        public static Coord operator -(Coord c1, Coord c2) {
            return new Coord(c1.x - c2.x, c1.y - c2.y);
        }
        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }
    }


    #endregion

    public Coord center {
        get {
            return new Coord((int)size.x / 2, (int)size.y / 2);
        }
    }

    public Vector3 TileCoordToWorldCoord(Coord c) {
        return new Vector3(c.x, 1.5f, c.y);
    }

    public int GetTileValue(Coord target) {

        ClickableTile ct = tiles[target.x, target.y].GetComponent<ClickableTile>();
        int value = ct.GetValue();
        Debug.Log("Tile value at " + target + " is " + value);
        return value;

    }

    public float CostToEnterTile(Coord source, Coord target) {

        //TileType tt = tileTypes[ tiles[targetX,targetY] ];

        if (UnitCanEnterTile(target) == false)
            return Mathf.Infinity;

        float cost = 1f;//tt.movementCost;

        if (source.x != target.x && source.y != target.y) {
            // We are moving diagonally!  Fudge the cost for tie-breaking
            // Purely a cosmetic thing!
            cost += 0.001f;
        }

        return cost;

    }

    public bool UnitCanEnterTile(Coord pos) {

        // We could test the unit's walk/hover/fly type against various
        // terrain flags here to see if they are allowed to enter the tile.

        return tiles[pos.x, pos.y].GetComponent<ClickableTile>().IsWalkable();
    }

    public void GeneratePathTo(Coord pos) {
        // Clear out our unit's old path.
        selectedUnit.currentPath = null;

        if (UnitCanEnterTile(pos) == false) {
            // We clicked on an unwalkable tile, so just quit.
            Debug.Log("Unable to create path to " + pos);
            return;
        }

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        // Setup the "Q" -- the list of nodes we haven't checked yet.
        List<Node> unvisited = new List<Node>();

        Node source = graph[
                            selectedUnit.myCoords.x,
                            selectedUnit.myCoords.y
                            ];

        Node target = graph[
                            pos.x,
                            pos.y
                            ];

        dist[source] = 0;
        prev[source] = null;

        // Initialize everything to have INFINITY distance, since
        // we don't know any better right now. Also, it's possible
        // that some nodes CAN'T be reached from the source,
        // which would make INFINITY a reasonable value
        foreach (Node v in graph) {
            if (v != source) {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0) {
            // "u" is going to be the unvisited node with the smallest distance.
            Node u = null;

            foreach (Node possibleU in unvisited) {
                if (u == null || dist[possibleU] < dist[u]) {
                    u = possibleU;
                }
            }

            if (u == target) {
                break;  // Exit the while loop!
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours) {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(u.pos, v.pos);
                if (alt < dist[v]) {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        // If we get there, then either we found the shortest route
        // to our target, or there is no route at ALL to our target.

        if (prev[target] == null) {
            // No route between our target and the source
            return;
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;

        // Step through the "prev" chain and add it to our path
        while (curr != null) {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        // Right now, currentPath describes a route from our target to our source
        // So we need to invert it!

        currentPath.Reverse();

        selectedUnit.currentPath = currentPath;
    }

    // Flood cells from specific coord and return an array of those flooded coords
    public List<Coord> breadthFirst(Node parent, int maxDepth) {

        List<Coord> nodes = new List<Coord>();

        if (maxDepth < 0) {
            return nodes;
        }

        Queue<Node> nodeQueue = new Queue<Node>();
        nodeQueue.Enqueue(parent);

        int currentDepth = 0,
            elementsToDepthIncrease = 1,
            nextElementsToDepthIncrease = 0;

        while (nodeQueue.Count > 0) {
            Node current = nodeQueue.Dequeue();

            nodes.Add(current.pos);

            nextElementsToDepthIncrease += current.neighbours.Count;
            if (--elementsToDepthIncrease == 0) {
                if (++currentDepth > maxDepth) {
                    return nodes;
                }
                elementsToDepthIncrease = nextElementsToDepthIncrease;
                nextElementsToDepthIncrease = 0;
            }
            foreach (Node child in current.neighbours) {
                nodeQueue.Enqueue(child);
            }
        }

        return nodes;

    }

    public void SelectUnit(Unit u) {
        selectedUnit = u;
        Debug.Log("Selected new unit");

    }
}

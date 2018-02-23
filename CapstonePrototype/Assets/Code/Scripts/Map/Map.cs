using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Map {

    public Coord size;
    public int seed;
    public int maxTileValue;
    public Node[,] graph;
    public Transform[,] tiles; //tilePrefabs with ClickableTile script

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

    public Coord Center {
        get {
            return new Coord((int)size.x / 2, (int)size.y / 2);
        }
    }

    public Vector3 TileCoordToWorldCoord(Coord c, float y) {
        return new Vector3(c.x, y, c.y);
    }

    public int GetTileValue(Coord target) {

        ClickableTile ct = tiles[target.x, target.y].GetComponent<ClickableTile>();
        int value = ct.GetValue();
        //Debug.Log("Tile value at " + target + " is " + value);
        return value;

    }

    public float CostToEnterTile(Coord start, Coord target, Unit u) {

        //TileType tt = tileTypes[ tiles[targetX,targetY] ];

        if (UnitCanEnterTile(target, u) == false)
            return Mathf.Infinity;

        float cost = 1f;//tt.movementCost;

        if (start.x != target.x && start.y != target.y) {
            // We are moving diagonally!  Fudge the cost for tie-breaking
            // Purely a cosmetic thing!
            cost += 0.001f;
        }

        return cost;

    }

    public bool UnitCanEnterTile(Coord pos, Unit u) {

        // We could test the unit's walk/hover/fly type against various
        // terrain flags here to see if they are allowed to enter the tile.
        bool canEnter = true;
        var targetTile = tiles[pos.x, pos.y].GetComponent<ClickableTile>();
        string ownerName = targetTile.GetOwner().name;
        canEnter = (targetTile.IsWalkable()
                    && !targetTile.IsOccupied()
                    && (ownerName == u.faction.name || ownerName == "NULL"));

        return canEnter;
    }

    public List<Node> GeneratePathTo(Coord start, Coord pos, Unit unit) {
        List<Node> currentPath = new List<Node>(0);
        /*Unit unit = GameController.instance.GetSelectedUnit();
        if (unit == null) {
            Debug.Log("Attempted to generate path for null unit.");
            return;
        }

        // Clear out our unit's old path.
        unit.currentPath = null;
        */
        if (UnitCanEnterTile(pos, unit) == false) {
            // We clicked on an unwalkable tile, so just quit.
            Debug.Log("Unable to create path to " + pos);
            return currentPath;
        }

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        // Setup the "Q" -- the list of nodes we haven't checked yet.
        List<Node> unvisited = new List<Node>();

        Node source = graph[
                            start.x,
                            start.y
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
                float alt = dist[u] + CostToEnterTile(u.pos, v.pos, unit);
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
            return currentPath;
        }

        Node curr = target;

        // Step through the "prev" chain and add it to our path
        while (curr != null) {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        // Right now, currentPath describes a route from our target to our source
        // So we need to invert it!

        currentPath.Reverse();

        return currentPath;
    }

    // Flood cells from specific coord and return an array of those flooded coords
    public List<Coord> BreadthFirst(Node parent, int maxDepth) {

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

            foreach (Node child in current.neighbours) {
                if (tiles[current.pos.x, current.pos.y].GetComponent<ClickableTile>().IsWalkable()) {
                    nextElementsToDepthIncrease++;
                }
            }

            if (--elementsToDepthIncrease == 0) {
                if (++currentDepth > maxDepth) {
                    return nodes;
                }
                elementsToDepthIncrease = nextElementsToDepthIncrease;
                nextElementsToDepthIncrease = 0;
            }

            foreach (Node child in current.neighbours) {
                if (tiles[current.pos.x, current.pos.y].GetComponent<ClickableTile>().IsWalkable()) {
                    nodeQueue.Enqueue(child);
                }
            }
        }
        
        return nodes;

    }

    /*public List<Coord> GetCellsOfCircle(Node center, int radius, bool isFilled) {

        List<Coord> nodes = new List<Coord>();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                // Compute LHS of circle equation
                int LHS = (int)Mathf.Floor( (x - center.pos.x)*(x - center.pos.x) + (y - center.pos.y)*(y - center.pos.y) );

                // If filled include values less than radius squared
                if (isFilled) {
                    if (LHS <= radius*radius) {
                        nodes.Add(graph[x,y].pos);
                    }
                }
                else {
                    if (LHS == radius*radius) {
                        nodes.Add(graph[x, y].pos);
                    }
                }
            }
        }
        
        return nodes;
    }*/

    public bool CheckOnCircle(Coord node, Coord center, int radius) {
        // Compute LHS of circle equation
        int LHS = (int)Mathf.Ceil((node.x - center.x) * (node.x - center.x) + (node.y - center.y) * (node.y - center.y));

        if (LHS == radius * radius) {
            return true;
        }

        return false;
    }

    public List<Coord> GetCircleCells(Coord source, int radius, bool isFilled) {

        List<Coord> points = new List<Coord>();

        // If filled include values less than radius squared
        if (isFilled) {
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    // Compute LHS of circle equation
                    int LHS = (int)Mathf.Floor((x - source.x) * (x - source.x) + (y - source.y) * (y - source.y));

                    if (LHS <= radius * radius) {
                        points.Add(graph[x, y].pos);
                    }
                }
            }
        }
        //Raytrace a circle
        else {
            float N = Mathf.Round(SectorLength(radius, 2 * Mathf.PI));
            Debug.Log("N=" + N);
            for (int step = 0; step <= N; step++) {
                float t = step / N;
                Debug.Log("t="+ t);
                float angle = Mathf.Lerp(0f, 2f * Mathf.PI, t);
                Debug.Log("angle="+ angle);
                Vector2 point = new Vector2(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle));
                Debug.Log("" + point);

                Coord coord = RoundPoint(point)+source;
                Debug.Log("" + coord);
                if (coord.x < size.x && coord.y < size.y &&
                    coord.x > 0 && coord.y > 0) {
                    Debug.Log("Adding " + coord);
                    points.Add(coord);
                }
            }
        }

        return points;
    }

    Vector2 LerpPoint(Vector2 c1, Vector2 c2, float t) {
        return new Vector2(Mathf.Lerp(c1.x, c2.x, t),
                         Mathf.Lerp(c1.y, c2.y, t));
    }

    float LerpRadians(float a, float b, float lerpFactor) // Lerps from angle a to b (both between 0.f and PI_TIMES_TWO), taking the shortest path
{
        float result;
        float diff = b - a;
        if (diff < -Mathf.PI) {
            // lerp upwards past PI_TIMES_TWO
            b += Mathf.PI * 2;
            result = Mathf.Lerp(a, b, lerpFactor);
            if (result >= Mathf.PI * 2) {
                result -= Mathf.PI * 2;
            }
        }
        else if (diff > Mathf.PI) {
            // lerp downwards past 0
            b -= Mathf.PI * 2;
            result = Mathf.Lerp(a, b, lerpFactor);
            if (result < 0f) {
                result += Mathf.PI * 2;
            }
        }
        else {
            // straight lerp
            result = Mathf.Lerp(a, b, lerpFactor);
        }

        return result;
    }

    Coord RoundPoint(Vector2 c) {
        return new Coord((int)Mathf.Round(c.x), (int)Mathf.Round(c.y));
    }

    float SectorLength(float radius, float angle) {
        // angle must be in radian measure
        return radius * angle;
    }

    public void MakeTilesAvailable(Coord[] coords) {
        foreach (Coord c in coords) {
            tiles[c.x, c.y].GetComponent<ClickableTile>().SetAvailability(true);
        }
    }

    public void ResetTileAvailability() {
        foreach (Transform tile in tiles) {
            tile.GetComponent<ClickableTile>().SetAvailability(false);
        }
    }

    public int Distance(Coord c1, Coord c2) {
        return (int)System.Math.Sqrt((c1.x - c2.x)* (c1.x - c2.x) + (c1.y - c2.y)* (c1.y - c2.y));
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    #region Variables

    public Map[] maps;

    [Header("Global Map Settings")]

    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;

    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;

    Map currentMap;

    #endregion

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
    }

    [System.Serializable]
    public class Map {

        public Coord size;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord center {
            get {
                return new Coord((int)size.x / 2, (int)size.y / 2);
            }
        }

    }
    #endregion

    #region Main Methods

    void Start() {
        GenerateMap();
    }

    public void GenerateMap() {

        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed);

        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.size.x; x++) {
            for (int y = 0; y < currentMap.size.y; y++) {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // Create GenreratedMap GO to hold tiles
        string holderName = "GeneratedMap";
        if (transform.FindChild(holderName)) {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Create tiles
        for (int x = 0; x < currentMap.size.x; x++) {
            for (int y = 0; y <currentMap.size.y; y++) {
                // Step through loop to create new positions
                Vector3 tilePosition = CoordToPostiion(x, y);
                // Create new tile instance from prefab, rotate 90 degrees, and cast as Tranform type
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                //Scale newTile based on outlinePercent
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                // Set tile's parent to GenratedMap GO
                newTile.parent = mapHolder;
            }
        }

        bool[,] obstacleMap = new bool[(int)currentMap.size.x, (int)currentMap.size.y];

        // Determine number of obstacles
        int obstacleCount = (int)(currentMap.size.x * currentMap.size.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        // Create obstacles
        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.center && MapIsFloodable(obstacleMap, currentObstacleCount)) {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPostiion(randomCoord.x, randomCoord.y);
                // instantiate obstacle
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3(tileSize, obstacleHeight, tileSize);
                newObstacle.transform.position = new Vector3(newObstacle.transform.position.x, obstacleHeight / 2, newObstacle.transform.position.z);

                //adjust color
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material (obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.size.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                newObstacle.parent = mapHolder;
            }
            else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }           
    }

    #endregion

    #region Helper Methods

    // Returns a random coord from the list of coords
    public Coord GetRandomCoord() {
        // Pop coord from shuffled coords
        Coord randomCoord = shuffledTileCoords.Dequeue();
        // Return it to back of queue
        shuffledTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    // Converts coord into real world location
    Vector3 CoordToPostiion(int x, int y) {
        return new Vector3(-currentMap.size.x / 2 + 0.5f + x, 0f, -currentMap.size.y + 0.5f + y) * tileSize;
    }

    // Ensures map is fully accessible using flood fill algorithm
    bool MapIsFloodable(bool[,] obstacleMap, int currentObstacleCount) {

        bool[,] visitedNeighbors = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        //Basis: mapCenter is empty
        queue.Enqueue(currentMap.center);
        visitedNeighbors[currentMap.center.x, currentMap.center.y] = true;
        int accessibleTileCount = 1;

        Coord[] neighborOffsets = {
            new Coord(-1, 0), // Left
            new Coord(0, 1), // Top
            new Coord(1, 0), // Right
            new Coord(0, -1) // Bottom
        };

        // Recursive step: check neighbors of accessible tiles, if accessible check neighbor's neighbors.
        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();
            //loop through adjacent tiles to "tile"
            foreach (Coord neighborOffset in neighborOffsets) {
                Coord neighbor = tile + neighborOffset;
                // Check if we are in buonds of obstacle map
                if ((neighbor.x >= 0 && neighbor.x < currentMap.size.x) && (neighbor.y >= 0 && neighbor.y < currentMap.size.y)) {
                    // Check that neighbor has NOT been checked already and that neighbor is NOT an obstacle
                    if (visitedNeighbors[neighbor.x, neighbor.y] || obstacleMap[neighbor.x, neighbor.y]) {
                        continue;
                    }

                    visitedNeighbors[neighbor.x, neighbor.y] = true;
                    //Enqueue this neighbor
                    queue.Enqueue(neighbor);
                    accessibleTileCount++;
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.size.x * currentMap.size.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGenerator: MonoBehaviour {

    #region Variables

    public Map[] maps;

    [Header("Global Map Settings")]

    [SerializeField]
    public int mapIndex;

    public TileType[] tileTypes;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform unitPrefab;

    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize;

    List<Map.Coord> allTileCoords;
    Queue<Map.Coord> shuffledTileCoords;

    Map currentMap;

    #endregion

   
    #region Main Methods



    public Map GenerateMap() {

        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed);

        Debug.Log("Map dimensions: " + currentMap.size);

        currentMap.tiles = new Transform[currentMap.size.x, currentMap.size.y];

        allTileCoords = new List<Map.Coord>();
        for (int x = 0; x < currentMap.size.x; x++) {
            for (int y = 0; y < currentMap.size.y; y++) {
                allTileCoords.Add(new Map.Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Map.Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // Create GenreratedMap GO to hold tiles
        string holderName = "GeneratedMap";
        if (transform.FindChild(holderName)) {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Create tiles
        for (int x = 0; x < currentMap.size.x; x++) {
            for (int y = 0; y < currentMap.size.y; y++) {
                // Step through loop to create new positions
                Vector3 tilePosition = CoordToPostiion(x, y);
                // Create new tile instance from prefab, rotate 90 degrees, and cast as Tranform type
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform; // Quaternion.Euler(Vector3.right * 90))
                //Scale newTile based on outlinePercent
                newTile.localScale = new Vector3((1 - outlinePercent) * tileSize, 1f, (1 - outlinePercent) * tileSize);//Vector3.one * (1 - outlinePercent) * tileSize;
                // Set tile's parent to GenratedMap GO
                newTile.parent = mapHolder;

                //Debug.Log(x + "," + y);
                currentMap.tiles[x, y] = newTile;
                ClickableTile ct = newTile.GetComponent<ClickableTile>();
                //ct.valueText = ui;
                ct.pos = new Map.Coord(x, y);
                ct.SetValue((int)Random.Range(1,currentMap.maxTileValue+1));
                ct.map = currentMap;
            }
        }

        bool[,] obstacleMap = new bool[(int)currentMap.size.x, (int)currentMap.size.y];

        // Determine number of obstacles
        int obstacleCount = (int)(currentMap.size.x * currentMap.size.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        // Create obstacles
        for (int i = 0; i < obstacleCount; i++) {
            Map.Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.center && MapIsFloodable(obstacleMap, currentObstacleCount)) {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPostiion(randomCoord.x, randomCoord.y);
                // instantiate obstacle
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3(tileSize, obstacleHeight, tileSize);
                newObstacle.transform.position = new Vector3(newObstacle.transform.position.x, obstacleHeight / 2 + 0.5f, newObstacle.transform.position.z);
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

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

        // TODO make this more robust. currently just spawns 2 player units with respective factions
        // eventually this should step through and generate untis for all existing factions associated with match settings

        // Setup the player unit
        Unit newUnit = Instantiate(unitPrefab, currentMap.TileCoordToWorldCoord(new Map.Coord(0, 0)), Quaternion.identity).GetComponent<Unit>() as Unit;
        newUnit.GetComponent<Unit>().map = currentMap;
        newUnit.myCoords.x = (int)newUnit.transform.position.x;
        newUnit.myCoords.y = (int)newUnit.transform.position.z;
        newUnit.map = currentMap;
        newUnit.transform.parent = mapHolder;
        newUnit.faction = GameController.instance.factions[1];
        newUnit.GetComponent<Renderer>().material.color = newUnit.faction.color;

        // Setup the AI unit
        newUnit = Instantiate(unitPrefab, currentMap.TileCoordToWorldCoord(new Map.Coord(currentMap.size.x-1, currentMap.size.y - 1)), Quaternion.identity).GetComponent<Unit>() as Unit;
        newUnit.GetComponent<Unit>().map = currentMap;
        newUnit.myCoords.x = (int)newUnit.transform.position.x;
        newUnit.myCoords.y = (int)newUnit.transform.position.z;
        newUnit.map = currentMap;
        newUnit.transform.parent = mapHolder;
        newUnit.faction = GameController.instance.factions[0];
        newUnit.GetComponent<Renderer>().material.color = newUnit.faction.color;

        GeneratePathfindingGraph(currentMap);

        return currentMap;
    }

    void GeneratePathfindingGraph(Map map) {
        // Initialize the array
        map.graph = new Node[map.size.x, map.size.y];

        // Initialize a Node for each spot in the array
        for (int x = 0; x < map.size.x; x++) {
            for (int y = 0; y < map.size.y; y++) {
                map.graph[x, y] = new Node();
                map.graph[x, y].pos.x = x;
                map.graph[x, y].pos.y = y;
            }
        }

        // Now that all the nodes exist, calculate their neighbours
        for (int x = 0; x < map.size.x; x++) {
            for (int y = 0; y < map.size.y; y++) { //map.size.x here too?

                // This is the 4-way connection version:
                if (x > 0)
                    map.graph[x, y].neighbours.Add(map.graph[x - 1, y]);
                if (x < map.size.x - 1)
                    map.graph[x, y].neighbours.Add(map.graph[x + 1, y]);
                if (y > 0)
                    map.graph[x, y].neighbours.Add(map.graph[x, y - 1]);
                if (y < map.size.y - 1)
                    map.graph[x, y].neighbours.Add(map.graph[x, y + 1]);

                #region// This is the 8-way connection version (allows diagonal movement)
                // Try left
                /*              if(x > 0) {
                                    graph[x,y].neighbours.Add( graph[x-1, y] );
                                    if(y > 0)
                                        graph[x,y].neighbours.Add( graph[x-1, y-1] );
                                    if(y < mapSizeY-1)
                                        graph[x,y].neighbours.Add( graph[x-1, y+1] );
                                }

                                // Try Right
                                if(x < mapSizeX-1) {
                                    graph[x,y].neighbours.Add( graph[x+1, y] );
                                    if(y > 0)
                                        graph[x,y].neighbours.Add( graph[x+1, y-1] );
                                    if(y < mapSizeY-1)
                                        graph[x,y].neighbours.Add( graph[x+1, y+1] );
                                }

                                // Try straight up and down
                                if(y > 0)
                                    graph[x,y].neighbours.Add( graph[x, y-1] );
                                if(y < mapSizeY-1)
                                    graph[x,y].neighbours.Add( graph[x, y+1] );
                */

                // This also works with 6-way hexes and n-way variable areas (like EU4)
                #endregion
            }
        }
    }



    //void GenerateMapVisual(Map map) {
    //    for (int x = 0; x < map.size.x; x++) {
    //        for (int y = 0; y < map.size.y; y++) {
    //            //TileType tt = tileTypes[tiles[x, y]];
    //            //GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);
    //            //GameObject ui = Instantiate(tt.tileUIPrefab, new Vector3(x, y, -0.51f), Quaternion.identity, tileCanvas.transform);

    //            ClickableTile ct = go.GetComponent<ClickableTile>();
    //            ct.valueText = ui;
    //            ct.tileX = x;
    //            ct.tileY = y;
    //            ct.SetValueText(tiles[x, y]);
    //            ct.map = this;
    //        }
    //    }
    //}

    #endregion

    #region Helper Methods

    // Returns a random coord from the list of coords
    public Map.Coord GetRandomCoord() {
        // Pop coord from shuffled coords
        Map.Coord randomCoord = shuffledTileCoords.Dequeue();
        // Return it to back of queue
        shuffledTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    // Converts coord into real world location
    Vector3 CoordToPostiion(int x, int y) {
        return new Vector3(x, 0f, y) * tileSize;
    }

    // Ensures map is fully accessible using flood fill algorithm
    bool MapIsFloodable(bool[,] obstacleMap, int currentObstacleCount) {

        bool[,] visitedNeighbors = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Map.Coord> queue = new Queue<Map.Coord>();
        //Basis: mapCenter is empty
        queue.Enqueue(currentMap.center);
        visitedNeighbors[currentMap.center.x, currentMap.center.y] = true;
        int accessibleTileCount = 1;

        Map.Coord[] neighborOffsets = {
            new Map. Coord(-1, 0), // Left
            new  Map.Coord(0, 1), // Top
            new  Map.Coord(1, 0), // Right
            new  Map.Coord(0, -1) // Bottom
        };

        // Recursive step: check neighbors of accessible tiles, if accessible check neighbor's neighbors.
        while (queue.Count > 0) {
            Map.Coord tile = queue.Dequeue();
            //loop through adjacent tiles to "tile"
            foreach (Map.Coord neighborOffset in neighborOffsets) {
                Map.Coord neighbor = tile + neighborOffset;
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

using System.Collections.Generic;
using UnityEngine;

public class MapGenerator: MonoBehaviour {

    #region Variables

    public Map[] maps;

    [Header("Global Map Settings")]

    [SerializeField]
    private int mapIndex;

    public TileType[] tileTypes;

    // Prefabs
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform unitPrefab;

    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize;

    private List<Map.Coord> allTileCoords;
    private Queue<Map.Coord> shuffledTileCoords;

    private Map currentMap;

    #endregion

   
    #region Main Methods

    /*void Update() {
        currentMap.graph.UpdateGraph();
    }*/

    public Map GenerateMap(Faction[] factions, int seed) {

        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(seed);
        
        currentMap.tiles = new ClickableTile[currentMap.size.x, currentMap.size.y];
        
        allTileCoords = new List<Map.Coord>();
        for (int x = 0; x < currentMap.size.x; x++) {
            for (int y = 0; y < currentMap.size.y; y++) {
                allTileCoords.Add(new Map.Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Map.Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));        

        //Get corners (will be where we spawn units)
        List<Map.Coord> corners = new List<Map.Coord>();
        corners.Add(new Map.Coord(0, 0));
        corners.Add(new Map.Coord(currentMap.size.x-1, currentMap.size.y-1));
        corners.Add(new Map.Coord(currentMap.size.x-1, 0));
        corners.Add(new Map.Coord(0, currentMap.size.y-1));

        // Create GenreratedMap GO to hold tiles
        string holderName = "GeneratedMap";
        if (transform.FindChild(holderName)) {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        int k = 0;
        List<Transform> tileTransforms = new List<Transform>(currentMap.size.x * currentMap.size.y);
        // Create tiles
        for (int x = 0; x < currentMap.size.x; x++) {
            for (int y = 0; y < currentMap.size.y; y++) {
                // Step through loop to create new positions
                Vector3 tilePosition = CoordToPosition(new Map.Coord(x,y), 0f);
                // Create new tile instance from prefab, rotate 90 degrees, and cast as Tranform type
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform; // Quaternion.Euler(Vector3.right * 90))
                //Scale newTile based on outlinePercent
                newTile.localScale = new Vector3((1 - outlinePercent) * tileSize, 1f, (1 - outlinePercent) * tileSize);//Vector3.one * (1 - outlinePercent) * tileSize;
                // Set tile's parent to GenratedMap GO
                newTile.parent = mapHolder;
                tileTransforms.Add(newTile);

                ClickableTile ct = newTile.GetComponent<ClickableTile>();
                currentMap.tiles[x, y] = ct;
                currentMap.tilesOneD[k++] = ct;
                ct.pos = new Map.Coord(x, y);

                ct.SetValue((int)prng.Next(1,currentMap.maxTileValue));
                ct.map = currentMap;
            }
        }
        //currentMap.tilesOneD = Utility.TwoDToOneDArray(currentMap.tiles) as ClickableTile[];
        currentMap.graph = new Graph(tileTransforms.ToArray(), currentMap.size.x, GraphFunctionName.Ripple);

        #region Obstacle Generation
        bool[,] obstacleMap = new bool[(int)currentMap.size.x, (int)currentMap.size.y];
        
        // Determine number of obstacles
        int obstacleCount = (int)(currentMap.size.x * currentMap.size.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        // Create obstacles using PerlinNoise
        PerlinNoise perlin = new PerlinNoise(currentMap.size.x, currentMap.size.y, seed);
        Map.Coord[] obstacleCoords = GetPerlinCoords(perlin, currentMap.obstaclePercent);

        foreach (Map.Coord c in obstacleCoords) {
            obstacleMap[c.x, c.y] = true;
            currentObstacleCount++;
            // Check that this coord is valid
            if (c != currentMap.Center && MapIsFloodable(obstacleMap, currentObstacleCount) && !corners.Contains(c)) {
                //Create obstacle
                Transform newObstacle = CreateObstacle(c, perlin.GetValueAt(c.x, c.y));
                // Parent obstacle to tile sharing coord position
                newObstacle.parent = currentMap.tiles[c.x,c.y].transform;
            }
            else {
                obstacleMap[c.x, c.y] = false;
                currentObstacleCount--;
            }
                
        }
        #endregion

        // Create units for each active faction, initialize them, and set its parent to the mapHolder
        Map.Coord origin; //this will tell us which quadrant to spawn each faction's units
        // step trough our list of factions TODO change back to length of factions
        for (int i = 0; i < 2/*factions.Length*/; i++) {

            // determine quadrant
            origin = corners[i];

            //for (int n = 0; n <= factions[i].GetStartingNumUnits(); n++) { TODO multiple units. for now just stick to ine for simplicity's sake

            Unit newUnit = Instantiate(unitPrefab, CoordToPosition(origin, 1f), Quaternion.identity).GetComponent<Unit>() as Unit;
            if (newUnit != null) {
                newUnit.Init(currentMap, origin, factions[i]);
                newUnit.transform.parent = mapHolder;

                //if we are training neural nets, add a brain!
                if (currentMap.isTraining) {
                    newUnit.gameObject.AddComponent<AIAgent>();
                }
            }
        }

        GeneratePathfindingGraph(currentMap);
        

        return currentMap;
    }

    void GeneratePathfindingGraph(Map map) {
        // Initialize the array
        map.nodes = new Node[map.size.x, map.size.y];

        // Initialize a Node for each spot in the array
        for (int x = 0; x < map.size.x; x++) {
            for (int y = 0; y < map.size.y; y++) {
                map.nodes[x, y] = new Node();
                map.nodes[x, y].pos.x = x;
                map.nodes[x, y].pos.y = y;
            }
        }

        // Now that all the nodes exist, calculate their neighbours
        for (int x = 0; x < map.size.x; x++) {
            for (int y = 0; y < map.size.y; y++) { //map.size.x here too?

                // This is the 4-way connection version:
                if (x > 0)
                    map.nodes[x, y].neighbours.Add(map.nodes[x - 1, y]);
                if (x < map.size.x - 1)
                    map.nodes[x, y].neighbours.Add(map.nodes[x + 1, y]);
                if (y > 0)
                    map.nodes[x, y].neighbours.Add(map.nodes[x, y - 1]);
                if (y < map.size.y - 1)
                    map.nodes[x, y].neighbours.Add(map.nodes[x, y + 1]);

                #region other grid types
                // This is the 8-way connection version (allows diagonal movement)
                // Try left
                /*              if(x > 0) {
                                    nodes[x,y].neighbours.Add( nodes[x-1, y] );
                                    if(y > 0)
                                        nodes[x,y].neighbours.Add( nodes[x-1, y-1] );
                                    if(y < mapSizeY-1)
                                        nodes[x,y].neighbours.Add( nodes[x-1, y+1] );
                                }

                                // Try Right
                                if(x < mapSizeX-1) {
                                    nodes[x,y].neighbours.Add( nodes[x+1, y] );
                                    if(y > 0)
                                        nodes[x,y].neighbours.Add( nodes[x+1, y-1] );
                                    if(y < mapSizeY-1)
                                        nodes[x,y].neighbours.Add( nodes[x+1, y+1] );
                                }

                                // Try straight up and down
                                if(y > 0)
                                    nodes[x,y].neighbours.Add( nodes[x, y-1] );
                                if(y < mapSizeY-1)
                                    nodes[x,y].neighbours.Add( nodes[x, y+1] );
                */

                // This also works with 6-way hexes and n-way variable areas (like EU4)
                #endregion

            }
        }
    }

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

    public Map.Coord[] GetPerlinCoords(PerlinNoise perlin, float upperBound) {
        List<Map.Coord> coords = new List<Map.Coord>();
        int width = currentMap.size.x;
        int height = currentMap.size.y;

        //Step through 2D perlin noise nodes
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                //If value is less than or equal to our upper bound, add this point as a coord
                if (perlin.GetValueAt((float)i / width * perlin.GetWidth(), (float)j / height * perlin.GetHeight()) <= upperBound) {
                    coords.Add(new Map.Coord(i, j));
                   
                }
            }
        }

        return coords.ToArray();
    }

    // Converts coord into real world location
    Vector3 CoordToPosition(Map.Coord coord, float y) {
        Vector3 pos = new Vector3(coord.x, 0f, coord.y) * tileSize;
        return new Vector3(pos.x, y, pos.z);
    }

    // Ensures map is fully accessible using flood fill algorithm
    bool MapIsFloodable(bool[,] obstacleMap, int currentObstacleCount) {

        bool[,] visitedNeighbors = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Map.Coord> queue = new Queue<Map.Coord>();
        //Basis: mapCenter is empty
        queue.Enqueue(currentMap.Center);
        visitedNeighbors[currentMap.Center.x, currentMap.Center.y] = true;
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

    Transform CreateObstacle(Map.Coord randomCoord, float heightScale) {
        float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, heightScale);
        Vector3 obstaclePosition = CoordToPosition(randomCoord, 0f);
        // instantiate obstacle
        Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity) as Transform;
        newObstacle.localScale = new Vector3(tileSize, obstacleHeight, tileSize);
        newObstacle.transform.position = new Vector3(newObstacle.transform.position.x, obstacleHeight / 2 + 0.5f, newObstacle.transform.position.z);
        newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

        // make tiles that obstacles occupy unwalkable
        currentMap.tiles[randomCoord.x, randomCoord.y].SetWalkability(false);

        //adjust color
        Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
        Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
        float colorPercent = randomCoord.y / (float)currentMap.size.y;
        obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
        obstacleRenderer.sharedMaterial = obstacleMaterial;

        return newObstacle;
    }

    #endregion
}

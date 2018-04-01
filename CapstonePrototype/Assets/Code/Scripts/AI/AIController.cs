using System.Collections.Generic;
using UnityEngine;

public class AIController {

    public enum AISkill {
        low,
        medium,
        high
    }

    private static AIController _default = new AIController();
    public static AIController Default { get { return _default; } }

    public AISkill skill = AISkill.medium;
    [Range(0, 1)]
    private float thouroughness; //0 being narrow , 1 being exhaustive in its consideration of possible paths

    private Map map;
    private Unit unit;
    private ClickableTile currentTile;
    private Node currentNode;
    private int currentValue;
    private List<Map.Coord> availableTiles;

    private List<KeyValuePair<List<Node>, int>> paths;
    private List<KeyValuePair<List<Node>, int>> sortedPaths;

    public AIController() {
        availableTiles = new List<Map.Coord>();
        paths = new List<KeyValuePair<List<Node>, int>>();
        sortedPaths = new List<KeyValuePair<List<Node>, int>>();
}

    public void SetUnit(Unit unit) {
        this.unit = unit;
        UpdateInformation();
    }

    public void SetMap(Map map) {
        this.map = map;
    }

    private void UpdateInformation() {
        currentTile = map.tiles[unit.pos.x, unit.pos.y];
        currentNode = map.nodes[unit.pos.x, unit.pos.y];
        currentValue = currentTile.GetValue();
        availableTiles = unit.GetAvailableTileOptions(currentNode, unit.GetMoveSpeed());
        paths.Clear();
        sortedPaths.Clear();
    }

    private void ProcessInformation() {

        //Populate dictionary of paths with assocociated utility values
        foreach (Map.Coord coord in availableTiles) {
            // Generate a path for each available option
            List<Node> tempPath = map.GeneratePathTo(unit.pos, coord, unit);
            // Add path as key, pair it with its utility value
            paths.Add( new KeyValuePair<List<Node>, int>(tempPath, CalcuatePathValue(tempPath)) );
        }

        //Sort path dictionary by value
        sortedPaths = paths;

        sortedPaths.Sort(
            delegate (KeyValuePair<List<Node>, int> pair1,
            KeyValuePair<List<Node>, int> pair2) {
                return pair1.Value.CompareTo(pair2.Value);
            }
        );

        sortedPaths.Reverse();

        /*Debug.Log("AFTER SORT:");

        foreach (KeyValuePair<List<Node>, int> ns in sortedPaths) {
            Debug.Log(ns.Value);
        }*/
    }

    //Returns the path we want to travel to
    public List<Node> MakeDecision(AISkill skill) {
        ProcessInformation();
        List<Node> path = null;

        int index = 0;
        float chance = Random.Range(0,1);
        int pathsCount = sortedPaths.Count-1;

        if (sortedPaths.Count > 0) {
            switch (skill) {
                case AISkill.low:
                if (chance <= .1f) {
                    index = System.Math.Min(2, pathsCount);
                }
                else if (chance > .1f && chance < .4f) {
                    index = System.Math.Min(1, pathsCount);
                }
                else {
                    index = 0;
                }
                break;

                case AISkill.medium:
                if (chance < .3f) {
                    index = System.Math.Min(1, pathsCount);
                }
                else {
                    index = 0;
                }
                break;

                case AISkill.high:
                if (chance < .05f) {
                    index = System.Math.Min(1, pathsCount);
                }
                else {
                    index = 0;
                }
                break;
            }

            Debug.Log("Chose " + index + " best option.");
            path = sortedPaths[index].Key;
        }

        for(int i=1; i < path.Count; i++) {
            map.tiles[path[i].pos.x, path[i].pos.y].DrawHighlight(true, .001f);
        }

        Debug.Log(Time.deltaTime);
        return path;
    }

    private int CalcuatePathValue(List<Node> path) {

        int value, resultingValue = 0, numCapturedTiles = 0;

        //Step through path, tally each captured 
        for (int steps = 0; steps < path.Count; steps++) {
            Node node = path[steps];
            ClickableTile tile = map.tiles[node.pos.x, node.pos.y];

            int sum = tile.GetValue() + steps;
            if (sum % 10 == 0) {
                numCapturedTiles += tile.GetValue() / 10;
            }

            //last node in path, check what final value will be
            if (steps == path.Count - 1) {
                if (sum % 10 == 0) {
                    resultingValue = 1;
                }
                else {
                    resultingValue = sum;
                }
            }
        }

        // Values increase as captured tiles increase and resulting values decrease
        value = numCapturedTiles - resultingValue;
        return value;
    }
}

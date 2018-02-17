using UnityEngine;
using System.Collections;

public enum TileTypes {
    a = 0,
    b = 1,
    c = 2, 
    d = 3
}

[System.Serializable]
public class TileType {

	public string name;
	public GameObject tileVisualPrefab;
    public GameObject tileUIPrefab;

    //public bool isWalkable = true;
	public float movementCost = 1;
    public int value = 2;

}

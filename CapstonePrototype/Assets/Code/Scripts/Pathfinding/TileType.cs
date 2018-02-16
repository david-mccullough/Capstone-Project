using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileType {

	public string name;
	public GameObject tileVisualPrefab;
    public GameObject tileUIPrefab;

    public bool isWalkable = true;
	public float movementCost = 1;
    public int value = 1;

}

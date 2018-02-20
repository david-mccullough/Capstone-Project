using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum UIPrefabs {
    unitPin = 0,
}

public class GameUI : MonoBehaviour {

    public Canvas screenCanvas;
    public Canvas worldCanvas;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    //private GameController controller = GameController.instance;

    public void InitUI() {

        // Unit pin points
        foreach (Unit u in GameController.instance.allUnits) {
            Vector3 uPos = u.transform.position;
            //Instantiate(prefabs[(int)UIPrefabs.unitPin], new Vector3 (uPos.x, uPos.y+0.5f, uPos.z), Quaternion.identity, worldCanvas.transform);
        }
	}
	
}

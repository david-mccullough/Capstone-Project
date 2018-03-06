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
    public GameController controller;

    private TextMesh turnTextMesh;
    private float turnTextSize;

    public Text debugText;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    public void InitUI() {

        controller.nextTurnEvent += OnNewTurn;
        controller.winEvent += OnWin;
        turnTextMesh = GetComponentInChildren<TextMesh>();
        turnTextSize = turnTextMesh.characterSize;
        OnNewTurn(controller.GetCurrentFaction());
        // Unit pin points
        /*foreach (Unit u in GameController.instance.allUnits) {
            Vector3 uPos = u.transform.position;
            //Instantiate(prefabs[(int)UIPrefabs.unitPin], new Vector3 (uPos.x, uPos.y+0.5f, uPos.z), Quaternion.identity, worldCanvas.transform);
        }*/
    }

    void Update() {

        if (turnTextMesh.characterSize != turnTextSize) {
            turnTextMesh.characterSize = Mathf.Lerp(turnTextMesh.characterSize, turnTextSize, 10f * Time.deltaTime);
        }

        debugText.text = "Controller State: " + controller.GetState();
    }

    void OnNewTurn(Faction faction) {
        turnTextMesh.characterSize *= 1.04f;
        turnTextMesh.text = faction.name + " TURN";
        Color c = faction.color;
        c.a = .5f;
        turnTextMesh.GetComponent<Renderer>().material.SetColor("_Color", c);
    }
    
    void OnWin(Faction faction) {
        turnTextMesh.characterSize *= 1.04f;
        turnTextMesh.text = faction.name + " WINS";
        Color c = faction.color;
        c.a = .5f;
        turnTextMesh.GetComponent<Renderer>().material.SetColor("_Color", c);
    }

	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : MonoBehaviour {
    private bool initilized = false;
    private Map map;
    private Unit unit;
    public NeuralNetwork net;
    private Material[] mats;
    private int invalidMoveCount = 0;

    void Start() {
        mats = new Material[transform.childCount];
        for (int i = 0; i < mats.Length; i++)
            mats[i] = transform.GetChild(i).GetComponent<Renderer>().material;
    }

    public void Init(NeuralNetwork net, Map map) {
        this.net = net;
        this.map = map;
        this.unit = this.gameObject.GetComponent<Unit>();
        initilized = true;
    }

    public List<Node> MakeDecision() {
        if (initilized == false) {
            Debug.LogError("Agent is uninitialized");
            return null;
        }

        List<Node> path = null;
        //for (int i = 0; i < mats.Length; i++)
            //mats[i].color = new Color(net.GetFitness());

        float[] inputs = new float[82];

        ///INPUT BOARD STATE
        //convert every tile value into an input
        int myTileIndex = 0;
        for (int i = 0; i < inputs.Length - 1; i++) {
            ClickableTile t = map.tilesOneD[i];

            // if unit cant enter tile, its a 0
            int b = t.IsAvailable() ? 1 : 0; //convert canEnterTile into 1 or 0
            if (t.pos == unit.pos) {
                myTileIndex = i;
            }

            // squash values via sigmoid function
            inputs[i] = Sigmoid((float)(t.GetValue() * b));
        }
        inputs[inputs.Length - 1] = Sigmoid((float)myTileIndex);

        /// GET OUTPUT
        float[] output = net.FeedForward(inputs);
        int index = GetLargest(output);
        Debug.Log("Choosing " + map.tilesOneD[index].pos + " with confidence of " + (float)((int)(output[index]*1000))*.001f + "%");

        /// MAKE DECISION BASED ON OUTPUT
        path = OutputToPath(index);

        /// CALCULATE FITNESS
        net.AddFitness(CalcuatePathValue(path));
        //Debug.Log(CalcuatePathValue(path));

        return path;
    }

    private List<Node> OutputToPath(int index) {
        List<Node> path = null;

        ClickableTile t = map.tilesOneD[index];
        if (t.IsAvailable()) {
            path = map.GeneratePathTo(unit.pos, t.pos, unit);
            net.AddFitness(1f);
            Debug.Log("<b>PICKED VALID PATH!</b>");
        }
        else {
            //if invalid pick worst option, idiot
            path = AIController.Default.MakeWorstDecision();
            invalidMoveCount++;
            if (invalidMoveCount >= 5) {
                //too many invalid choices, kill this agent
                if (unit.faction.IsActive())
                unit.Elimnate();
            }
        }

        return path;
    }

    private float Sigmoid(float x) {
        return (float)(1 / (1 + System.Math.Exp(-x)));
    }


    private int GetLargest(float[] arr) {
        int largest = 0;
        for (int i = 1; i < arr.Length; i++) {
            if (largest < arr[i]) {
                largest = i;
            }
        }
        return largest;
    }

    private int CalcuatePathValue(List<Node> path) {

        int numCapturedTiles = 0;

        //Step through path, tally each captured 
        for (int steps = 0; steps < path.Count; steps++) {
            Node node = path[steps];
            ClickableTile tile = map.tiles[node.pos.x, node.pos.y];

            int sum = tile.GetValue() + steps;
            if (sum % 10 == 0) {
                numCapturedTiles += tile.GetValue() / 10;
            }
        }

        return numCapturedTiles;
    }

}

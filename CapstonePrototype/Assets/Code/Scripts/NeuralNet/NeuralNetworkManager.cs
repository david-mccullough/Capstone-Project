using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour {

    public static NeuralNetworkManager instance = null;

    private List<AIAgent> agentList = null;
    private bool isTraining = false;
    private int populationSize = 2;
    [SerializeField]
    private int generationNumber = 0;
    private int[] layers = new int[] { 82, 10, 10, 81 }; //82 input and 81 output
    private List<NeuralNetwork> nets;
    private GameController gameController;

    void Awake() {
        //singleton pattern
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        FindController();
    }
    
	public void StartTraining() {
        FindController();
        isTraining = true;

        if (generationNumber == 0) { //first gen, init
            InitAgentNeuralNetworks();
        }
        else {
            // evolve winning neural net
            nets.Sort();
            for (int i = 0; i < populationSize / 2; i++) {
                nets[i] = new NeuralNetwork(nets[i+(populationSize / 2)]);
                nets[i].Mutate();
                nets[i].species = Utility.RandomString(3);

                nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy
            }

            // Reset all fitness scores
            for (int i = 0; i < populationSize; i++) {
                nets[i].SetFitness(0f);
            }
        }
        CreateAgents(gameController.allUnits);
    }

    void EndTraining() {

        generationNumber++;
        isTraining = false;
        gameController.RestartGame();
    }
    
    private void CreateAgents(Unit[] units) {
        /*if (agentList != null) {
            for (int i = 0; i < agentList.Count; i++) {
                GameObject.Destroy(agentList[i].gameObject);
            }
        }*/

        agentList = new List<AIAgent>();

        for (int i = 0; i < populationSize; i++) {
            AIAgent agent = units[i].gameObject.GetComponent<AIAgent>();
            agent.Init(nets[i], units[i].map);
            agentList.Add(agent);
        }

    }

    void InitAgentNeuralNetworks() {
        //population must be even, just setting it to 2 in case it's not
        if (populationSize % 2 != 0) {
            populationSize = 2; 
        }

        nets = new List<NeuralNetwork>();
        
        for (int i = 0; i < populationSize; i++) {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            nets.Add(net);
        }
    }

    void EndGame(Faction faction) {
        NeuralNetwork net0 = gameController.allUnits[0].GetComponent<AIAgent>().net;
        NeuralNetwork net1 = gameController.allUnits[1].GetComponent<AIAgent>().net;
        faction.GetUnits()[0].GetComponent<AIAgent>().net.AddFitness(.25f);
        Debug.Log(net0.species + ": " + net0.GetFitness());
        Debug.Log(net1.species + ": " + net1.GetFitness());
        if (this != null)
            Invoke("EndTraining", .1f);
    }

    void FindController() {
        gameController = GameObject.Find("GameManager").GetComponent<GameController>();
        gameController.winEvent += EndGame;
    }
}

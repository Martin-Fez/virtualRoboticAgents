using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TrainerExplore : TrainerParent
{


    [Header("reward Parameters")]
    public float rewardGridFound = 0.1f;

    void Start()
    {
        generatedObstacles = new GameObject[NumberOfObjects + NumOfGoals];

        if(!manualSpawnAgent)
        {
            GameObject newAgent = Instantiate(agentObject, transform.position, transform.rotation);
            newAgent.transform.parent = gameObject.transform.parent;
            agent = newAgent.GetComponent<ControllerAgent>();
        }

        agent.FloorMeshRenderer = basePlate.GetComponent<MeshRenderer>();
        agent.TrainManager = this;
        agent.startPostion = transform.position;

    }

    void Update()
    {
        if (!hasGrid)
        {
            GetGrid();
            if (!hasGrid)
                return;
        }

        CheckTimer();
        ExplorerTrain();

        

    }







    void ExplorerTrain()
    {

        if (hasGrid && CheckGridStatus())
        {

            resetTimer();
            handleEndOfEpisode(1.0f, true);

        }
    }

    
    bool CheckGridStatus()
    {
        int gridSize = gridTimer.Length;

        for (int i = 0; i < gridSize; i++)
        {
            if (gridTimer[i] == 600)
                return false;
        }





        return true;
    }
    



    public override void handleGrid(int GridID) // can be changed to give reward based on last time when explored
    {
        if(gridTimer[GridID] >= 600) // again global timer max would be good
        {
            agent.AddReward(rewardGridFound);
        }
    }




}

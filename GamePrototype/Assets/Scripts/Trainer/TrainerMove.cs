using System.Linq;
using Unity.AI.Navigation;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TrainerMove : TrainerParent
{


    public GameObject GoalObject;
    public int GoalCount;



    int TotalGoalsToReach;


    private int OriginalNumOfGoals;


    [Header("reward Parameters")]
    public float GoalReached = 0.5f;


    private void Start()
    {
        TotalGoalsToReach = GoalCount;
        generatedObstacles = new GameObject[NumberOfObjects + NumOfGoals+ TotalGoalsToReach];
        OriginalNumOfGoals = NumOfGoals;


        if (!manualSpawnAgent)
        {
            GameObject newAgent = Instantiate(agentObject, transform.position, transform.rotation);
            newAgent.transform.parent = gameObject.transform.parent;

            agent = newAgent.GetComponent<ControllerAgent>();
        }
        agent.FloorMeshRenderer = basePlate.GetComponent<MeshRenderer>();
        agent.TrainManager = this;
        agent.startPostion = transform.position;


    }

    private void Update()
    {

        if (!hasGrid)
        {
            GetGrid(); 
            if (!hasGrid)
                return;
        }
        



        CheckTimer();

        if(TotalGoalsToReach <= 0)
            {



            resetTimer();

            handleEndOfEpisode(1.0f,true);
            TotalGoalsToReach = GoalCount; 

        }


    }



    public override void CheckCounts()
    {

    }



    public override void BuildEpisode(GameObject agentObject)
    {

        generatedObstacles[0] = agentObject;


        placeEnemiesToFind();



    }

    public override void handleGoalCollision(GameObject goalObject)
    {
        agent.AddReward(GoalReached);
        TotalGoalsToReach -= 1;
        goalObject.SetActive(false);
    }





    private void placeEnemiesToFind()
    {

        NumOfGoals = OriginalNumOfGoals; //Reseet num of goals

        int NumToGenerate = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("goals", GoalCount);


        for (int i = 0; i < NumToGenerate; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = GoalObject;


            Vector3 newObstaclePostion = generatePosition(i);

            if (GenCheckMax <= GenCheck)
            {
                NumOfGoals += i; 

                return;
            }


            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;



            generatedObstacles[NumOfGoals + i] = TempObj; // Place after the orig goals

        }


        if (GenCheckMax > GenCheck)
        {
            NumOfGoals += GoalCount; // we add to numOfGoals 

        }



    }




    public override void EpisodeEnded()
    {
        if (generatedObstacles == null)
        {
            return;
        }

        for (int i = 0; i < generatedObstacles.Length; i++)
        {
            if (i < OriginalNumOfGoals) // if i is smaller than numofGoals continue, ie don't destroy objects on the list that are goals and are not generated // ALSO this only applies for the original!
                continue;

            Destroy(generatedObstacles[i]);
        }

    }


}

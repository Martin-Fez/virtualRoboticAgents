using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TrainerBattle : TrainerParent
{

    public GameObject EnemyBattleObject;
    public int EnemyCount;

    int TotalEnemiesToDefeat;


    private int OriginalNumOfGoals;


    [Header("reward Parameters")]
    public float rewardDamageTaken = -0.01f;
    public float rewardDamageGiven = 0.01f;
    public float rewardKillEnemy = 0.5f;
    public float rewardLifeLost = -0.5f;
    public float rewardEnemyDefeat = 1f;





    private void Start()
    {
        TotalEnemiesToDefeat = EnemyCount;
        generatedObstacles = new GameObject[NumberOfObjects + NumOfGoals+ TotalEnemiesToDefeat];
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

        if(TotalEnemiesToDefeat <= 0)
            {



            resetTimer();

            handleEndOfEpisode(1.0f,true);

        }


    }

    public override void handleDamage(ControllerParent TargetAgent)
    {
        if(TargetAgent == agent) // if TargetAgent is the main agent
        {
            agent.AddReward(rewardDamageTaken);
        }
        else // else give reward
        {

            agent.AddReward(rewardDamageGiven);
        }


    }

    public override void handleDeath(ControllerParent TargetAgent)
    {
        if (TargetAgent == agent) // if TargetAgent is the main agent
        {
            agent.AddReward(rewardLifeLost);
            if(TargetAgent.lives <= 0 )
            {
                handleEndOfEpisode(-1f,false);
            }

        }
        else // else give reward
        {

            agent.AddReward(rewardKillEnemy);
            if (TargetAgent.lives <= 0)
            {
                agent.AddReward(rewardEnemyDefeat);
                TargetAgent.gameObject.SetActive(false);
                TotalEnemiesToDefeat -= 1;
            }
        }



    }



    public override void BuildEpisode(GameObject agentObject)
    {
        TotalEnemiesToDefeat = EnemyCount;


        generatedObstacles[0] = agentObject;


        placeEnemiesToFind();

        placeObstacles();

        basePlate.GetComponent<NavMeshSurface>().BuildNavMesh();


            
        resetPositions();

    }

    public override void CheckCounts()
    {
        for (int i = 0; i < EnemyCount; i++)
        {
            //Debug.Log("Checking enemy position again X:" + generatedObstacles[1 + i].transform.position);
        }
    }

    void resetPositions()
    {
        for (int i = 0; i < EnemyCount; i++)
        {
            generatedObstacles[1 + i].GetComponent<NavMeshAgent>().Warp(generatedObstacles[1 + i].transform.position);
        }

    }


    private void placeEnemiesToFind()
    {

        NumOfGoals = OriginalNumOfGoals; //Reset num of goals


        for (int i = 0; i < EnemyCount; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = EnemyBattleObject;


            Vector3 newObstaclePostion = generatePosition(i);

            if (GenCheckMax <= GenCheck)
            {
                //Debug.Log("failed to generate suitable position quiting generation");
                NumOfGoals += i; // we add to numOfGoals 

                return;
            }




            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;
            TempObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            TempObj.GetComponent<StatePatternEnemy>().TrainManager = this;
            TempObj.GetComponent<StatePatternEnemy>().GridParent = GridParent;



            generatedObstacles[NumOfGoals + i] = TempObj; // Place after the orig goals




        }


        if (GenCheckMax > GenCheck)
        {
            //Debug.Log("correct generation");
            NumOfGoals += EnemyCount; // we add to numOfGoals 

        }





    }




    public override void EpisodeEnded()
    {
        if (generatedObstacles == null)
        {
            //Debug.Log("Array is null, wait for it generate objects");
            return;
        }

        for (int i = 0; i < generatedObstacles.Length; i++)
        {
            if (i < OriginalNumOfGoals) // if i is smaller than numofGoals continue, ie don't destroy objects on the list that are goals and are not generated // ALSO this only applies for the original!
                continue;

            Destroy(generatedObstacles[i]);
        }

        // destroy created objects
    }


}

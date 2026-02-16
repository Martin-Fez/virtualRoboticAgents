using System.Linq;
using Unity.AI.Navigation;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UIElements;

public class TrainerDodgeEnemy : TrainerParent
{


    public GameObject TurretEnemyObject;
    public int enemyCountTurret;

    int TotalEnemiesToDefeat;


    private int OriginalNumOfGoals;

    private float rewardTimer = 0;

    [Header("reward Parameters")]
    public float rewardDamageTaken = -0.1f;
    public float rewardTimeAlive = 0.01f;
    public float rewardDamageGiven = 0.1f;
    public float rewardKilledEnemy = 1f;


    private void Start()
    {
        TotalEnemiesToDefeat = enemyCountTurret;
        generatedObstacles = new GameObject[NumberOfObjects + NumOfGoals+ TotalEnemiesToDefeat];
        OriginalNumOfGoals = NumOfGoals;
        rewardTimer = 0;

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


         // has grid currently not working as intended
        if (!hasGrid)
        {
            GetGrid(); 
            if (!hasGrid)
                return;
        }
        



        CheckTimer();

        if (TotalEnemiesToDefeat <= 0)
        {



            resetTimer();

            TotalEnemiesToDefeat = enemyCountTurret;
            handleEndOfEpisode(1.0f, true);

        }




    }

    protected override void CheckTimer()
    {
        rewardTimer += Time.deltaTime;
        InternalTimer -= Time.deltaTime;



        if (InternalTimer <= endTime)
        {
            rewardTimer = 0;
            handleEndOfEpisode(1f, true);
        }

        if (rewardTimer > 5)
        {
            rewardTimer = 0;
            agent.AddReward(rewardTimeAlive); // change to variable
        }



    }

    public override void handleDamage(ControllerParent TargetAgent) // in this case these are extra // could be usefull when we need to check who is who
    {
        if (TargetAgent == agent) // if TargetAgent is the main agent
        {
            agent.AddReward(rewardDamageTaken);
        }
        else // else give reward
        {

            agent.AddReward(rewardDamageGiven);
        }



        agent.AddReward(rewardDamageTaken);
    }

    public override void handleDeath(ControllerParent TargetAgent)
    {
        if (TargetAgent == agent) // if TargetAgent is the main agent
        {
            handleEndOfEpisode(-1f, false);
        }
        else
        {
            agent.AddReward(rewardKilledEnemy);
            TargetAgent.gameObject.SetActive(false);
            TotalEnemiesToDefeat -= 1;
        }
    }


    public override void BuildEpisode(GameObject agentObject)
    {

        generatedObstacles[0] = agentObject;


        int numEnemyTurret = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("turrets", enemyCountTurret);

        if (numEnemyTurret > enemyCountTurret)
            numEnemyTurret = enemyCountTurret;

        TotalEnemiesToDefeat = numEnemyTurret;



        placeEnemiesToFind();

        placeObstacles();




    }


    private void placeEnemiesToFind()
    {

        NumOfGoals = OriginalNumOfGoals; //Reset num of goals


        int numEnemyTurret = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("turrets", enemyCountTurret);

        if (numEnemyTurret > enemyCountTurret)
            numEnemyTurret = enemyCountTurret;


        for (int i = 0; i < numEnemyTurret; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = TurretEnemyObject;


            Vector3 newObstaclePostion = generatePosition(i);

            if (GenCheckMax <= GenCheck)
            {
                NumOfGoals += i; // we add to numOfGoals 

                return;
            }


            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;
            TempObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            TempObj.GetComponent<DummyControl>().agent = agent.gameObject; // Get gameobject of agent for the turret
            TempObj.GetComponent<DummyControl>().TrainManager = this;



            generatedObstacles[NumOfGoals + i] = TempObj; // Place after the orig goals

        }


        if (GenCheckMax > GenCheck)
        {
            //Debug.Log("correct generation");
            NumOfGoals += numEnemyTurret; // we add to numOfGoals 

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
            if (i < OriginalNumOfGoals) // if it is smaller than numofGoals continue, ie don't destroy objects on the list that are goals and are not generated // ALSO this only applies for the original!
                continue;

            Destroy(generatedObstacles[i]);
        }

    }


}

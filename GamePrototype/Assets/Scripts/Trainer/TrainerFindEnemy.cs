using System.Linq;
using Unity.AI.Navigation;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TrainerFindEnemy : TrainerParent
{

    public GameObject StaticEnemyObject;
    public int EnemyCountStatic;
    public GameObject MovingEnemyObject;
    public GameObject PointPrefab;
    public int EnemyCountMoving;
    public int EnemyPoints;



    int TotalEnemiesToFind;

    public int GoalCountOfEnemiesToFind;


    private int OriginalNumOfGoals;


    [Header("reward Parameters")]
    public float rewardSee = 0.01f;

    public float rewardFound = 0.5f;

    public float timerNeededToSee = 3;


    private void Start()
    {
        TotalEnemiesToFind = EnemyCountStatic + EnemyCountMoving;

        if (GoalCountOfEnemiesToFind == 0 || GoalCountOfEnemiesToFind > TotalEnemiesToFind)
            GoalCountOfEnemiesToFind = TotalEnemiesToFind;


        generatedObstacles = new GameObject[NumberOfObjects + NumOfGoals+ TotalEnemiesToFind + EnemyCountMoving*EnemyPoints];
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
        



        int enemiesFound = (EnemyCountMoving + EnemyCountStatic) - TotalEnemiesToFind;

        if ( ( ((InternalTimer-10) <= endTime) && (enemiesFound >= GoalCountOfEnemiesToFind) ) || TotalEnemiesToFind <= 0) // bandaid, we just end it 10 seconds early
        {


            resetTimer();

            handleEndOfEpisode(1.0f,true);
            TotalEnemiesToFind = EnemyCountMoving + EnemyCountStatic; 

        }
        CheckTimer();

        checkInternalTimers();

    }

    void checkInternalTimers()
    {
        int numEnemyStatic = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("staticEnemies", EnemyCountStatic); //not properly implemented

        if (numEnemyStatic > EnemyCountStatic)
            numEnemyStatic = EnemyCountStatic;

        int numEnemyMoving = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("movingEnemies", EnemyCountMoving);//not properly implemented

        if (numEnemyMoving > EnemyCountMoving)
            numEnemyMoving = EnemyCountMoving;


        for (int i = 0; i < numEnemyStatic; i++)
        {
            float EnemyTime = generatedObstacles[1 + i].GetComponent<DummyControlMove>().seenTimer;
            float rewardSeeAccum = generatedObstacles[1 + i].GetComponent<DummyControlMove>().accumulatedRewardSee;
            bool ActiveCheck = generatedObstacles[1 + i].activeSelf;

            if (ActiveCheck && EnemyTime == 0 && rewardSee != 0)
            {
                agent.AddReward(-rewardSeeAccum);
                generatedObstacles[1 + i].GetComponent<DummyControlMove>().accumulatedRewardSee = 0;
            }

        }



        for (int i = 0; i < numEnemyMoving; i++)
        {

            float EnemyTime = generatedObstacles[1 + numEnemyStatic + i + i * EnemyPoints].GetComponent<DummyControlMove>().seenTimer;
            float rewardSeeAccum = generatedObstacles[1 + numEnemyStatic + i + i * EnemyPoints].GetComponent<DummyControlMove>().accumulatedRewardSee;
            bool ActiveCheck = generatedObstacles[1 + numEnemyStatic + i + i * EnemyPoints].activeSelf;

            if (ActiveCheck && EnemyTime == 0 && rewardSee != 0)
            {
                agent.AddReward(-rewardSeeAccum);
                generatedObstacles[1 + numEnemyStatic + i + i * EnemyPoints].GetComponent<DummyControlMove>().accumulatedRewardSee = 0;
            }


        }

    }


    public override void CheckCounts()
    {

    }

    public override void handleEnemyPlayer(GameObject EnemyPlayer) // for now it ends when found, change to when found to hide/delete the enemy and decrease the TotalEnemyCount
    {


        if (EnemyPlayer == agent.gameObject)
        {
            //Debug.Log("detected self");
            return;
        }

        if (EnemyPlayer.GetComponent<DummyControlMove>() == null)
        {
            //Debug.Log("incorrect object, please find a fix: "+ EnemyPlayer.gameObject);
            return;
        }


        float AgentSeenTime = EnemyPlayer.GetComponent<DummyControlMove>().beingSeen();


        float lenToSee = (float)Academy.Instance.EnvironmentParameters.GetWithDefault("timerNeededToSee", timerNeededToSee);

        float addedtime = AgentSeenTime * 0.5f - EnemyPlayer.GetComponent<DummyControlMove>().accumulatedRewardSee; // how long see - the seen time we get how much new time we got

        if ( ( (EnemyPlayer.GetComponent<DummyControlMove>().accumulatedRewardSee*2) < lenToSee)) 
        {
            agent.AddReward(addedtime);
            EnemyPlayer.GetComponent<DummyControlMove>().accumulatedRewardSee += addedtime;
        }



        if (AgentSeenTime > lenToSee)
        {
            agent.AddReward(rewardFound);
            EnemyPlayer.GetComponent<DummyControlMove>().accumulatedRewardSee = 0;


            EnemyPlayer.SetActive(false);
            TotalEnemiesToFind -= 1;

        }




    }

    public override void BuildEpisode(GameObject agentObject)
    {

        generatedObstacles[0] = agentObject;

        TotalEnemiesToFind = EnemyCountStatic + EnemyCountMoving;



        placeEnemiesToFind();

        placeObstacles();


        basePlate.GetComponent<NavMeshSurface>().BuildNavMesh();
        resetPositions();



    }

    void resetPositions()
    {
        int numEnemyStatic = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("staticEnemies", EnemyCountStatic);

        if (numEnemyStatic > EnemyCountStatic)
            numEnemyStatic = EnemyCountStatic;

        int numEnemyMoving = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("movingEnemies", EnemyCountMoving);

        if (numEnemyMoving > EnemyCountMoving)
            numEnemyMoving = EnemyCountMoving;




        for (int i = 0; i < numEnemyStatic; i++)
        {

            generatedObstacles[1 + i].GetComponent<NavMeshAgent>().Warp(generatedObstacles[1 + i].transform.position);
        }



        for (int i = 0; i < numEnemyMoving; i++)
        {

            generatedObstacles[1 + EnemyCountStatic + i + i * EnemyPoints].GetComponent<NavMeshAgent>().Warp(generatedObstacles[1 + numEnemyStatic + i + i * EnemyPoints].transform.position);
        }
    }



    private void placeEnemiesToFind()
    {

        NumOfGoals = OriginalNumOfGoals; //Reset num of goals

        int numEnemyStaticToGenerate = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("staticEnemies", EnemyCountStatic);

        if (numEnemyStaticToGenerate > EnemyCountStatic)
            numEnemyStaticToGenerate = EnemyCountStatic;


        for (int i = 0; i < numEnemyStaticToGenerate; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = StaticEnemyObject;


            Vector3 newObstaclePostion = generatePosition(i);

            if (GenCheckMax <= GenCheck)
            {
                Debug.Log("failed to generate suitable position quiting generation");
                NumOfGoals += i; // we add to numOfGoals 

                return;
            }


            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;
            TempObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);



            generatedObstacles[NumOfGoals + i] = TempObj; // Place after the orig goals

        }


        if (GenCheckMax > GenCheck)
        {
            NumOfGoals += EnemyCountStatic; // we add to numOfGoals 

        }


        int numEnemyMovingToGenerate = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("movingEnemies", EnemyCountMoving);

        if (numEnemyMovingToGenerate > EnemyCountMoving)
            numEnemyMovingToGenerate = EnemyCountMoving;


        for (int i = 0; i < numEnemyMovingToGenerate; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = MovingEnemyObject;


            Vector3 newObstaclePostion = generatePosition(i + i * EnemyPoints);





            if (GenCheckMax <= GenCheck)
            {
                Debug.Log("failed to generate suitable position quiting generation");
                NumOfGoals += i + i * EnemyPoints; // we add to numOfGoals 

                return;
            }



            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;
            TempObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);




            generatedObstacles[NumOfGoals + i + i* EnemyPoints] = TempObj; // Place after the orig goals

            TempObj.GetComponent<DummyControlMove>().wayPointsDummy = new Transform[EnemyPoints];


            for (int j = 0; j < EnemyPoints; j++)
            {
                Vector3 newPointPosition;



                do
                {

                    GenCheck = 0; // since we are not checking GenCheck with these
                    newPointPosition = generatePosition((i + 1) + ((i * EnemyPoints) + j)); // keep it a i, we do not care about the position of other points

                } while (GenCheck >= GenCheckMax); // this is to ensure a point DOES generate no matter what


                GameObject TempPoint = Instantiate(PointPrefab, this.transform);


                TempPoint.transform.position = newPointPosition;

                TempObj.GetComponent<DummyControlMove>().wayPointsDummy[j] = TempPoint.transform; // could prob generate transforms instead


                generatedObstacles[NumOfGoals + (i+1) + ((i * EnemyPoints ) + j)] = TempPoint; // the 1 is from the tempObj we already added


            }




        }


        if (GenCheckMax > GenCheck)
        {
            NumOfGoals += EnemyCountMoving + EnemyCountMoving * EnemyPoints; // we add to numOfGoals 

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

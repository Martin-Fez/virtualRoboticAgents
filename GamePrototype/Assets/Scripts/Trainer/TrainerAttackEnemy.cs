using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TrainerAttackEnemy : TrainerParent
{
    public GameObject StaticEnemyObject;
    public int EnemyCountStatic;
    public GameObject MovingEnemyObject;
    public GameObject PointPrefab;
    public int EnemyCountMoving;
    public int EnemyPoints;

    int TotalEnemiesToDefeat;


    private int OriginalNumOfGoals;

    [Header("reward Parameters")]
    public float rewardDamageGiven = 0.1f;
    public float rewardKilledEnemy = 1f;
    public float GoalCountOfEnemiesToDefeat;


    private void Start()
    {
        TotalEnemiesToDefeat = EnemyCountStatic + EnemyCountMoving;
        generatedObstacles = new GameObject[NumberOfObjects + NumOfGoals+ TotalEnemiesToDefeat + EnemyCountMoving*EnemyPoints];
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

        int enemiesDefeated = (EnemyCountMoving + EnemyCountStatic) - TotalEnemiesToDefeat;


        if ((((InternalTimer - 10) <= endTime) && (enemiesDefeated >= GoalCountOfEnemiesToDefeat)) || TotalEnemiesToDefeat <= 0)
        {



            resetTimer();

            TotalEnemiesToDefeat = EnemyCountMoving + EnemyCountStatic;
            handleEndOfEpisode(1.0f, true);

        }



    }

    void resetPositions()
    {
        for (int i = 0; i < EnemyCountStatic; i++)
        {

            generatedObstacles[1 + i].GetComponent<NavMeshAgent>().Warp(generatedObstacles[1 + i].transform.position);
        }



        for (int i = 0; i < EnemyCountMoving; i++)
        {

            generatedObstacles[1 + EnemyCountStatic + i + i * EnemyPoints].GetComponent<NavMeshAgent>().Warp(generatedObstacles[1 + EnemyCountStatic + i + i * EnemyPoints].transform.position);
        }
    }

    public override void handleDamage(ControllerParent TargetAgent)
    {
        agent.AddReward(rewardDamageGiven);
    }

    public override void handleDeath(ControllerParent TargetAgent)
    {
        agent.AddReward(rewardKilledEnemy);
        TargetAgent.gameObject.SetActive(false);
        TotalEnemiesToDefeat -= 1;
    }



    public override void BuildEpisode(GameObject agentObject)
    {

        TotalEnemiesToDefeat = EnemyCountStatic + EnemyCountMoving;



        generatedObstacles[0] = agentObject;


        placeEnemiesToFind();

        placeObstacles();



        basePlate.GetComponent<NavMeshSurface>().BuildNavMesh();

        resetPositions();

    }


    private void placeEnemiesToFind()
    {

        NumOfGoals = OriginalNumOfGoals; //Reset num of goals


        for (int i = 0; i < EnemyCountStatic; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = StaticEnemyObject;


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
            TempObj.GetComponent<DummyControlMove>().TrainManager = this;



            generatedObstacles[NumOfGoals + i] = TempObj; // Place after the orig goals

        }


        if (GenCheckMax > GenCheck)
        {
            //Debug.Log("correct generation");
            NumOfGoals += EnemyCountStatic; // we add to numOfGoals 

        }

        for (int i = 0; i < EnemyCountMoving; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = MovingEnemyObject;


            Vector3 newObstaclePostion = generatePosition(i + i * EnemyPoints);





            if (GenCheckMax <= GenCheck)
            {
                NumOfGoals += i + i * EnemyPoints; // we add to numOfGoals 

                return;
            }


            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;
            TempObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            TempObj.GetComponent<DummyControlMove>().TrainManager = this;



            generatedObstacles[NumOfGoals + i + i* EnemyPoints] = TempObj; // Place after the orig goals

            TempObj.GetComponent<DummyControlMove>().wayPointsDummy = new Transform[EnemyPoints];


            // It does not matter where the points are generated at this point since obstacles get generated afterwards
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

                TempObj.GetComponent<DummyControlMove>().wayPointsDummy[j] = TempPoint.transform; 

                generatedObstacles[NumOfGoals + (i+1) + ((i * EnemyPoints ) + j)] = TempPoint; // the 1 is from the tempObj we already added


            }




        }


        if (GenCheckMax > GenCheck)
        {
            //Debug.Log("correct generation");
            NumOfGoals += EnemyCountMoving + EnemyCountMoving * EnemyPoints; // we add to numOfGoals 

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

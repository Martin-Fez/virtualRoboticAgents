using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class TrainerParent : MonoBehaviour
{
    public GameObject[] obstacles;
    public int NumberOfObjects;
    public GameObject basePlate;

    public float obstacleDistance;
    public int GenCheckMax;
    protected int GenCheck = 0;



    public GameObject agentObject;
    public ControllerAgent agent;
    public GameObject goal;


    protected GameObject[] generatedObstacles;

    public int NumOfGoals = 1;

    public GameObject GridParent;

    public Transform[] gridPoints; // Array of GridsPoints
    public float[] gridTimer; // Add to start that all values get calculated or something else
    public int currentGridIndex; // The index the robot is heading towards right now
    [HideInInspector] public bool hasGrid = false;

    public float ObjectHeight = 3;
    public float agentHeight = 3;




    public float InternalTimer;
    public float endTime = 0;

    public double EpisodeNum = 0;  
    public bool lastEpisodeSuccess = false;

    public bool manualSpawnAgent = false;

    float penaltyTimer = 0;
    public float timePenalty = -0.01f;

    public float lastEpisodeCulmulativeReward = 0;




    void Start()
    {



    }

    void Update()
    {



    }

    public virtual void CheckCounts()
    {

    }




    public virtual void BuildEpisode(GameObject agentObject)
    {
        generatedObstacles[0] = agentObject;
        placeObstacles();
        


    }

    protected void placeObstacles()
    {

        int NumToGenerate = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("obstacles",NumberOfObjects);


        if (NumToGenerate > NumberOfObjects) 
            NumToGenerate = NumberOfObjects;



        for (int i = 0; i < NumToGenerate; i++)
        {
            GenCheck = 0;


            GameObject chosenObject = obstacles[Random.Range(0, obstacles.Length)];


            Vector3 newObstaclePostion = generatePosition(i);

            if (GenCheckMax <= GenCheck)
            {
                Debug.Log("failed to generate suitable position quiting generation");
                return;
            }



            GameObject TempObj = Instantiate(chosenObject, this.transform);


            TempObj.transform.position = newObstaclePostion;
            TempObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);


            generatedObstacles[NumOfGoals + i] = TempObj; // Place after the goals



        }
    }

    protected Vector3 generatePosition(int generatedLength)
    {
        Vector3 newObstaclePostion;

        do
        {
            newObstaclePostion = new Vector3(UnityEngine.Random.Range(-45f, +45f), ObjectHeight, UnityEngine.Random.Range(-45f, 45f));


            newObstaclePostion = newObstaclePostion + basePlate.transform.position;
            newObstaclePostion.y = ObjectHeight;
        } while (ObstaclesDistance(newObstaclePostion, generatedLength) && GenCheckMax > GenCheck++);




        return newObstaclePostion;
    }


    private bool ObstaclesDistance(Vector3 collider,int generatedLength)
    {
        for(int i = 0; i < generatedLength+NumOfGoals; i++) // needs to check properly, added num of goals
        {
            float ColliderDistance2D = Vector3.Distance(collider, generatedObstacles[i].transform.position);

            if (ColliderDistance2D < obstacleDistance)
            {
            
            return true;
            }

        }
        return false;
    }


    public virtual void EpisodeEnded()
    {

        if (generatedObstacles == null) 
        {
            return;
        }


        for(int i = 0; i < generatedObstacles.Length; i++)
        {

            if (i < NumOfGoals) // if i is smaller than numofGoals continue, ie don't destroy objects on the list that are goals and are not generated
                continue;

            Destroy(generatedObstacles[i]); 
        }

    }



    protected void GetGrid()
    {
        int gridSize = GridParent.GetComponent<GridMaker>().gridSizeX * GridParent.GetComponent<GridMaker>().gridSizeZ;
        gridPoints = new Transform[gridSize];
        gridPoints = GridParent.GetComponent<GridMaker>().gridPoints;

        gridTimer = new float[gridPoints.Length];

        for (int i = 0; i < gridTimer.Length; i++) gridTimer[i] = 600;

        if (gridTimer.Length > 0)
            hasGrid = true;
    }




    protected virtual void CheckTimer()
    {
        InternalTimer -= Time.deltaTime;
        penaltyTimer += Time.deltaTime;


        if (InternalTimer <= endTime)
            {
            penaltyTimer = 0;
  
            resetTimer();
            handleEndOfEpisode(-1.0f,false);

        }


        if (penaltyTimer > 1)
        {
            penaltyTimer = 0;
            agent.AddReward(timePenalty); // change to variable
        }


    }

    public virtual void handleGrid(int GridID) // can be changed to give reward based on last time when explored
    {
        if(gridTimer[GridID] < 600) 
        {
            //agent.AddReward(0.1f);
        }
    }

    public virtual void handleDeath(ControllerParent TargetAgent)
    {
        return;
    }

    public virtual void handleDamage(ControllerParent TargetAgent)
    {
        return;
    }

    public virtual void handleGoalCollision(GameObject goalObject)
    {
        return;
    }

    public virtual void handleEnemyPlayer(GameObject EnemyPlayer)
    {
        return;
    }


    public void handleEndOfEpisode(float reward, bool win)
    {
        resetTimer();

        lastEpisodeCulmulativeReward = agent.currentReward;


        if(win)
        {
            lastEpisodeSuccess = true;
            agent.FloorMeshRenderer.material = agent.winMaterial;
        }
        else
        {
            lastEpisodeSuccess = false;

            agent.FloorMeshRenderer.material = agent.loseMaterial;

        }


        agent.AddReward(reward);
        agent.EndEpisode();
    }

    protected void resetTimer()
    {
        InternalTimer = 600; // could be made into a proper variable
        for (int i = 0; i < gridTimer.Length; i++)
        {
            gridTimer[i] = 600;
        }
    }


}

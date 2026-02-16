using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class StatePatternEnemy : ControllerParent
{
    private Rigidbody rb;




    public float searchDuration; // How long we seach in Alert state
    public float searchTurnSpeed; // how fast we turn in alert state
    public float sightRange; // how far does the enemy see. This is distance of the raycast

    public Vector3 startPostion;

    bool hasState = false;


    

    public float robotSpeed;
    public float robotAcceleration;



    public Transform[] gridPoints; // Array of GridsPoints

    public GameObject GridParent;
    [HideInInspector] public bool hasGrid = false;

    public float[] gridTimer; // Add to start that all values get calculated or something else
    public int currentGridIndex; // The index the robot is heading towards right now

    bool dashed;
    float dashCooldown;
    public float maxDashCooldown;
    public float dashSpeed;


    public TrainerParent TrainManager;



    public Transform eye; // this is the eye, we will send raycasts from here
    public MeshRenderer indicator; // this is the box above player. It Changes color based on state.

    [HideInInspector] public Transform chaseTarget; // This is what we chase in chase state. Usually Player.
    [HideInInspector] public bool dodgeBullet;

    [HideInInspector] public Vector3 previousChaseTargetPosition;
    [HideInInspector] public Vector3 currentChaseTargetPosition;

    [HideInInspector] public Vector3 currentTargetDirection;


    [HideInInspector] public IEnemyState currentState;
    [HideInInspector] public PatrolState patrolState;
    [HideInInspector] public AlertState alertState;
    [HideInInspector] public ChaseState chaseState;
    [HideInInspector] public TrackingState trackingState;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    public Vector3 lastKnownPlayerPostition; // When sight to player is lost, the position of the player is stored in this variable

    protected override void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        patrolState = new PatrolState(this);
        alertState = new AlertState(this);
        chaseState = new ChaseState(this);
        trackingState = new TrackingState(this);
    }



    void Start()
    {
        // Let's tell enemy that in the beginning it's current state is the patrolStaet
        rb = GetComponent<Rigidbody>();

        startPostion = transform.position;
        GetComponent<NavMeshAgent>().speed = robotSpeed;
        GetComponent<NavMeshAgent>().acceleration = robotAcceleration;
        currentState = patrolState;


        hasState = true;

    }

    void Update()
    {
        if (!hasGrid)
        {
            GetGrid();
            if (!hasGrid)
                return;
        }


        if (dead)
            return;



        RequestDecision(); 


        runCooldowns();


        currentState.UpdateState(); // we are running the UpdateState in the object that is currentState
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasState)
            return;

        currentState.OnTriggerEnter(other);



    }

    public void Dash()
    {



        Vector3 movementVector = navMeshAgent.desiredVelocity;




        bool DashAction = false;
        var DashChance = 0.75f;

        if (chaseTarget && (Vector3.Distance(chaseTarget.position, this.transform.position) > 40) && Random.value < DashChance)
            DashAction = true;







        if (DashAction && !dashed && movementVector.magnitude != 0)
        {
            Vector3 relativeMovement = transform.TransformDirection(movementVector);

            rb.AddForce(movementVector * dashSpeed, ForceMode.VelocityChange);


            dashed = true;
        }

        if (dashed)
        {
            dashCooldown += Time.deltaTime;



            if (dashCooldown > maxDashCooldown)
            {
                dashed = false;
                dashCooldown = 0;
            }



        }


    }

    public override void LoseHealth()
    {
        if(navMeshAgent.isOnNavMesh)
            navMeshAgent.ResetPath();


        if (dead)
            return;

        health -= 10;
        if (TrainManager)
            TrainManager.handleDamage(this);

        if (health <= 0)
        {
            if (TrainManager)
            {
                TrainManager.handleDeath(this);

            }


            lives -= 1;


            if (lives > 0)
                StartCoroutine(death());
        }
    }



    override protected IEnumerator death()
    {




        dead = true;
        GetComponent<NavMeshAgent>().speed = 0;
        GetComponent<NavMeshAgent>().acceleration = 0;
        GetComponent<NavMeshAgent>().velocity = Vector3.zero;
        GetComponent<NavMeshAgent>().isStopped = true;




        health = 100;
        gameObject.transform.position = startPostion;
        var render = GetComponent<Renderer>();
        render.enabled = false;

        yield return new WaitForSecondsRealtime(3);




        render.enabled = true;

        dead = false;
        GetComponent<NavMeshAgent>().speed = robotSpeed;
        GetComponent<NavMeshAgent>().acceleration = robotAcceleration;
        if(navMeshAgent.isOnNavMesh)
            GetComponent<NavMeshAgent>().isStopped = false;



    }


    public int FindNextGrid()
    {


        float BestGridValue = Mathf.Infinity;
        int BestGridIndex = 0; // Default value
        Vector3 middle = new Vector3(0, 0, 0);

        for (int i = 0; i < gridTimer.Length; i++)
        {

            float distanceFromMiddle = Vector3.Distance(middle, gridPoints[i].position);
            float distanceFromPlayer = Vector3.Distance(transform.position, gridPoints[i].position);






            float GridValue = (600-gridTimer[i])*10 + distanceFromMiddle + distanceFromPlayer;

            if (GridValue < BestGridValue)
            {
                BestGridValue = GridValue;
                BestGridIndex = i;

            }



        }

        return BestGridIndex;


    }



    void GetGrid()
    {
        int gridSize = GridParent.GetComponent<GridMaker>().gridSizeX * GridParent.GetComponent<GridMaker>().gridSizeZ;
        gridPoints = new Transform[gridSize];
        gridPoints = GridParent.GetComponent<GridMaker>().gridPoints;

        gridTimer = new float[gridPoints.Length];

        for (int i = 0; i < gridTimer.Length; i++) gridTimer[i] = 600;

        if(gridTimer.Length > 0)
            hasGrid = true;
    }




}

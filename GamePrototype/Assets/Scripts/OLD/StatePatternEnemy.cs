using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StatePatternEnemy : MonoBehaviour
{

    public float searchDuration; // How long we seach in Alert state
    public float searchTurnSpeed; // how fast we turn in alert state
    public float sightRange; // how far does the enemy see. This is distance of the raycast
    public Transform[] wayPoints; // Array of waypoints, There can be any number of waypoints
    public Transform eye; // this is the eye, we will send raycasts from here
    public MeshRenderer indicator; // this is the box above player. It Changes color based on state.

    [HideInInspector] public Transform chaseTarget; // This is what we chase in chase state. Usually Player.
    [HideInInspector] public IEnemyState currentState;
    [HideInInspector] public PatrolState patrolState;
    [HideInInspector] public AlertState alertState;
    [HideInInspector] public ChaseState chaseState;
    [HideInInspector] public TrackingState trackingState;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    public Vector3 lastKnownPlayerPostition; // When sight to player is lost, the position of the player is stored in this variable

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        patrolState = new PatrolState(this);
        alertState = new AlertState(this);
        chaseState = new ChaseState(this);
        trackingState = new TrackingState(this);
    }




    // Start is called before the first frame update
    void Start()
    {
        // Let's tell enemy that in the beginning it's current state is the patrolStaet
        currentState = patrolState;
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(); // we are running the UpdateState in the object that is currentState
        // if the current state = patrolState we run UpdateState function in patrolState.cs file
    }

    private void OnTriggerEnter(Collider other)
    {
        // we call currentState's ontriggerEnter
        currentState.OnTriggerEnter(other);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DummyControlMove : StatePatternEnemy // maybe change it to regular
{



    public Transform[] wayPointsDummy; // Array of waypoints, There can be any number of waypoints
    int nextWaypoint; // Index of the waypoint in the array



    public float seenTimer = 0;
    public float notSeenTimer = 0;

    public float accumulatedRewardSee = 0;

    void Start()
    {
        nextWaypoint = 0;
        GetComponent<NavMeshAgent>().speed = robotSpeed;
        GetComponent<NavMeshAgent>().acceleration = robotAcceleration;


        seenTimer = 0;
        notSeenTimer = 0;

    }

    void Update()
    {


        if(seenTimer > 0)
        {
            notSeenTimer += Time.deltaTime;
            if (notSeenTimer > 6)
            {
                seenTimer = 0;
                notSeenTimer = 0;
            }

        }


        runCooldowns();

        Patrol();


    }

    public float beingSeen()
    {
        notSeenTimer = 0;

        seenTimer += Time.deltaTime;
        return seenTimer;
    }

    public override void LoseHealth()
    {
        health -= 10;
        TrainManager.handleDamage(this); // these are specific for the attack enemy trainer, as the handle damage will only need to account for the enemy robots taking damage, not the active player
        if (health <= 0)
        {
            TrainManager.handleDeath(this); 

        }
    }

    override protected IEnumerator death()
    {
        yield return new WaitForSecondsRealtime(0);


    }


    void Patrol()
    {
        if (wayPointsDummy.Length < 2)
            return;


        navMeshAgent.destination = wayPointsDummy[nextWaypoint].position;
        navMeshAgent.isStopped = false;

        //When we get to the current waypoint, switch to the next one. When you get tot the last waypoint, go the the first and continue
        // We want to check are we at end location.
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
        {
            // Enemy is definitely is at goal the posisition
            nextWaypoint = (nextWaypoint + 1) % wayPointsDummy.Length;
        }




    }



}

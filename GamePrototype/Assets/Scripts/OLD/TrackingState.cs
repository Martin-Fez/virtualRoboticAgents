using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingState : IEnemyState
{

    // we declare a variable called enemy. it's type is statePattern enemy. it is a class.
    private StatePatternEnemy enemy;

    int nextWaypoint; // Index of the waypoint in the array

    //When we create patrol state objecct in statePattern enemy, function bellow is invoked atomatically.
    //this is the constructor. This function gets the whole StatePatternEnemy class as parameter and all the features
    // Basicaly we pass all enemy features to this script (object) 
    // the parameter (variable name) is statePatternEnemy and we assign it to local variable "enemy"
    // this means that in the future we get access to enemy's properties by writing enemy.something
    // for example enemy.searchDuration so we get the value on that.
    // constructor
    public TrackingState(StatePatternEnemy statePatternEnemy)
    {
        enemy = statePatternEnemy;


    }

    public void UpdateState()
    {
        Look();
        Track();
    }

    public void OnTriggerEnter(Collider other)
    {
        // we check what is triggering us. If it is player -> Alerts state.
        if (other.CompareTag("Player"))
        {
            ToAlertState();
        }
    }

    public void ToAlertState()
    {
        enemy.currentState = enemy.alertState;
    }

    public void ToChaseState()
    {
        enemy.currentState = enemy.chaseState;
    }

    public void ToPatrolState()
    {
        // WE cannot use this becouse we are already on the patrol state, leave it empty
    }

    public void ToTrackingState()
    {
    }


    void Look()
    {
        Debug.DrawRay(enemy.eye.position, enemy.eye.forward * enemy.sightRange, Color.magenta);

        RaycastHit hit;
        if (Physics.Raycast(enemy.eye.position, enemy.eye.forward, out hit, enemy.sightRange) && hit.collider.CompareTag("Player"))
        {
            // We go here only if the ray hits the player
            // if the ray hits player the enemy sees it and goes instantly to Chase State. And enemy hows what to follow.
            enemy.chaseTarget = hit.transform; // chaseTarget is the player
            ToChaseState();


        }

    }

    void Track()
    {
        enemy.indicator.material.color = Color.magenta;
        enemy.navMeshAgent.destination = enemy.lastKnownPlayerPostition;
        enemy.navMeshAgent.isStopped = false;

        //When we get to the current waypoint, switch to the next one. When you get tot the last waypoint, go the the first and continue
        // We want to check are we at end location.
        if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance && !enemy.navMeshAgent.pathPending)
        {
            // Enemy is definitely is at goal the posisition
            ToAlertState();
        }




    }


}

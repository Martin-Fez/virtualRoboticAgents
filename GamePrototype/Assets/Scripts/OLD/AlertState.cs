using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertState : IEnemyState
{
    private StatePatternEnemy enemy;
    float searchTimer;

    public AlertState(StatePatternEnemy statePatternEnemy)
    {
        enemy = statePatternEnemy;
    }

    public void UpdateState()
    {
        Search();
        Look();
    }

    public void OnTriggerEnter(Collider other)
    {

    }

    public void ToAlertState()
    {
        enemy.currentState = enemy.alertState;
    }

    public void ToChaseState()
    {
        searchTimer = 0;
        enemy.currentState = enemy.chaseState;
    }

    public void ToPatrolState()
    {
        searchTimer = 0;
        enemy.currentState = enemy.patrolState;
    }

    public void ToTrackingState()
    {

    }


    void Search()
    {

        //Debug.Log("Changing yellow");
        enemy.indicator.material.color = Color.yellow;

        enemy.navMeshAgent.isStopped = true;

        enemy.transform.Rotate(0, enemy.searchTurnSpeed * Time.deltaTime, 0);
        searchTimer += Time.deltaTime;
        if(searchTimer > enemy.searchDuration)
        {
            ToPatrolState();
        }


    }

    void Look()
    {
        Debug.DrawRay(enemy.eye.position, enemy.eye.forward * enemy.sightRange, Color.yellow);

        RaycastHit hit;
        if (Physics.Raycast(enemy.eye.position, enemy.eye.forward, out hit, enemy.sightRange) && hit.collider.CompareTag("Player"))
        {
            // We go here only if the ray hits the player
            // if the ray hits player the enemy sees it and goes instantly to Chase State. And enemy hows what to follow.
            enemy.chaseTarget = hit.transform; // chaseTarget is the player
            ToChaseState();


        }

    }
}

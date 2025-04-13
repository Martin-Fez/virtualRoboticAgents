using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private StatePatternEnemy enemy;

    public ChaseState(StatePatternEnemy statePatternEnemy)
    {
        enemy = statePatternEnemy;
    }

    public void UpdateState()
    {
        Chase();
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

    }

    public void ToPatrolState()
    {

    }

    public void ToTrackingState()
    {
        enemy.currentState = enemy.trackingState;
    }

    void Chase()
    {
        enemy.indicator.material.color = Color.red; // to delete
        enemy.navMeshAgent.stoppingDistance = 1;
        enemy.navMeshAgent.destination = enemy.chaseTarget.position;
        enemy.navMeshAgent.isStopped = false;
    }

    void Look()
    {
        Vector3 enemyToTarget = enemy.chaseTarget.position - enemy.eye.position;

        Debug.DrawRay(enemy.eye.position, enemyToTarget, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(enemy.eye.position, enemyToTarget, out hit, enemy.sightRange) && hit.collider.CompareTag("Player"))
        {
            // We go here only if the ray hits the player
            // if the ray hits player the enemy sees it and goes instantly to Chase State. And enemy hows what to follow.
            enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player

        }
        else // if player goes around corner, enemy does not see the player
        {

            enemy.lastKnownPlayerPostition = enemy.chaseTarget.position;
            ToTrackingState();
            //ToAlertState();
        }

    }





    // Start is called before the first frame update

}

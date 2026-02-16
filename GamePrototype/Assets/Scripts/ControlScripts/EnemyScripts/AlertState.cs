using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

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
        enemy.navMeshAgent.isStopped = false;

        searchTimer = 0;
        enemy.currentState = enemy.chaseState;
    }

    public void ToPatrolState()
    {
        enemy.navMeshAgent.isStopped = false;

        searchTimer = 0;
        enemy.currentState = enemy.patrolState;
    }

    public void ToTrackingState()
    {
        enemy.navMeshAgent.isStopped = false;

        searchTimer = 0;
        enemy.currentState = enemy.trackingState;

    }


    void Search()
    {

        enemy.indicator.material.color = Color.yellow;

        enemy.navMeshAgent.velocity = Vector3.zero;
        enemy.navMeshAgent.isStopped = true;

        if (enemy.chaseTarget)
        {

            var q = Quaternion.LookRotation(enemy.chaseTarget.position - enemy.transform.position);
            enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, q, enemy.searchTurnSpeed * Time.deltaTime);
        }
        else
        {
            enemy.transform.Rotate(0, -enemy.searchTurnSpeed * Time.deltaTime, 0);
        }



        searchTimer += Time.deltaTime;
        if(searchTimer > enemy.searchDuration)
        {
            enemy.navMeshAgent.isStopped = false;
            ToPatrolState();
        }


    }

    void Look()
    {
        Debug.DrawRay(enemy.eye.position, enemy.eye.forward * enemy.sightRange, Color.yellow);


        Quaternion myRotation1;
        Quaternion myRotation2;
        Vector3 startingDirection = enemy.eye.forward;
        Vector3 result1;
        Vector3 result2;
        RaycastHit hit;
        if (Physics.Raycast(enemy.eye.position, enemy.eye.forward, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
            {

                enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player
                ToChaseState();

                return;
            }

            if (hit.collider.CompareTag("Bullet"))
            {
                enemy.lastKnownPlayerPostition = hit.collider.gameObject.transform.position - 5f * hit.collider.gameObject.transform.forward;
                ToTrackingState();
                return;
            }


        }

        for (int i = 1; i <= enemy.numberOfRays; i++)
        {


            float angleView = (float)Math.Pow(2, i - 2);

            if (angleView > enemy.MaxViewRange)
                break;

            myRotation1 = Quaternion.AngleAxis(angleView, Vector3.up);
            myRotation2 = Quaternion.AngleAxis(-angleView, Vector3.up);
            result1 = myRotation1 * startingDirection;
            result2 = myRotation2 * startingDirection;


            Debug.DrawRay(enemy.eye.position, result1 * enemy.sightRange, Color.yellow);
            Debug.DrawRay(enemy.eye.position, result2 * enemy.sightRange, Color.yellow);


            if (Physics.Raycast(enemy.eye.position, result1, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                {
                    enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player
                    ToChaseState();
                    return;
                }

                if (hit.collider.CompareTag("Bullet"))
                {
                    enemy.lastKnownPlayerPostition = hit.collider.gameObject.transform.position - 5f * hit.collider.gameObject.transform.forward;
                    ToTrackingState();
                    return;
                }

            }


            if (Physics.Raycast(enemy.eye.position, result2, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                {
                    enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player
                    ToChaseState();
                    return;
                }

                if (hit.collider.CompareTag("Bullet"))
                {
                    enemy.lastKnownPlayerPostition = hit.collider.gameObject.transform.position - 5f * hit.collider.gameObject.transform.forward;
                    ToTrackingState();
                    return;
                }

            }



        }





    }
}

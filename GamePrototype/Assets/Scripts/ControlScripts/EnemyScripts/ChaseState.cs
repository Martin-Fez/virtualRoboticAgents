using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.XR;
using static UnityEngine.EventSystems.EventTrigger;

public class ChaseState : IEnemyState
{
    private StatePatternEnemy enemy;

    public ChaseState(StatePatternEnemy statePatternEnemy)
    {
        enemy = statePatternEnemy;
    }

    public void UpdateState()
    {
        if(!enemy.chaseTarget)
            ToAlertState();


        enemy.Shoot();
        Chase();
        Look();
        enemy.Dash();
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

            enemy.previousChaseTargetPosition = enemy.currentChaseTargetPosition;

        enemy.currentChaseTargetPosition = enemy.chaseTarget.position;

        enemy.currentTargetDirection = (enemy.currentChaseTargetPosition - enemy.previousChaseTargetPosition).normalized;

        var targetPosition = (enemy.chaseTarget.position + enemy.currentTargetDirection * 2f);
        var q = Quaternion.LookRotation(targetPosition - enemy.transform.position); 


        enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, q, enemy.searchTurnSpeed * Time.deltaTime);

        enemy.navMeshAgent.updateRotation = false;

        enemy.indicator.material.color = Color.red; // to delete
        enemy.navMeshAgent.stoppingDistance = 30;
        enemy.navMeshAgent.destination = enemy.chaseTarget.position;

        if(enemy.dodgeBullet)
        {
            enemy.navMeshAgent.stoppingDistance = 0;
            enemy.navMeshAgent.destination = enemy.transform.position + enemy.transform.right*10;
            enemy.dodgeBullet = false;
        }

        enemy.navMeshAgent.isStopped = false;
    }

    void Look()
    {
        Vector3 enemyToTarget = enemy.chaseTarget.position - enemy.eye.position;



        Debug.DrawRay(enemy.eye.position, enemy.eye.forward * enemy.sightRange, Color.red);


        Quaternion myRotation1;
        Quaternion myRotation2;
        Vector3 startingDirection = enemy.eye.forward;
        Vector3 result1;
        Vector3 result2;
        RaycastHit hit;


        if (Physics.Raycast(enemy.eye.position, enemy.eye.forward, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
        {


            if (hit.collider.CompareTag("Bullet"))
            {

                enemy.dodgeBullet = true;
                return;
            }


            if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
            {

                enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player

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


            Debug.DrawRay(enemy.eye.position, result1 * enemy.sightRange, Color.red);
            Debug.DrawRay(enemy.eye.position, result2 * enemy.sightRange, Color.red);


            if (Physics.Raycast(enemy.eye.position, result1, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Bullet") && !hit.collider.GetComponent<Ammo>().CheckOwner(enemy))
                {
                    enemy.dodgeBullet = true;
                    return;
                }

                if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                {

                    enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player

                    return;
                }



            }


            if (Physics.Raycast(enemy.eye.position, result2, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Bullet"))
                {
                    enemy.dodgeBullet = true;
                    return;
                }

                if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                {

                    enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player

                    return;
                }



            }


        }


        enemy.lastKnownPlayerPostition = enemy.chaseTarget.position;
        enemy.navMeshAgent.updateRotation = true; 
        ToTrackingState();

    }



}

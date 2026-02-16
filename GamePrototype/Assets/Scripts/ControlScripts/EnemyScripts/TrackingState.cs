using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class TrackingState : IEnemyState
{

    private StatePatternEnemy enemy;

    int nextWaypoint; // Index of the waypoint in the array



    float trackingTimer;
    float trackingTimerMax = 10;

    public TrackingState(StatePatternEnemy statePatternEnemy)
    {
        enemy = statePatternEnemy;


    }

    public void UpdateState()
    {
        Look();
        Track();
        checkTime();
    }

    void checkTime()
    {
        trackingTimer += Time.deltaTime;

        if(trackingTimer >= trackingTimerMax)
            ToAlertState();

    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
        {
            ToAlertState();
        }
    }

    public void ToAlertState()
    {
        trackingTimer = 0;
        enemy.currentState = enemy.alertState;
    }

    public void ToChaseState()
    {
        trackingTimer = 0;
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


        Quaternion myRotation1;
        Quaternion myRotation2;
        Vector3 startingDirection = enemy.eye.forward;
        Vector3 result1;
        Vector3 result2;
        RaycastHit hit;

        RaycastHit hitBullet;
        if (Physics.Raycast(enemy.eye.position, enemy.eye.forward, out hit, enemy.sightRange,9, QueryTriggerInteraction.Ignore) )
        {
            if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
            {

                enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player
                ToChaseState();

                return;
            }

            if (hit.collider.CompareTag("Bullet"))
            {
                if (Physics.Raycast(hit.collider.transform.position, hit.collider.transform.forward, out hitBullet, enemy.sightRange, 9, QueryTriggerInteraction.Ignore)
                    && hitBullet.collider.CompareTag("Player") && !enemy.dodgeBullet)
                {
                    enemy.lastKnownPlayerPostition = enemy.lastKnownPlayerPostition + 2f * hitBullet.collider.gameObject.transform.right;
                    enemy.dodgeBullet = true;
                }


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


            Debug.DrawRay(enemy.eye.position, result1 * enemy.sightRange, Color.magenta);
            Debug.DrawRay(enemy.eye.position, result2 * enemy.sightRange, Color.magenta);


            if (Physics.Raycast(enemy.eye.position, result1, out hit, enemy.sightRange, 9, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                {
                    enemy.chaseTarget = hit.transform; // Enemy makes sure the chaseTarget is Player
                    ToChaseState();
                    return;
                }

                if (hit.collider.CompareTag("Bullet") && !hit.collider.GetComponent<Ammo>().CheckOwner(enemy))
                {
                    if (Physics.Raycast(hit.collider.transform.position, hit.collider.transform.forward, out hitBullet, enemy.sightRange, 9, QueryTriggerInteraction.Ignore)
                        && hitBullet.collider.CompareTag("Player") && !enemy.dodgeBullet)
                    {
                        enemy.lastKnownPlayerPostition = enemy.lastKnownPlayerPostition + 2f * hitBullet.collider.gameObject.transform.right;
                        enemy.dodgeBullet = true;
                    }


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
                    if (Physics.Raycast(hit.collider.transform.position, hit.collider.transform.forward, out hitBullet, enemy.sightRange, 9, QueryTriggerInteraction.Ignore)
                        && hitBullet.collider.CompareTag("Player") && !enemy.dodgeBullet)
                    {
                        enemy.lastKnownPlayerPostition = enemy.lastKnownPlayerPostition + 2f * hitBullet.collider.gameObject.transform.right;
                        enemy.dodgeBullet = true;
                    }

                    return;
                }

            }




        }

        enemy.dodgeBullet = false; 


    }

    void Track()
    {



        enemy.indicator.material.color = Color.magenta;
        enemy.navMeshAgent.destination = enemy.lastKnownPlayerPostition;
        enemy.navMeshAgent.isStopped = false;

        enemy.navMeshAgent.stoppingDistance = 0;
        if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance && !enemy.navMeshAgent.pathPending)
        {
            ToAlertState();
        }




    }


}

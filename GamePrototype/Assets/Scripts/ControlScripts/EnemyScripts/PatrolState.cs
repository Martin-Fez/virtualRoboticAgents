using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.MLAgents;
using UnityEngine;

public class PatrolState : IEnemyState
{

    private StatePatternEnemy enemy;

    int nextWaypoint; // Index of the waypoint in the array


    public PatrolState(StatePatternEnemy statePatternEnemy)
    {
        enemy = statePatternEnemy;
        

    }

    public void UpdateState()
    {
        Look();
        Patrol();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
        {
            enemy.chaseTarget = other.transform; // Enemy makes sure the chaseTarget is Player
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

    }

    public void ToTrackingState()
    {
        enemy.currentState = enemy.trackingState;
    }

    void Look()
    {



        Debug.DrawRay(enemy.eye.position, enemy.eye.forward * enemy.sightRange, Color.green);


        Quaternion myRotation1;
        Quaternion myRotation2;
        Vector3 startingDirection = enemy.eye.forward;
        Vector3 result1;
        Vector3 result2;

        RaycastHit[] hits = Physics.RaycastAll(enemy.eye.position, enemy.eye.forward, enemy.sightRange);
        Array.Sort(hits,(a, b) => (a.distance.CompareTo(b.distance))); 
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if(hits[i].collider.CompareTag("Wall"))
                {
                    break;
                }


                if (hits[i].collider.CompareTag("Grid"))
                {
                    if(GameManager.manager != null)
                        enemy.gridTimer[hits[i].transform.GetComponent<GridLogic>().GridID] = GameManager.manager.FightTimer;

                    if(enemy.TrainManager != null)
                    {
                        enemy.gridTimer[hits[i].transform.GetComponent<GridLogic>().GridID] = enemy.TrainManager.InternalTimer;
                    }



                }

                if (hits[i].collider.CompareTag("Player") && hits[i].collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                {

                    enemy.chaseTarget = hits[i].transform; // Enemy makes sure the chaseTarget is Player
                    ToChaseState();
                    return;

                }

                if (hits[i].collider.CompareTag("Bullet"))
                {
                    enemy.lastKnownPlayerPostition = hits[i].collider.gameObject.transform.position - 5f*hits[i].collider.gameObject.transform.forward;
                    ToTrackingState();
                    return;

                }
            }
        }


        

        for (int rayIndex = 1; rayIndex <= enemy.numberOfRays; rayIndex++)
        {


            float angleView = (float)Math.Pow(2, rayIndex - 2);

            if (angleView > enemy.MaxViewRange)
                break;

            myRotation1 = Quaternion.AngleAxis(angleView, Vector3.up);
            myRotation2 = Quaternion.AngleAxis(-angleView, Vector3.up);
            result1 = myRotation1 * startingDirection;
            result2 = myRotation2 * startingDirection;

            Debug.DrawRay(enemy.eye.position, result1 * enemy.sightRange, Color.green);
            Debug.DrawRay(enemy.eye.position, result2 * enemy.sightRange, Color.green);


            hits = Physics.RaycastAll(enemy.eye.position, result1, enemy.sightRange);
            Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.CompareTag("Wall"))
                    {
                        break;
                    }


                    if (hits[i].collider.CompareTag("Grid"))
                    {
                        if (GameManager.manager != null)
                            enemy.gridTimer[hits[i].transform.GetComponent<GridLogic>().GridID] = GameManager.manager.FightTimer;

                        if (enemy.TrainManager != null)
                        {
                            enemy.gridTimer[hits[i].transform.GetComponent<GridLogic>().GridID] = enemy.TrainManager.InternalTimer;
                        }

                    }

                    if (hits[i].collider.CompareTag("Player") && hits[i].collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                    {
                        enemy.chaseTarget = hits[i].transform; // Enemy makes sure the chaseTarget is Player
                        ToChaseState();
                        return;

                    }

                    if (hits[i].collider.CompareTag("Bullet"))
                    {
                        enemy.lastKnownPlayerPostition = hits[i].collider.gameObject.transform.position - 5f * hits[i].collider.gameObject.transform.forward;
                        ToTrackingState();
                        return;
                    }
                }
            }

            hits = Physics.RaycastAll(enemy.eye.position, result2, enemy.sightRange);
            Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.CompareTag("Wall"))
                    {
                        break;
                    }


                    if (hits[i].collider.CompareTag("Grid"))
                    {
                        if (GameManager.manager != null)
                            enemy.gridTimer[hits[i].transform.GetComponent<GridLogic>().GridID] = GameManager.manager.FightTimer;

                        if (enemy.TrainManager != null)
                        {
                            enemy.gridTimer[hits[i].transform.GetComponent<GridLogic>().GridID] = enemy.TrainManager.InternalTimer;
                        }

                    }

                    if (hits[i].collider.CompareTag("Player") && hits[i].collider.TryGetComponent(out ControllerParent otherAgent) && otherAgent.teamID != enemy.teamID && !otherAgent.dead)
                    {

                    enemy.chaseTarget = hits[i].transform; // Enemy makes sure the chaseTarget is Player
                    ToChaseState();
                    return;

                    }

                    if (hits[i].collider.CompareTag("Bullet"))
                    {
                        enemy.lastKnownPlayerPostition = hits[i].collider.gameObject.transform.position - 5f * hits[i].collider.gameObject.transform.forward;
                        ToTrackingState();
                        return;
                    }
                }
            }



        }










    }

    void Patrol()
    {


        enemy.indicator.material.color = Color.green;
        enemy.currentGridIndex = enemy.FindNextGrid();

        enemy.navMeshAgent.destination = enemy.gridPoints[enemy.currentGridIndex].position;

        if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance && !enemy.navMeshAgent.pathPending)
        {
            if (GameManager.manager != null)
                enemy.gridTimer[enemy.currentGridIndex] = GameManager.manager.FightTimer;

            if (enemy.TrainManager != null)
            {
                enemy.gridTimer[enemy.currentGridIndex] = enemy.TrainManager.InternalTimer;
            }



        }




    }


}

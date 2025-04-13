using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyState 
{
    // Put here list of methods for the contract

    void UpdateState();

    void OnTriggerEnter(Collider other);


    void ToPatrolState();

    void ToAlertState();

    void ToChaseState();

    void ToTrackingState();






}

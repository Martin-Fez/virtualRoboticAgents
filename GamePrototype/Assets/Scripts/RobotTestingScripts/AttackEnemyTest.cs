using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyTest : MonoBehaviour
{



    public GameObject SubjectRobot;

    public GameObject TargetToAttack;



    float startHp;


    void Start()
    {
        startHp = TargetToAttack.GetComponent<PlayerControlNetwork>().health;
        startHp = 100; 
        

    }

    void Update()
    {

        float Health = TargetToAttack.GetComponent<PlayerControlNetwork>().health;


        if (startHp != Health)
        {
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));
            
            Application.Quit();
            Time.timeScale = 0;
        }



    }





}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindEnemyTest : MonoBehaviour
{



    public GameObject SubjectRobot;

    public Transform TargetToFind;




    void Start()
    {
        


    }

    void Update()
    {

        var chaseTarget = SubjectRobot.GetComponent<StatePatternEnemy>().chaseTarget;


        if (chaseTarget == TargetToFind)
        {
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));
            
            Application.Quit();
            Time.timeScale = 0;
        }



    }





}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyTest : MonoBehaviour
{



    public GameObject SubjectRobot;

    public GameObject TargetToAttack;



    public float startLives;


    void Start()
    {
        startLives = TargetToAttack.GetComponent<ControllerParent>().lives;
        

    }

    void Update()
    {

        float lives = TargetToAttack.GetComponent<ControllerParent>().lives;


        if (startLives != lives)
        {
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));
            
            Application.Quit();
            Time.timeScale = 0;
        }



    }





}

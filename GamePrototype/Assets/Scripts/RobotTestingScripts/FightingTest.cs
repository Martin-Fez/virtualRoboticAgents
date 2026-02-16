using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingTest : MonoBehaviour
{

    public GameObject SubjectRobot;

    public GameObject TargetToAttack;



    public float startLives;
    public float startLivesDummy;


    void Start()
    {

        startLivesDummy = TargetToAttack.GetComponent<StatePatternEnemy>().lives;
        startLives = SubjectRobot.GetComponent<StatePatternEnemy>().lives;
    }

    void Update()
    {

        float dummylives = TargetToAttack.GetComponent<StatePatternEnemy>().lives;
        float lives = SubjectRobot.GetComponent<StatePatternEnemy>().lives;


        if (startLives != lives)
        {
            Debug.Log("Test Failed");
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));

            Application.Quit();
            Time.timeScale = 0;
        }

        if (startLivesDummy != dummylives)
        {
            Debug.Log("Test succeeded");
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));

            Application.Quit();
            Time.timeScale = 0;
        }



    }
}

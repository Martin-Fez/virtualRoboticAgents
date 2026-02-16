using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgingTest : MonoBehaviour
{

    public GameObject SubjectRobot;

    public GameObject TargetToAttack;



    public float startHealth;
    public float startLivesDummy;


    void Start()
    {

        startLivesDummy = TargetToAttack.GetComponent<StatePatternEnemy>().lives;
        startHealth = SubjectRobot.GetComponent<StatePatternEnemy>().health;


    }

    void Update()
    {

        float dummylives = TargetToAttack.GetComponent<StatePatternEnemy>().lives;
        float health = SubjectRobot.GetComponent<StatePatternEnemy>().health;


        if (startHealth != health)
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

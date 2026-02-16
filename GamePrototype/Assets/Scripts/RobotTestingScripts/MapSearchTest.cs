using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSearchTest : MonoBehaviour
{



    public GameObject SubjectRobot;
    public bool isMlAgent;
    bool testStart = false;

    public TrainerParent TrainManager;

    public int currentTestNum = 1;
    public int NumberOfTests;
    public float numOfWins = 0;
    public float numOfFails = -1; // code needs to have the agent fail atleast once
    public float ratioOfWins;

    public float timeSpeed = 1;

    public float totalScore = 0;




    void Start()
    {
        


    }

    bool CheckGridTest()
    {
        if(isMlAgent)
        {
            if (TrainManager.gridTimer.Length > 0)
                return false;
            else   
                return true;
        }
        else
        {
            return !SubjectRobot.GetComponent<StatePatternEnemy>().hasGrid;
        }


    }


    void setTimeSpeed()
    {
        Time.timeScale = timeSpeed;

    }

    void Update()
    {
        setTimeSpeed();



        if (!testStart)
        {
            if (CheckGridTest())
                return;

            testStart = true;


            


        }

        if(isMlAgent && currentTestNum < TrainManager.EpisodeNum)
        {
            totalScore += TrainManager.lastEpisodeCulmulativeReward;
            Debug.Log("Checking test results");
            if(TrainManager.lastEpisodeSuccess)
            {
                numOfWins++;
            }
            else
            {
                numOfFails++;
            }


            if (NumberOfTests < TrainManager.EpisodeNum)
            {
                timeSpeed = 0;
                Time.timeScale = 0;
                ratioOfWins = numOfWins / NumberOfTests;

                Debug.Log("Tests ended");
                Debug.Log("Test Results are: ");
                Debug.Log("Wins: " + numOfWins);
                Debug.Log("Fails: " + numOfFails);
                Debug.Log("win ratio: " + ratioOfWins);
                Debug.Log("total score: " + totalScore);
                Debug.Log("mean score: " + (totalScore/NumberOfTests));


            }

            currentTestNum++;




        }



        if (!isMlAgent && testStart && CheckGridStatus())
        {
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));
            
            Application.Quit();
            Time.timeScale = 0;
        }



    }



    bool CheckGridStatus()
    {
        float[] gridTimer = SubjectRobot.GetComponent<StatePatternEnemy>().gridTimer;
        int gridSize = gridTimer.Length;

        for (int i = 0; i < gridSize;i++)
        {
            if (gridTimer[i] == 600)
                return false;
        }





        return true;
    }



}

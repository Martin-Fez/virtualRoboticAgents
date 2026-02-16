using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigationtest : MonoBehaviour
{
    public GameObject PointA;
    public GameObject PointB;


    Vector3 APos;
    Vector3 BPos;

    public GameObject SubjectRobot;

    bool testStart = false;


    void Start()
    {
        


    }

    void Update()
    {
        if (!testStart)
        {
            testStart = true;


            var Ypos = SubjectRobot.transform.position.y;
            APos = PointA.transform.position;
            BPos = PointB.transform.position;
            APos.y = Ypos;
            BPos.y = Ypos;


            SubjectRobot.transform.position = APos;

            


        }

        if(Vector3.Distance(SubjectRobot.transform.position, BPos) < 5)
        {
            Debug.Log("Time taken: " + (600 - GameManager.manager.FightTimer));
            
            Application.Quit();
            Time.timeScale = 0;
        }



    }
}

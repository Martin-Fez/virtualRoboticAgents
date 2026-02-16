using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using System.Threading;
using System.Security;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{

    public static GameManager manager;

    public TMP_Text TimerField;

    public GameObject UIPanel;
    public float FightTimer = 0;



    private void Awake()
    {
        if (manager == null)
        {
            DontDestroyOnLoad(gameObject);
            manager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

    }

   
    void Update()
    {
        MatchTimer();

    }


    void MatchTimer()
    {
        FightTimer -= Time.deltaTime;

        float minutes = Mathf.FloorToInt(FightTimer / 60);
        float seconds = Mathf.FloorToInt(FightTimer % 60);


        TimerField.text = "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);


        if (FightTimer >= 600)
        {
            Debug.Log("Time Ran out");
        }

    }



}










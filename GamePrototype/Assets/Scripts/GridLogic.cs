using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLogic : MonoBehaviour
{
    public int GridID;

    void Start()
    {
        
    }

    void Update()
    {
       // Debug.Log(timer);
    }

    public void OnTriggerEnter(Collider other)
    {
        // we check what is triggering us. If it is player -> Alerts state.
        /*
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered");
            //ToAlertState();
        }
        else 
        {
            Debug.Log("We were seen");
        }
        */

    }

    public void WasSeen()
    {
        //Debug.Log(timer);
        //timer = 0;
    }

}

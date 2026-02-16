using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    public GameObject Owner;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("Vision") && !collision.gameObject.CompareTag("Grid"))
        {
            Destroy(gameObject);
        }
    }


    // if owner return true, else false
    public bool CheckOwner(ControllerParent other)
    {
        if(other == Owner)
            return true;

        return false;
    }

}

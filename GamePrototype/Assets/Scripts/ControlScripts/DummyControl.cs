using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyControl : StatePatternEnemy
{
    public GameObject Target;
    public bool RotationAllowed;
    public GameObject agent;


    void Start()
    {
        
    }

    void Update()
    {





        runCooldowns();

        if (!Target)
        {
            //Debug.Log("Lacking target");
            return;
        }



        if(RotationAllowed)
        {
            var q = Quaternion.LookRotation(Target.transform.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, searchTurnSpeed * Time.deltaTime);
        }


        Shoot();
    }


    


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject == agent && !agent.GetComponent<ControllerAgent>().dead) // checks if it is even player
        {
            Target = other.gameObject;
        }
    }

    public override void LoseHealth()
    {
        health -= 10;
        TrainManager.handleDamage(this); // these are specific for the attack enemy trainer, as the handle damage will only need to account for the enemy robots taking damage, not the active player
        if (health <= 0)
        {
            TrainManager.handleDeath(this); 

        }
    }

    override protected IEnumerator death()
    {
        yield return new WaitForSecondsRealtime(0);


    }


}

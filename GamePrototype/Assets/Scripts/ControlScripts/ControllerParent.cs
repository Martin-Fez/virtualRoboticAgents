using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public abstract class ControllerParent : Agent
{



    public bool dead = false;

    public int teamID = 1;



    protected float targetRotationDirection;

    protected bool Rotating; // Whether we are rotating or not
    protected bool RotateClockWise; // Whether the rotation is right ie clockwise or left ie counter clockwise
    





    public float shootCooldownMax;
    protected float shootCooldown;

    public int numberOfRays;
    public float MaxViewRange;



    public GameObject ammo;
    public GameObject ammoSpawn;
    public float BulletLifeSpan;

    public float bulletSpeed;

    public float health;
    public float lives;




    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("Collecting data");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(actions.ContinuousActions[0]);
        //Debug.Log(actions.DiscreteActions[0]);
        //Debug.Log("sending action");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        return;
    }

    public void Shoot()
    {


        if (shootCooldown <= 0)
        {
            GameObject ammoInstance = Instantiate(ammo, ammoSpawn.transform.position, Quaternion.identity);
            ammoInstance.GetComponent<Rigidbody>().AddForce(ammoSpawn.transform.forward * bulletSpeed, ForceMode.Impulse);
            ammoInstance.GetComponent<Ammo>().Owner = gameObject;
            ammoInstance.transform.rotation = gameObject.transform.rotation;
            Destroy(ammoInstance, BulletLifeSpan);
            shootCooldown = shootCooldownMax;
        }

    }


    public void runCooldowns()
    {
        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }    
    }

    public abstract void LoseHealth();



    protected abstract IEnumerator death();

    public virtual void handleSpecialCollision(Collision other)
    {
        return;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            if (collision.gameObject.GetComponent<Ammo>().Owner == gameObject) // Check if we are the owner of the bullet, might be better to do though ids
                return;

            Destroy(collision.gameObject);
            LoseHealth();

        }

        handleSpecialCollision(collision);

    }
    




    //need to add dash common dash function






}

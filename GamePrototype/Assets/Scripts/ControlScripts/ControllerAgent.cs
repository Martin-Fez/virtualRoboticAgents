using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
//using Unity.Sentis;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;


public class ControllerAgent : ControllerParent
{
    private Rigidbody rb;

    bool UpdateMovement;
    public Vector3 startPostion;

    public bool enableExtraObservations = true;

    Vector3 TargetPositionTest;

    private float previousDistance;

    public Material winMaterial;
    public Material loseMaterial;
    public MeshRenderer FloorMeshRenderer;
    public CharacterController controller;

    public Camera agentCamera;
    public bool IsCameraBased = false;

    Vector3 movementVector;
    public float maxMoveSpeed;
    public float currentMaxMoveSpeed;
    public float dashSpeed;
    public float acceleration;
    public float deacceleration;
    public float rotateSpeed;

    public float currentSpeed = 0;
    bool LoseSpeed;





    float moveXAction;
    float moveZAction;

    float rotateAction;

    bool VerticalMovement;
    bool HorizontalMovement;
    bool DashAction;
    bool ShootAction;

    int rotateDirectionAction; // could be removed in favor of Rotate being in the 0-2 range and then pushed to -1 to 1 range // Could also have 3 variables left,forward, right



    public Transform eye; // this is the eye, we will send raycasts from here
    public float sightRange; // how far does the enemy see. This is distance of the raycast








    public TrainerParent TrainManager;
    public float FixedYSpawnBot;
    public float FixedYSpawnGoal;

    public float currentReward=0;


    public bool dashed = false;
    float dashCooldown = 0;
    public float maxDashCooldown;
    float dashTime = 0.5f;


    public float OuterWallPenalty = -0.1f;
    public float WallPenalty = 0f;




    bool CollisionHappened = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        CharacterController controller = GetComponent<CharacterController>();
        UpdateMovement = true;
        agentCamera.enabled = true;

    }


    
    void Update()
    {

        if(!agentCamera.isActiveAndEnabled)
        {
            agentCamera.enabled = true;
        }

        RequestDecision(); // ml agent thing

        currentReward = GetCumulativeReward();

        runCooldowns();


        if (UpdateMovement)
        {



            GetBodyMovement();

            TryDash();



            CalculateSpeed();

            Vector3 relativeMovement = transform.TransformDirection(movementVector);

            Vector3 move = relativeMovement * currentSpeed * Time.deltaTime;

            controller.Move(move);

            float mouseInput = rotateAction * rotateSpeed * Time.deltaTime;

            Vector3 lookHere = new Vector3(0, mouseInput, 0);


            transform.Rotate(lookHere);


            if(ShootAction)
            {
                Shoot();
            }

            returnToBounds();



        }



    }
    

    public void returnToBounds()
    {
        Vector3 relativePos = FloorMeshRenderer.transform.InverseTransformPoint(transform.position);
        Vector3 groundCubePosition = FloorMeshRenderer.transform.position;


        if(transform.position.y > 3 || transform.position.y < 3)
        {
            transform.position = new Vector3(transform.position.x, 3, transform.position.z);
        }

        if (relativePos.x > 0.5)
            transform.position = new Vector3(groundCubePosition.x+49, transform.position.y, transform.position.z);

        if (relativePos.x < -0.5)
            transform.position = new Vector3(groundCubePosition.x - 49, transform.position.y, transform.position.z);

        if (relativePos.z > 0.5)
            transform.position = new Vector3(transform.position.x, startPostion.y, groundCubePosition.z + 49);

        if (relativePos.z < -0.5)
            transform.position = new Vector3(transform.position.x, startPostion.y, groundCubePosition.z - 49);




    }


    public override void OnEpisodeBegin()
    {


        TrainManager.EpisodeEnded(); // Delete stuff if needed 

        

        agentCamera.enabled = true;
        health = 100; 
        lives = 3;

        TrainManager.EpisodeNum += 1;


        transform.position = startPostion;

        if(!TrainManager.manualSpawnAgent)
            transform.localPosition = new Vector3(UnityEngine.Random.Range(-45f, +45f), startPostion.y, UnityEngine.Random.Range(-45f, 45f));




        TrainManager.BuildEpisode(gameObject); 

        TrainManager.CheckCounts();


    }





    public override void CollectObservations(VectorSensor sensor)
    {


        float yaw = transform.eulerAngles.y * Mathf.Deg2Rad;
        sensor.AddObservation(Mathf.Sin(yaw));
        sensor.AddObservation(Mathf.Cos(yaw));

        Vector3 relativePos = FloorMeshRenderer.transform.InverseTransformPoint(transform.position);
        sensor.AddObservation(relativePos.x); // need only 2 parts of the for the position, height does not matter
        sensor.AddObservation(relativePos.z);

        Look(sensor);// have look take place here when collecting observation


        sensor.AddObservation(shootCooldown);
        sensor.AddObservation(currentSpeed);


        sensor.AddObservation(TrainManager.InternalTimer);


        if(enableExtraObservations)
        { 
            sensor.AddObservation(dashCooldown);
            sensor.AddObservation(health);
            sensor.AddObservation(lives);
            sensor.AddObservation(CollisionHappened);
            CollisionHappened = false;
        }



    }

    public override void OnActionReceived(ActionBuffers actions)
    {



        moveXAction = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        moveZAction = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        rotateAction = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        VerticalMovement = (actions.DiscreteActions[0] == 1);
        HorizontalMovement = (actions.DiscreteActions[1] == 1);

        DashAction = (actions.DiscreteActions[2] == 1);
        ShootAction = (actions.DiscreteActions[3] == 1);


    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continousActions[0] = Input.GetAxis("Horizontal");
        continousActions[1] = Input.GetAxis("Vertical");
        continousActions[2] = Input.GetAxis("Mouse X");



        discreteActions[0] = Input.GetButton("Vertical") ? 1 : 0;
        discreteActions[1] = Input.GetButton("Horizontal") ? 1 : 0;

        discreteActions[2] = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;
        discreteActions[3] = Input.GetButtonDown("Fire1") ? 1 : 0;




        return;
    }



    override protected IEnumerator death()
    {

        dead = true;
        lives -= 1;
        UpdateMovement = false;
        health = 100;
        gameObject.transform.position = startPostion;
        var render = GetComponent<Renderer>();
        render.enabled = false;

        yield return new WaitForSecondsRealtime(3);



        dead = false;
        render.enabled = true;


        UpdateMovement = true;



    }

    private void GetBodyMovement()
    {


        movementVector.x = moveXAction;
        movementVector.z = moveZAction; 
        movementVector.Normalize();
    }

    private void CalculateSpeed()
    {

        if ((VerticalMovement || HorizontalMovement) && !LoseSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= deacceleration * Time.deltaTime;

        }
        currentSpeed = Mathf.Clamp(currentSpeed, 0, currentMaxMoveSpeed);

    }


    void Look(VectorSensor sensor) // number of inputs will be based on number of rays, each ray returning type and position atleast
    {


        Debug.DrawRay(eye.position, eye.forward * sightRange, Color.green);


        Quaternion myRotation1;
        Quaternion myRotation2;
        Vector3 startingDirection = eye.forward;
        Vector3 result1;
        Vector3 result2;

        bool HasHitInfo = false;


        RaycastHit[] hits = Physics.RaycastAll(eye.position, eye.forward, sightRange);
        Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
        CheckHits(hits, sensor, HasHitInfo);




        for (int rayIndex = 1; rayIndex <= numberOfRays; rayIndex++)
        {


            float angleView = (float)Math.Pow(2, rayIndex - 2);

            if (angleView > MaxViewRange)
                break;

            myRotation1 = Quaternion.AngleAxis(angleView, Vector3.up);
            myRotation2 = Quaternion.AngleAxis(-angleView, Vector3.up);
            result1 = myRotation1 * startingDirection;
            result2 = myRotation2 * startingDirection;



            Debug.DrawRay(eye.position, result1 * sightRange, Color.green);
            Debug.DrawRay(eye.position, result2 * sightRange, Color.green);



            // ALL THE STUFF WRITTEN ABOVE APPLIES HERE TOO
            hits = Physics.RaycastAll(eye.position, result1, sightRange);
            Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
            CheckHits(hits, sensor, HasHitInfo);


            // other half of the rays
            hits = Physics.RaycastAll(eye.position, result2, sightRange);
            Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));
            CheckHits(hits, sensor, HasHitInfo);


        }


    }



    private void CheckHits(RaycastHit[] hits, VectorSensor sensor, bool HasHitInfo) 
    {
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.CompareTag("Wall"))
                {
                    CalculateObservations(sensor, hits[i].collider, (float)sensorObjType.wall);

                    HasHitInfo = true;
                    break;
                }


                if (hits[i].collider.CompareTag("Grid"))
                {
                    if (TrainManager.hasGrid)
                    {
                        int GridID = hits[i].transform.GetComponent<GridLogic>().GridID;

                        TrainManager.handleGrid(GridID);

                        TrainManager.gridTimer[GridID] = TrainManager.InternalTimer;

                    }

                }

                if (hits[i].collider.CompareTag("Player") && hits[i].collider != this)
                {


                    CalculateObservations(sensor, hits[i].collider, (float)sensorObjType.player);

                    HasHitInfo = true;


                    if (hits[i].collider.gameObject)
                    {
                        TrainManager.handleEnemyPlayer(hits[i].collider.gameObject);
                    }
                    else
                        Debug.Log("Enemy player issue");


                    break;

                }

                if (hits[i].collider.CompareTag("Bullet"))
                {


                    CalculateObservations(sensor, hits[i].collider, (float)sensorObjType.bullet);

                    HasHitInfo = true;
                    break;
                }


                if (hits[i].collider.CompareTag("OuterWall"))
                {

                    CalculateObservations(sensor, hits[i].collider, (float)sensorObjType.outerWall);

                    HasHitInfo = true;
                    break;
                }
            }
        }


        if (!HasHitInfo) // ELSE IF WE DO NOT GET A HIT
        {
            HandOverObservations(sensor, (float)sensorObjType.none, 0, 0, 0, 0, sightRange);



        }

        HasHitInfo = false;
    }


    void CalculateObservations(VectorSensor sensor, Collider Observed,float observationType)
    {


        float yaw = Observed.transform.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 relativePos = FloorMeshRenderer.transform.InverseTransformPoint(Observed.transform.position);
        float distance = Vector3.Distance(this.transform.position, Observed.gameObject.transform.position);

        HandOverObservations(sensor, observationType, Mathf.Sin(yaw), Mathf.Cos(yaw), relativePos.x, relativePos.z, distance);

    }


    void HandOverObservations(VectorSensor sensor,float observationType,float yawSin,float yawCos, float relativePosX, float relativePosZ, float distance)
    {

        if (IsCameraBased) // stops from handing over observations
            return;

        sensor.AddObservation(observationType); 


        sensor.AddObservation(yawSin);
        sensor.AddObservation(yawCos);

        sensor.AddObservation(relativePosX); 
        sensor.AddObservation(relativePosZ);
        sensor.AddObservation(distance);
    }


    private void TryDash()
    {

        if (DashAction && !dashed && movementVector.magnitude != 0)
        {
            Vector3 relativeMovement = transform.TransformDirection(movementVector);



            StartCoroutine(Dash());

            dashed = true;
            LoseSpeed = true;
            currentMaxMoveSpeed = maxMoveSpeed + dashSpeed;
        }

        if (dashed)
        {
            dashCooldown += Time.deltaTime;


            if (dashCooldown > maxDashCooldown)
            {
                dashed = false;
                dashCooldown = 0;
            }



        }

        if (currentSpeed <= maxMoveSpeed)
        {
            currentMaxMoveSpeed = maxMoveSpeed;
            LoseSpeed = false;
        }
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;

        Vector3 relativeMovement = transform.TransformDirection(movementVector);

        while (Time.time < startTime + dashTime)
        {
            controller.Move(relativeMovement * dashSpeed * Time.deltaTime);

            yield return null;
        }
    }

    public override void LoseHealth()
    {
        if (dead)
            return;


        health -= 10;

        TrainManager.handleDamage(this);
        if (health <= 0)
        {
            TrainManager.handleDeath(this);



            health = 100; 

            StartCoroutine(death()); // couroutine death should work here fine



        }
    }

    public override void handleSpecialCollision(Collision other)
    {
        CollisionHappened = true;


        if (other.gameObject.CompareTag("Target"))
        {
            TrainManager.handleGoalCollision(other.gameObject);
        }



        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(WallPenalty);
        }



        if (other.gameObject.CompareTag("OuterWall"))
        {

            AddReward(OuterWallPenalty); // penalty for hitting the wall

        }
        return;
    }


    private void OnTriggerEnter(Collider other)
    {



    }




}

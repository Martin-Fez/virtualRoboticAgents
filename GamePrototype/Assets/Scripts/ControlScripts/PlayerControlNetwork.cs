using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.UI.GridLayoutGroup;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System;

public class PlayerControlNetwork : ControllerParent
{
    
    public float maxMoveSpeed;
    public float currentMaxMoveSpeed;
    public float dashSpeed;
    public float acceleration;
    public float deacceleration;
    public float rotateSpeed;
    

    float currentSpeed = 0;
    Vector2 movementVector;


    bool dashed = false;
    float dashCooldown = 0;
    public float maxDashCooldown;
    bool LoseSpeed;


    public Camera myCam;


    public bool grounded;
    public Transform eye; // this is the eye, we will send raycasts from here
    public float sightRange; // how far does the enemy see. This is distance of the raycast

    [HideInInspector] public Transform Target; 

    bool showCursor = false;

    public string playerName;

    public TMP_Text healthField;
    public TMP_Text nameField;

    Vector3 startPostion;
    bool UpdateMovement;

    void Start()
    {
        playerName = "placeholder";
        nameField.text = playerName;
        startPostion = transform.position;
        UpdateMovement = true;
        currentMaxMoveSpeed = maxMoveSpeed;

        GameObject.Find("BirdViewCamera").gameObject.GetComponent<Camera>().enabled = true;
        UnityEngine.Cursor.visible = showCursor;
        UnityEngine.Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;


    }

    public override void LoseHealth()
    {
        health -= 10;
        if (health < 0)
        {
            StartCoroutine(death());
        }
    }

    void Update()
    {
        healthField.text = health.ToString(); 

        Look();

        runCooldowns();


        if (gameObject.transform.position.y < -50)
        {
            StartCoroutine(death());
        }

        if (UpdateMovement)
            {


            GetBodyMovement();


            CalculateSpeed();

            Dash();

            float xMovement = movementVector.y * currentSpeed * Time.deltaTime;
            float zMovement = movementVector.x * currentSpeed * Time.deltaTime;

            transform.Translate(xMovement, 0, zMovement);

            float mouseInput = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            Vector3 lookHere = new Vector3(0, mouseInput, 0);
            transform.Rotate(lookHere);


            if (Input.GetButtonDown("Fire1"))
            {
                Shoot(); 

            }


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                showCursor = !showCursor;
                UnityEngine.Cursor.visible = showCursor;
                UnityEngine.Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
            }



        }




    }

    private void GetBodyMovement()
    {
        movementVector.x = Input.GetAxis("Vertical");
        movementVector.y = Input.GetAxis("Horizontal");


        movementVector.Normalize();

    }

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !dashed && movementVector.magnitude != 0)
        {
            currentSpeed += dashSpeed;
            dashed = true;
            LoseSpeed = true;
            currentMaxMoveSpeed = maxMoveSpeed + dashSpeed;
        }

        if(dashed)
        {
            dashCooldown += Time.deltaTime;
            if (currentSpeed <= maxMoveSpeed)
            {
                currentMaxMoveSpeed = maxMoveSpeed;
                LoseSpeed = false;
            }


            if (dashCooldown > maxDashCooldown)
            {
                dashed = false;
                dashCooldown = 0;
            }
        }
    }

    private void CalculateSpeed()
    {

        if  ((Input.GetButton("Vertical") || Input.GetButton("Horizontal")) && !LoseSpeed )
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= deacceleration * Time.deltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0, currentMaxMoveSpeed);

    }

    override protected IEnumerator death()
    {
        Debug.Log("death is running");


        lives -= 1;
        UpdateMovement = false;
        health = 100;
        gameObject.transform.position = startPostion;
        var render = GetComponent<Renderer>();
        render.enabled = false;

        yield return new WaitForSecondsRealtime(3);

        render.enabled = true;

        Debug.Log("death has finished");
        UpdateMovement = true;



    }

    void Look()
    {

        Debug.DrawRay(eye.position, eye.forward * sightRange, Color.green);

        RaycastHit hit;
        if (Physics.Raycast(eye.position, eye.forward, out hit, sightRange))
        {
            Target = hit.transform; 

            if(hit.collider.CompareTag("Grid"))
            {
                hit.collider.GetComponent<GridLogic>().WasSeen();
            }


        }



        Quaternion myRotation1;
        Quaternion myRotation2;
        Vector3 startingDirection = eye.forward;
        Vector3 result1;
        Vector3 result2;

        for (int i = 1; i <= numberOfRays; i++)
        {

            float angleView = (float)Math.Pow(2, i - 2);

            if (angleView > MaxViewRange)
                break;

            myRotation1 = Quaternion.AngleAxis(angleView, Vector3.up);  
            myRotation2 = Quaternion.AngleAxis(-angleView, Vector3.up);
            result1 = myRotation1 * startingDirection;
            result2 = myRotation2 * startingDirection;

            Debug.DrawRay(eye.position, result1 * sightRange, Color.green);
            Debug.DrawRay(eye.position, result2 * sightRange, Color.green);


            if (Physics.Raycast(eye.position, result1, out hit, sightRange))
            {
                Target = hit.transform; 
            }

            if (Physics.Raycast(eye.position, result2, out hit, sightRange))
            {
                Target = hit.transform; 


            }

        }

    }


    private void OnCollisionStay(Collision collision)
    {
        grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        grounded = false;
    }
}

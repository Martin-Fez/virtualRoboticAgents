using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class PlayerControlNetwork : MonoBehaviourPunCallbacks
{

    public float moveSpeed;
    public float rotateSpeed;

    public Camera myCam;

    public GameObject ammo;
    public GameObject ammoSpawn;
    public bool grounded;
    public Transform eye; // this is the eye, we will send raycasts from here
    public float sightRange; // how far does the enemy see. This is distance of the raycast


    [HideInInspector] public Transform Target; // This is what we see, will need to implement multiple Target

    bool showCursor = false;

    public float health;
    public string playerName;

    public TMP_Text healthField;
    public TMP_Text nameField;

    Vector3 startPostion;
    bool UpdateMovement;

    // Start is called before the first frame update
    void Start()
    {
        // When network object is created it gets it's data in a array. We get the data from the array
        // and put it to playerName variable and also to the name field.
        object[] obj = photonView.InstantiationData;
        playerName = obj[0].ToString();
        nameField.text = playerName;
        startPostion = transform.position;
        UpdateMovement = true;



        // if the current game window is controlled by me. Then turn on the camera
        if (photonView.IsMine)
        {


            GameObject.Find("BirdViewCamera").gameObject.GetComponent<Camera>().enabled = false;
            myCam.enabled = true;
            Cursor.visible = showCursor;
            Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        }


    }

    // Update is called once per frame
    void Update()
    {
        healthField.text = health.ToString(); // Update hleaht value to health box.

        Look();

        if (gameObject.transform.position.y < -50)
        {
            Debug.Log("death");
            //death();
            StartCoroutine(death());

        }

        // We want to control only our own game object, not other players gameobjects.
        if (photonView.IsMine && UpdateMovement)
        {
            float xMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float zMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            transform.Translate(xMovement, 0, zMovement);

            float mouseInput = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            Vector3 lookHere = new Vector3(0, mouseInput, 0);
            transform.Rotate(lookHere);

            if (Input.GetButtonDown("Fire1"))
            {
                // We pressed the shoot button, let's inform other(or all) players to run Shoot function
                photonView.RPC("Shoot", RpcTarget.All);
            }

            /*
            if (Input.GetButtonDown("Jump") && grounded)
            {
                GetComponent<Rigidbody>().velocity = Vector3.up * 10;
            }
            */

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                showCursor = !showCursor;
                Cursor.visible = showCursor;
                Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
            }



        }

        // same thing
        /*
        transform.Translate(Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime, 0));
        */



    }

    [PunRPC]
    public void Shoot()
    {
        GameObject ammoInstance = Instantiate(ammo, ammoSpawn.transform.position, Quaternion.identity);
        ammoInstance.GetComponent<Rigidbody>().AddForce(ammoSpawn.transform.forward * 30, ForceMode.Impulse);
        Destroy(ammoInstance, 5);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            if (photonView.IsMine)
            {
                photonView.RPC("LoseHealth", RpcTarget.AllBuffered);
            }
            Destroy(collision.gameObject);
        }
    }

    [PunRPC]
    public void LoseHealth()
    {
        health -= 10;
        if (health < 0  && photonView.IsMine)
        {
            Debug.Log("death");
            //Destroy(gameObject);
            //death();
            StartCoroutine(death());
        }
    }

    IEnumerator death()
    {
        Debug.Log("death is running");

        UpdateMovement = false;
        health = 100;
        gameObject.transform.position = startPostion;
        //gameObject.SetActive(false);
        //myCam.enabled = true;
        var render = GetComponent<Renderer>();
        render.enabled = false;
        //Color theColorToAdjust = render.material.color;
        //theColorToAdjust.a = 0.1f;


        yield return new WaitForSecondsRealtime(3);



        //theColorToAdjust.a = 1f;

        render.enabled = true;


        //gameObject.SetActive(true);
        Debug.Log("death has finished");
        UpdateMovement = true;



    }

    // for loop function whose input is the amount rays we want to make, and how wide the range is
    // WE will then somehow store the positions, and the NN will receive the position and the name of the object
    void Look()
    {
        Vector3 Direction2 = new Vector3(0, 0, 1);
        Debug.DrawRay(eye.position, eye.forward * sightRange, Color.green);
        Debug.DrawRay(eye.position, (eye.forward+Direction2)  * sightRange, Color.green);
        Debug.DrawRay(eye.position, (eye.forward - Direction2) * sightRange, Color.green);

        RaycastHit hit;
        //if (Physics.Raycast(eye.position, eye.forward, out hit, sightRange) && hit.collider.CompareTag("Player"))
        if (Physics.Raycast(eye.position, eye.forward, out hit, sightRange))
            {
            // We go here only if the ray hits the player
            // if the ray hits player the enemy sees it and goes instantly to Chase State. And enemy hows what to follow.
            Target = hit.transform; // chaseTarget is the player
            Debug.Log(hit.transform);
            //ToChaseState();


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

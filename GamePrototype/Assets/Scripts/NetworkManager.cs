using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//using NUnit.Framework;


public class NetworkManager : MonoBehaviourPunCallbacks
{

    private List<RoomInfo> roomsList; // This is a list of rooms we get from the cloud, 
    private const string roomNamePrefix = "MyRoom"; // start of the name of every romm will Myroom..
    // myroom44335 every room name is a unique name
    public GUIStyle myStyle;
    string playerInput = "Please enter your name here:";

    // Start is called before the first frame update
    void Start()
    {
        myStyle.fontSize = 18;
        myStyle.normal.textColor = Color.red;

        roomsList = new List<RoomInfo>(); // Empty list of rooms.

        // Let's connect to the cloud
        PhotonNetwork.ConnectUsingSettings();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.IsConnectedAndReady.ToString(), myStyle);
        GUILayout.Label(PhotonNetwork.InLobby.ToString(), myStyle);
        if(PhotonNetwork.CurrentRoom != null) 
        {
            GUILayout.Label(PhotonNetwork.CurrentRoom.Name.ToString(), myStyle);
        }

        if(PhotonNetwork.InRoom == false)
        {
            playerInput = GUI.TextField(new Rect(10, 70, 200, 20), playerInput, myStyle);
            // if we are not in room, show all avaialable rooms and create room button
            if(GUI.Button(new Rect(200,100,250,100), "Create Room"))
            {
                // We create room unique name
                PhotonNetwork.CreateRoom(roomNamePrefix + System.Guid.NewGuid().ToString());
            }

            // We list all available rooms. We make list only if there is more than 0 rooms
            if(roomsList.Count > 0)
            {
                // Make a button using for each 
                int i = 0;
                foreach(RoomInfo room in roomsList) 
                {
                    if (GUI.Button(new Rect(200, 250 + 110 * i, 250, 100), "Join" + room.Name))
                    {
                        // Join the room, how to do it?
                        PhotonNetwork.JoinRoom(room.Name);
                    }
                    i++;
                }

            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // We Update local roomsList with the info that's coming from the cloud
        roomsList = roomList; // WE update our own list whenever there is updates on the cloud.
    }

    public override void OnConnectedToMaster()
    {
        // We connected to cloud, let's go to Lobby
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedRoom()
    {
        string[] plData = new string[1]; //Only one slot in the array
        plData[0] = playerInput;
        PhotonNetwork.Instantiate("PlayerBox", new Vector3(0, 0.5f, 0), Quaternion.identity, 0, plData);
    }
}

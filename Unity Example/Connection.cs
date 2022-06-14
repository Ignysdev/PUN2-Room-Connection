using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class Connection : MonoBehaviourPunCallbacks
{
    [Header("Static - CHANGE ON FIRST SCENE")]
    [SerializeField, Tooltip("Scene build index that you insert your username and connect")] int loginScene;
    [SerializeField, Tooltip("Scene build index where you enter or create a room")] int roomCreationScene;
    [SerializeField, Tooltip("Scene build index inside a room")] int joinedRoomScene;
    [SerializeField, Tooltip("Scene build index for disconnected")] int disconnectedScene;
    [SerializeField, Tooltip("Scene build index to the actual game")] int playScene;
    
    
    static int _loginScene = -1;
    static int _roomCreationScene = -1;
    static int _joinedRoomScene = -1;
    static int _disconnectedScene = -1;
    static int _playScene = -1;


    [Header("Login")]
    [SerializeField, Tooltip("MUST insert the Input Field for the username here")] InputField usernameInput;
    [SerializeField] string defaultName = "Default Name";
    [SerializeField, Tooltip("List of first names to be picked randomly. If empty, default name will be used instead")] string[] firstName;
    [SerializeField, Tooltip("List of last names to be picked randomly. If empty, default name will be used instead")] string[] lastName;

    [Header("Room Creation")]
    [SerializeField, Tooltip("MUST insert the Input Field for the roomm name here")] InputField roomInput;

    [Header("Joined Room")]

    [SerializeField, Tooltip("MUST insert the room name text here")]TMP_Text roomName;
    [SerializeField, Tooltip("The amount of playernames is the amount of text available. You can use tags instead of selecting on this array")]TMP_Text[] playerName;


    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // Set the index to a static variable
        if(_loginScene == -1) _loginScene = loginScene;
        if(_roomCreationScene == -1) _roomCreationScene = roomCreationScene;
        if(_joinedRoomScene == -1) _joinedRoomScene = joinedRoomScene;
        if(_disconnectedScene == -1) _disconnectedScene = disconnectedScene;
        if(_playScene == -1) _playScene = playScene;

        // Get all the text for the room scene. 
        if (SceneManager.GetActiveScene().buildIndex == _joinedRoomScene)
        {
            GameObject _roomName = GameObject.FindGameObjectWithTag("roomName");
            if (_roomName != null) roomName = _roomName.GetComponent<TMP_Text>();

            for (int i = 0; i < playerName.Length; i++)
            {
                GameObject _player = GameObject.FindGameObjectWithTag("playerName" + i);
                if(_player != null) playerName[i] = _player.GetComponent<TMP_Text>();
            }
        }
    }


    private void Update()
    {
        // Change all the texts to match the player in the room
        if(SceneManager.GetActiveScene().buildIndex == _joinedRoomScene && roomName)
        {
            roomName.text = PhotonNetwork.CurrentRoom.Name + ":";

            for (int i = 0; i < playerName.Length; i++)
            {
                if(PhotonNetwork.CurrentRoom.PlayerCount > i)
                {
                    playerName[i].text = PhotonNetwork.PlayerList[i].NickName;
                }
                else
                {
                    playerName[i].text = "";
                }
            }
        }
    }


    public void Connect()
    {
        // Disable button if on button
        if (TryGetComponent(out Button button))
        {
            button.interactable = false;
        }

        // Connect
        Debug.Log("Attempting Connection...");
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();

        // Set username
        string user = "";
        if (usernameInput)
        {
            if(usernameInput.text == "" || usernameInput.text.Length > 16)
            {
                if (firstName.Length > 0 && lastName.Length > 0)
                {
                    user = firstName[Random.Range(0, firstName.Length)] + " " + lastName[Random.Range(0, lastName.Length)];
                }
                else
                {
                    user = defaultName;
                }
                Debug.Log("Username: " + user);
            }
            else
            {
                user = usernameInput.text;
            }
        }
        else
        {
            if (firstName.Length > 0 && lastName.Length > 0)
            {
                user = firstName[Random.Range(0, firstName.Length)] + " " + lastName[Random.Range(0, lastName.Length)];
            }
            else
            {
                user = defaultName;
            }
            Debug.Log("Username: " + user);
        }
        PhotonNetwork.LocalPlayer.NickName = user;
    }


    public void CreateRoom(string maxPlayers = "4")
    {
        // Get max players
        int max = 4;

        if(int.TryParse(maxPlayers, out int m))
        {
            max = m;
        }
        

        // Disable button if on button
        if (TryGetComponent(out Button button))
        {
            button.interactable = false;
        }

        // Set room name
        string roomName = "";
        if(roomInput.text == "" || roomInput.text.Length > 16)
        {

            roomName = RandomCode(6);
            Debug.Log("Room Name: " + roomName);
        }
        else
        {
            roomName = roomInput.text;
        }
        
        // Creates room
        if(PhotonNetwork.CreateRoom(roomName, new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = System.Convert.ToByte(max), PublishUserId = true }))
        {

        }
        else
        {
            PhotonNetwork.CreateRoom(RandomCode(6), new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = System.Convert.ToByte(max), PublishUserId = true });
        }
    }


    public void EnterRoom()
    {
        // Works only if room input isn't blank
        if(roomInput.text != "")
        {
            // Disable button if on button
            if (TryGetComponent(out Button button))
            {
                button.interactable = false;
            }
            
            // Join room
            if (PhotonNetwork.JoinRoom(roomInput.text))
            {

            }
            else
            {
                // Error catch
                Debug.LogWarning("This room does not exist.");
                if (TryGetComponent(out Button button1))
                {
                    button1.interactable = true;
                }
            } 
        }
        else
        {
            // Error catch
            Debug.LogWarning("Text field empty");
        }
    }


    public void Play()
    {
        // Enter play scene
        PhotonNetwork.LoadLevel(_playScene);
    }


    public void LeaveRoom()
    {
        // Leave room
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(_roomCreationScene);

    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // If error ocurred entering room, debug
        Debug.LogWarning(message);
        if (TryGetComponent(out Button button))
        {
            button.interactable = true;
        }
    }

    public override void OnConnectedToMaster()
    {
        // On connection, enter lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // On disconnectiuon, debug
        Debug.LogWarning(cause);
        SceneManager.LoadScene(_disconnectedScene);
    }

    public override void OnJoinedLobby()
    {
        // On joined lobby, load room creation scene
        SceneManager.LoadScene(_roomCreationScene);
    }

    public override void OnJoinedRoom()
    {
        // On joined room, load room
        SceneManager.LoadScene(_joinedRoomScene);
    }


    // Create random string.
    string RandomCode(int lenght)
    {
        string result = "";
        
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        for (int i = 0; i < lenght; i++)
        {
            result += characters[Random.Range(0, characters.Length)];
        }
        return result;
        
    }


}

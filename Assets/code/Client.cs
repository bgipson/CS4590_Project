using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour {

    short MSG_NEW_HUMAN = 1000;
    short MSG_POSITION = 100;

    NetworkClient client;
    public Text clientText;
    
    public int xPosZombie = -1000;
    public int zPosZombie = 60;

    // need handles on these for events
    GameObject enemyText;
    GameObject connectToServer;
    GameObject exit;
    GameObject toggleMouse;
    public bool pauseMouse = false;

    Player player;
    PlayerInfo playerInfo;

    public Client() // construct before start()
    {
        // avoid:
        // called 2x in scene start
        // called 0x times when started from menu
    }

    // Use this for initialization
    void Start() {
        client = new NetworkClient();
        clientText.text = "STATUS: NOT CONNECTED";
        player = FindObjectOfType<Player>(); // local to this particular Client
        playerInfo = new PlayerInfo();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        client.RegisterHandler(MSG_POSITION, onReceivePosition);
        client.RegisterHandler(MSG_NEW_HUMAN, onReceiveID);
        enemyText = GameObject.Find("enemyText");
        connectToServer = GameObject.Find("ConnectToServer");
        exit = GameObject.Find("Exit");
        toggleMouse = GameObject.Find("pauseText");
        pauseMouse = false;

        EnableClientDisplays();
    }

    public PlayerInfo getPlayerInfo()
    {
        return playerInfo;
    }

    public void OnConnected(NetworkMessage msg) {
        clientText.text = "STATUS: CONNECTED";
    }

    public void OnDisconnected(NetworkMessage msg)
    {
        // Disconnected by server
        clientText.text = "STATUS: NOT CONNECTED";
        playerInfo.initialized = false;
        player.ResetSettings(); // handles playerInfo too
    }

    // Update is called once per frame
    void Update () {

        if (playerInfo == null)
        {
            return;
        }

        if (playerInfo.initialized)
        {
            // send updated position to server
            client.Send(playerInfo.getID(), playerInfo);
        } else
        {
            // Might take a few update cycles for server to acknowledge
            //print("here not connected");
        }

        HandleKeyboard();
    }

    void HandleKeyboard()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (playerInfo.initialized)
            {
                returnToMenu();
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playerInfo.initialized)
            {
                pauseMouse = !pauseMouse;
                if (pauseMouse)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }
    }

    void connect() {
        // @TODO: Only connect once determined zombie/human
        if (playerInfo.initialized)
        {
            return;
        }
        DisableClientDisplays();
        client.Connect("127.0.0.1", 1234);
    }

    void onReceiveID(NetworkMessage msg)
    {
        if (!playerInfo.initialized)
        {
            NetworkMess details = msg.ReadMessage<NetworkMess>();
            playerInfo.setID(short.Parse(details.messageContents));
            playerInfo.setMessage("New Player");
            client.Send(playerInfo.getID(), playerInfo);
            playerInfo.initialized = true;
        }
    }

    void onReceivePosition(NetworkMessage msg) {
        NetworkMess details = msg.ReadMessage<NetworkMess>();
        string[] pos = details.messageContents.Split(' ');
        zPosZombie = int.Parse(pos[0]);
        xPosZombie = int.Parse(pos[1]);
        Vector2 position = new Vector2(zPosZombie, xPosZombie);
        player.zombieUpdate(position);
        clientText.text = "NEW MESSAGE: " +  details.messageContents;
    }

    private void OnApplicationQuit()
    {
        clientText.text = "STATUS: NOT CONNECTED";
        Cursor.visible = true;
        playerInfo.setMessage("Disconnecting");
        if (playerInfo != null)
        {
            if (playerInfo.initialized)
            {
                client.Send(playerInfo.getID(), playerInfo);
            }
            playerInfo.initialized = false;
        }
    }

    public void DisableClientDisplays()
    {
        pauseMouse = true;
        enemyText.SetActive(true);
        connectToServer.SetActive(false);
        //exit.SetActive(false);
        toggleMouse.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void EnableClientDisplays()
    {
        pauseMouse = false;
        enemyText.SetActive(false);
        connectToServer.SetActive(true);
        //exit.SetActive(true);
        toggleMouse.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void returnToMenu() {
        pauseMouse = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
        clientText.text = "STATUS: NOT CONNECTED";
        if (playerInfo != null)
        {
            playerInfo.initialized = false;
            playerInfo.setMessage("Disconnecting");
            player.ResetSettings(); // handles playerInfo too
            if (playerInfo.getID() != 0)
            {
                client.Send(playerInfo.getID(), playerInfo);
            }
        }
    }
}

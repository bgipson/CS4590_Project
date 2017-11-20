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
    GameObject pauseText;
    GameObject ipAddress;
    GameObject ipAddressLabel;
    GameObject ipAddressTextObject;
    public bool pauseMouse = false;

    Player player;
    PlayerInfo playerInfo;

    public Client() // construct before start()
    {
        // avoid:
        // called 2x in scene start
        // called 0x times when started from menu
    }

    // Use this for initialization, called each time the scene is loaded
    void Start() {
        client = new NetworkClient();
        clientText.text = "STATUS: NOT CONNECTED";
        player = FindObjectOfType<Player>(); // local to this particular Client
        playerInfo = new PlayerInfo();
        playerInfo.Init();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        client.RegisterHandler(MSG_POSITION, onReceivePosition);
        client.RegisterHandler(MSG_NEW_HUMAN, onReceiveID);
        enemyText = GameObject.Find("enemyText");
        connectToServer = GameObject.Find("ConnectToServer");
        pauseText = GameObject.Find("pauseText");
        ipAddressTextObject = GameObject.Find("ipAddressText");
        ipAddress = GameObject.Find("ipAddress");
        ipAddressLabel = GameObject.Find("ipAddressLabel");
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
        playerInfo.setInitialized(false);
        player.ResetSettings(); // handles playerInfo too
    }

    // Update is called once per frame
    void Update () {

        if (playerInfo == null)
        {
            return;
        }

        if (playerInfo.isInitialized())
        {
            if (playerInfo.getMessage().Equals("Disconnecting"))
            {
                return;
            }
            // send updated position to server
            CopyPlayerDataToInfo();
            client.Send(playerInfo.getID(), playerInfo);
        } else
        {
            if (!playerInfo.getMessage().Equals("not initialized"))
            {
                playerInfo.setMessage("not initialized");
            }
            // Might take a few update cycles for server to acknowledge
            //print("here not connected");
        }

        HandleKeyboard();
    }

    void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerInfo.isInitialized())
            {
                returnToMenu();
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playerInfo.isInitialized())
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
        if (playerInfo.isInitialized())
        {
            return;
        }
        Text ipAddress = ipAddressTextObject.GetComponent<Text>();
        bool isValid = CheckIPAddress(ipAddress.text);
        if (isValid)
        {
            playerInfo.setMessage("Connecting"); // can get stuck based on threading from a last session.
            client.Connect(ipAddress.text, 1234);
        }
    }

    bool CheckIPAddress(string text)
    {
        string[] parsed = text.Split('.');
        if (parsed.GetLength(0) != 4)
        {
            return false;
        }
        for (int i=0; i<4; i++)
        {
            int temp;
            if (int.TryParse(parsed[i], out temp))
            {
                if (temp < 0 || temp > 255)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    bool CheckClientReady()
    {
        if (this == null)
        {
            return false;
        }
        return true;
    }

    void onReceiveID(NetworkMessage msg)
    {
        if (!CheckClientReady())
        {
            return;
        }
        if (!playerInfo.isInitialized())
        {
            NetworkMess details = msg.ReadMessage<NetworkMess>();
            playerInfo.setID(short.Parse(details.messageContents));
            playerInfo.setMessage(""); // clear message
            client.Send(playerInfo.getID(), playerInfo);
            playerInfo.setInitialized(true);
            DisableClientDisplays();
        }
    }

    void onReceivePosition(NetworkMessage msg) {
        if (!CheckClientReady())
        {
            return;
        }
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
            if (playerInfo.isInitialized())
            {
                client.Send(playerInfo.getID(), playerInfo);
            }
            playerInfo.setInitialized(false);
        }
    }

    public void DisableClientDisplays()
    {
        pauseMouse = true;
        enemyText.SetActive(true);
        connectToServer.SetActive(false);
        ipAddress.SetActive(false);
        ipAddressLabel.SetActive(false);
        pauseText.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void EnableClientDisplays()
    {
        pauseMouse = false;
        enemyText.SetActive(false);
        connectToServer.SetActive(true);
        ipAddress.SetActive(true);
        ipAddressLabel.SetActive(true);
        pauseText.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CopyPlayerDataToInfo()
    {
        playerInfo.position.x = player.position.x;
        playerInfo.position.y = player.position.y;
        playerInfo.position.z = player.position.z;
    }

    public void returnToMenu() {
        pauseMouse = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        clientText.text = "STATUS: NOT CONNECTED";
        if (playerInfo != null)
        {
            playerInfo.setInitialized(false);
            playerInfo.setMessage("Disconnecting");
            player.ResetSettings(); // handles playerInfo too
            if (playerInfo.getID() != 0)
            {
                client.Send(playerInfo.getID(), playerInfo);
            }
        }
        SceneManager.LoadScene("MainMenu");
    }
}

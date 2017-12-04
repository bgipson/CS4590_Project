﻿using System.Collections;
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

    float timeBetweenGrowls = 5.0f;
    float lastGrowl = 0.0f;

    // need handles on these for events
    GameObject enemyText;
    GameObject connectToServer;
    GameObject pauseText;
    GameObject ipAddress;
    GameObject ipAddressLabel;
    GameObject ipAddressTextObject;
    public bool pauseMouse = false;

    // player
    Player player;
    PlayerInfo playerInfo;

    // NPCs
    GameObject zombiePath;

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

        zombiePath = GameObject.Find("zombiePath");

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

            // handle audio
            if (lastGrowl + timeBetweenGrowls < Time.time)
            {
                //print("growling" + lastGrowl + "          " + Time.time);
                lastGrowl = Time.time;
                player.zombieUpdate(player.nearestPlayerOfInterest);
            }
            player.ManageBackgroundAudio();
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
        //clientText.text = "NEW MESSAGE: " +  details.messageContents;

        if (details.name.Equals("zombiePath"))
        {
            zombiePath.transform.position = details.position;
            zombiePath.transform.eulerAngles = details.rotation;
            player.nearestPlayerOfInterest = details.position;
        }
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
        if (!player.baseAmbientTrack.isPlaying)
        {
            player.baseAmbientTrack.Play();
            player.moreIntenseAmbientTrack.Play();
            player.variometer.Play();
            player.variometerBG.Play();
            player.variometerBeep.Play();
        }
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
        if (player.baseAmbientTrack.isPlaying)
        {
            player.baseAmbientTrack.Stop();
            player.moreIntenseAmbientTrack.Stop();
            player.variometer.Stop();
            player.variometerBG.Stop();
            player.variometerBeep.Stop();
        }
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

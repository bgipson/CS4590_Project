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

    public int yPos = 0;
    public int xPos = 0;

    Human human;
    HumanInfo humanInfo;

    public Client() // construct before start()
    {
        // avoid:
        // called 2x in scene start
        // called 0x times when started from menu
    }

    // Use this for initialization
    void Start() {
        client = new NetworkClient();
        clientText.text = "STATUS: STARTED, NOT CONNECTED";
        human = FindObjectOfType<Human>(); // local to this particular Client
        humanInfo = new HumanInfo();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        client.RegisterHandler(MSG_POSITION, onReceivePosition);
        client.RegisterHandler(MSG_NEW_HUMAN, onReceiveID);
    }

    public void OnConnected(NetworkMessage msg) {
        clientText.text = "STATUS: CONNECTED";
        // @TODO: Load the 3D setting
    }

    public void OnDisconnected(NetworkMessage msg)
    {
        // Disconnected by server
        clientText.text = "STATUS: NOT CONNECTED";
        humanInfo.initialized = false;
    }

    // Update is called once per frame
    void Update () {

        if (humanInfo.initialized)
        {
            if (MovementHasOccurred())
            {
                // send updated position to server
                client.Send(humanInfo.getID(), humanInfo);
            }
        } else
        {
            // Might take a few update cycles for server to acknowledge
            //print("here not connected");
        }

    }

    void connect() {
        // @TODO: Only connect once determined zombie/human
        if (humanInfo.initialized)
        {
            return;
        }
        client.Connect("127.0.0.1", 1234);
    }

    void onReceiveID(NetworkMessage msg)
    {
        if (!humanInfo.initialized)
        {
            NetworkMess details = msg.ReadMessage<NetworkMess>();
            humanInfo.setID(short.Parse(details.messageContents));
            humanInfo.setMessage("New Player");
            client.Send(humanInfo.getID(), humanInfo);
            humanInfo.initialized = true;
        }
    }

    void onReceivePosition(NetworkMessage msg) {
        NetworkMess details = msg.ReadMessage<NetworkMess>();
        string[] pos = details.messageContents.Split(' ');
        yPos = int.Parse(pos[0]);
        xPos = int.Parse(pos[1]);
        Vector2 position = new Vector2(yPos, xPos);
        human.zombieUpdate(position);
        clientText.text = "NEW MESSAGE: " +  details.messageContents;
    }

    private void OnApplicationQuit()
    {
        clientText.text = "STATUS: NOT CONNECTED";
        humanInfo.initialized = false;
        humanInfo.setMessage("Disconnecting");
        client.Send(humanInfo.getID(), humanInfo);
    }

    public void returnToMenu() {
        SceneManager.LoadScene("MainMenu");
        clientText.text = "STATUS: NOT CONNECTED";
        humanInfo.initialized = false;
        humanInfo.setMessage("Disconnecting");
        client.Send(humanInfo.getID(), humanInfo);
    }

    bool MovementHasOccurred()
    {
        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D))
        {
            return true;
        }
        return false;
    }

}

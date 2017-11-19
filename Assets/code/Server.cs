using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour {
    NetworkServer server;
    public Text serverText;
    public InputField latField;
    public InputField lonField;
    public int port = 1234;

    // Use this for initialization
    void Start() {
        serverText.text = "STATUS: NOT INITIATED";
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            setupServer();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            serverText.text = "STATUS: RESPONDING";
            sendMessage();
        }
    }

    //Initiates the Server and has it listen to the specified port
    public void setupServer() {
        NetworkServer.Listen(port);
        serverText.text = "STATUS: SERVER INITIATED";
        NetworkServer.RegisterHandler(MsgType.Connect, onConnection);
        print(NetworkServer.serverHostId);
    }

    //Handler for any connection requests
    void onConnection(NetworkMessage message) {
        serverText.text = "STATUS: CONNECTION";
    }

    public void sendMessage() {
        NetworkMess msg = new NetworkMess();
        if (latField.text == "" || lonField.text == "") {
            msg.messageContents = "N/A";
        } else {
            msg.messageContents = latField.text + " " + lonField.text;
        }
        NetworkServer.SendToAll(100, msg);
        serverText.text = "STATUS: CONNECTED";
    }

    public void returnToMenu() {
        SceneManager.LoadScene(0);
    }
}

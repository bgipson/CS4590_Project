using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour {
    NetworkClient client;
    public Text clientText;

    public int curLat = 0;
    public int curLon = 0;

    Human human;
    
	// Use this for initialization
	void Start () {
        print(Screen.width + " , " + Screen.height);
        clientText.text = "STATUS: NOT CONNECTED";
        human = FindObjectOfType<Human>();
	}

    //Initiates the client
    public void setupClient() {
        clientText.text = "STATUS: CLIENT STARTED, NOT CONNECTED";
        client = new NetworkClient();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(100, onReceive);
        print("STARTING SETUP");
    }
	
    public void OnConnected(NetworkMessage msg) {
        clientText.text = "STATUS: CONNECTED";
    }
	// Update is called once per frame
	void Update () {
        
        
	}

    public void connect() {
        client.Connect("127.0.0.1", 1234);
    }

    void onReceive(NetworkMessage msg) {
        NetworkMess details = msg.ReadMessage<NetworkMess>();
        string[] latLon = details.messageContents.Split(' ');
        curLat = int.Parse(latLon[0]);
        curLon = int.Parse(latLon[1]);
        Vector2 position = new Vector2(curLat, curLon);
        human.zombieUpdate(position);
        clientText.text = "NEW MESSAGE: " +  details.messageContents;
    }

    public void returnToMenu() {
        SceneManager.LoadScene(0);
    }

}

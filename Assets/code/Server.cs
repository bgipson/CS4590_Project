using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour {

    short SHRT_MAX = 32767;
    short MSG_NEW_HUMAN = 1000;
    short MSG_POSITION = 100;

    public Text serverText;
    public Text humansTotal;
    public InputField yField;
    public InputField xField;
    public int port = 1234;
    public float maxTimeBeforeKick = 5.0f;

    // players
    List<PlayerInfo> serverPlayers;

    // zombie walking around
    List<NPC> NPCs;

    public Server()
    {
        // avoid:
        // called 2x in scene start
        // called 0x times when started from menu
    }

    // Use this for initialization
    void Start() {
        serverPlayers = new List<PlayerInfo>();
        NPCs = new List<NPC>();
        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MsgType.Connect, onConnection);
        NetworkServer.RegisterHandler(MsgType.Disconnect, onDisnnection);

        serverText.text = "SERVER STARTED";
        humansTotal.text = "Humans Total: " + serverPlayers.Count.ToString();

        // setup NPC
        setupNPCs();
    }

    // Update is called once per frame
    void Update() {
        if (serverPlayers.Count == 0)
        {
            serverText.text = "SERVER STARTED";
        }
        // check if players have timed out
        for (int i=serverPlayers.Count-1; i>=0; i--) // must go backward when deleting
        {
            if ((serverPlayers[i].getTimeStamp() + maxTimeBeforeKick < Time.time) &&
                serverPlayers[i].getTimeStamp() > 0)
            {
                print("Player timed out: " + serverPlayers[i].getID());
                NetworkServer.UnregisterHandler(serverPlayers[i].getID());
                serverPlayers.RemoveAt(i);
            }
        }
        ControlNPCs();
        if (serverPlayers.Count > 0)
        {
            sendZombieInfo();
        }

        humansTotal.text = "Humans Total: " + serverPlayers.Count.ToString();
    }

    void setupNPCs()
    {
        // NOTE: these names must match between different scenes
        NPC temp = GameObject.Find("zombiePath").GetComponent<NPC>();
        if (temp != null)
        {
            NPCs.Add(temp);
        }
    }

    void ControlNPCs()
    {
        if (serverPlayers.Count > 0)
        {
            for (int i=0; i<NPCs.Count; i++)
            {
                NPCs[i].setCanMove(true);
            }
        } else
        {
            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].setCanMove(false);
            }
        }
    }

    //Handler for any incoming connection requests
    void onConnection(NetworkMessage message) {
        short newPlayerID = GenerageUniquePlayerId();
        serverText.text = "STATUS: CONNECTION";
        NetworkServer.RegisterHandler(newPlayerID, OnPlayerUpdate);
        PlayerInfo newPlayer = new PlayerInfo(newPlayerID);
        newPlayer.Init();
        serverPlayers.Add(newPlayer);

        // send to player their ID
        NetworkMess msg = new NetworkMess();
        msg.messageContents = newPlayerID.ToString();
        NetworkServer.SendToAll(MSG_NEW_HUMAN, msg);
        print("new player: " + newPlayerID);
    }

    void onDisnnection(NetworkMessage message)
    {
        //PlayerInfo temp = message.ReadMessage<PlayerInfo>();
    }

    void OnPlayerUpdate(NetworkMessage message)
    {
        PlayerInfo temp = message.ReadMessage<PlayerInfo>();
        int index = -1;
        if (temp.getMessage() != null) {
            if (temp.getMessage().Equals("Disconnecting"))
            {
                index = FindPlayerById(temp.getID());
                if (index != -1)
                {
                    print("player disconnecting: " + serverPlayers[index].getID());
                    NetworkServer.UnregisterHandler(temp.getID());
                    serverPlayers.RemoveAt(index);
                }
            }
        }
        index = FindPlayerById(temp.getID());
        if (index != -1)
        {
            serverPlayers[index].setTimeStamp(Time.time);
            serverPlayers[index].setPosition(temp.getPosition());
            //print("x: " + temp.getPosition().x + " z: " + temp.getPosition().z);
        }
    }

    short GenerageUniquePlayerId()
    {
        short newID = (short)Random.Range(1, SHRT_MAX);
        for (int i=0; i<serverPlayers.Count; i++)
        {
            if (serverPlayers[i].getID() == newID)
            {
                i = 0;
                newID = (short)Random.Range(1, SHRT_MAX);
            }
        }
        return newID;
    }

    public void sendZombieInfo()
    {
        NetworkMess msg = new NetworkMess();
        for (int i=0; i<NPCs.Count; i++)
        {
            msg.position = NPCs[i].getPosition();
            msg.rotation = NPCs[i].getRotation();
            msg.name = NPCs[i].getName();
            msg.messageType = MSG_POSITION;
            NetworkServer.SendToAll(msg.messageType, msg);
        }
    }

    public void returnToMenu() {
        SceneManager.LoadScene(0);
    }

    int FindPlayerById(short id)
    {
        for (int i=0; i<serverPlayers.Count; i++)
        {
            if (serverPlayers[i].getID() == id)
            {
                return i;
            }
        }
        return -1;
    }
}

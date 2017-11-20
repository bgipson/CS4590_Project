using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Controls the Title Menu
public class MenuController : MonoBehaviour {

    public MenuController()
    {

    }

    private void Start()
    {

    }

    public void loadClient() {
        SceneManager.LoadScene("ClientRoom");
    }

    public void loadServer() {
        SceneManager.LoadScene("ServerRoom");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Controls the Title Menu
public class MenuController : MonoBehaviour {
    public void loadClient() {
        SceneManager.LoadScene(1);
    }

    public void loadServer() {
        SceneManager.LoadScene(2);
    }
}

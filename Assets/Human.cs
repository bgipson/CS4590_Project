using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Human : MonoBehaviour {
    Client client;
    public Vector2 position;

    public AudioSource growl;
	// Use this for initialization
	void Start () {
        //AudioMixer mixer = growl.outputAudioMixerGroup.audioMixer;
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void zombieUpdate(Vector2 zombiePosition) {
        float distance = Vector2.Distance(position, zombiePosition);
        growl.volume = Mathf.Clamp((100 - distance) / 100, 0, 1);
        growl.Play();
        print((100 - distance) / 100);
    }
}

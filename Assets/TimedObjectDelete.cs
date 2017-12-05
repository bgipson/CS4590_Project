using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedObjectDelete : MonoBehaviour {
    AudioSource aud;
	// Use this for initialization
	void Start () {
        aud = GetComponent<AudioSource>();
        
	}

    IEnumerator destroy() {
        
        yield return new WaitForSeconds(aud.clip.length + 0.1f);
        Destroy(gameObject);
    }

    bool running = false;
	// Update is called once per frame
	void Update () {
        if (!running && aud.isPlaying) {
            running = true;
            StartCoroutine(destroy());
        }
	}
}

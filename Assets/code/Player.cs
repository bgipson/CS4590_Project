using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

// help move player around
public class Player : MonoBehaviour {

    Client _client;
    public Vector3 position;
    public Vector3 nearestPlayerOfInterest;
    public float heading;
    float defaultHeading;
    public Vector3 headingVector;
    Vector3 upVector = new Vector3(0, 1, 0);
    Camera _camera;
    PlayerInfo _playerInfo;
    float speed = 0.25f;
    float speedHorizontal = 100f;

    // variometer
    float lastBeep;
    float timeBetweenBeeps; // 0.26 0.13 silence, duration 0.26 0.13

    static Vector3 worldCoordinateBaseOffset = new Vector3(-1000, 5, 0);

    public AudioSource growl;
    public AudioSource baseAmbientTrack;
    public AudioSource moreIntenseAmbientTrack;
    public AudioSource variometer;
    public AudioSource variometerBG;
    public AudioSource variometerBeep;
    // Use this for initialization
    void Start () {

        nearestPlayerOfInterest = new Vector3(-1000, 0, 60);

        // read in camera
        // also read in client
        _camera = Camera.main;
        _client = _camera.GetComponent<Client>();
        position = new Vector3(_camera.transform.position.x,
            _camera.transform.position.y,
            _camera.transform.position.z);
        Vector3 eulerAngles = _camera.transform.rotation.eulerAngles;
        heading = eulerAngles.y;
        defaultHeading = heading;
        headingVector = new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        lastBeep = Time.time;
        timeBetweenBeeps = 0.26f;
    }

    // Update is called once per frame
    void Update () { 

        if (_client.getPlayerInfo() != null)
        {
            _playerInfo = _client.getPlayerInfo();
            if (_playerInfo.isInitialized())
            {
                if (_client.pauseMouse)
                {
                    RotateCamera();
                }
                // write to playerInfo which the client will send to the server
                // update position
                if (MovementHasOccurred())
                {
                    CalculateHeading();
                    UpdatePosition();
                }
                // update heading


                float distance = Vector3.Distance(position, nearestPlayerOfInterest);
                //print(distance);
                float volumeBase = Mathf.Clamp((2*distance - 60.0f) / 100.0f, 0, 0.75f);
                baseAmbientTrack.volume = volumeBase;
                moreIntenseAmbientTrack.volume = Mathf.Clamp((2.0f*(100 - distance)) / 100, 0, 0.75f);

                // variometer
                if (distance > 0 && distance < 50)
                {
                    variometerBG.volume = 1;
                    variometer.volume = 0;

                    // min 10
                    //timeBetweenBeeps = 0.26f - 0.13f*Mathf.Clamp(((50f - distance)/(40f)), 0f, 1f);
                    timeBetweenBeeps = 0.26f - 0.13f * Mathf.Clamp((-1/40f)*distance + 1.25f, 0f, 1f);
                    if (lastBeep + timeBetweenBeeps*1.5 < Time.time)
                    {
                        variometerBeep.pitch = 1 + 2* Mathf.Clamp((-1 / 40f) * distance + 1.25f, 0f, 1f);
                        variometerBeep.Play();
                        lastBeep = Time.time;
                    }
                }
                else
                {
                    lastBeep = Time.time;
                    //also stop playing beep if playing
                    variometerBeep.Stop();

                    variometerBG.volume = 0;
                }

                if (distance < 80 && distance > 50)
                {
                    variometer.pitch = 1 + 1*((80 - distance)/30f);
                    variometer.volume = 1;
                } 
                if (distance > 80 && distance < 120)
                {
                    variometer.pitch = 1;
                    variometer.volume = ((120-distance)/40f);
                }
                if (distance > 120)
                {
                    variometer.volume = 0;
                }
            }
        }
	}

    void RotateCamera()
    {
        float rotate = Input.GetAxis("Mouse X") * speedHorizontal * Time.deltaTime;

        Vector3 eulerAngles = _camera.transform.rotation.eulerAngles;
        float newY = eulerAngles.y + rotate;
        _camera.transform.rotation = Quaternion.Euler(
            _camera.transform.rotation.eulerAngles.x,
            newY,
            _camera.transform.rotation.eulerAngles.z);
    }

    public void ResetSettings()
    {
        position = new Vector3(worldCoordinateBaseOffset.x,
            worldCoordinateBaseOffset.y,
            worldCoordinateBaseOffset.z);
        _camera.transform.position = position;
        heading = defaultHeading;
        Vector3 eulerAngles = _camera.transform.rotation.eulerAngles;
        headingVector = new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
    }

    void UpdatePosition()
    {
        position += AdjustedLocalSpeedVector();
        _camera.transform.position = position;
    }

    void CalculateHeading()
    {
        position.x = _camera.transform.position.x;
        position.y = _camera.transform.position.y;
        position.z = _camera.transform.position.z;
        heading = _camera.transform.rotation.eulerAngles.y;
        headingVector.x = Mathf.Sin((heading * Mathf.PI) / 180f);
        headingVector.z = Mathf.Cos((heading * Mathf.PI) / 180f);
    }

    public void zombieUpdate(Vector3 zombiePosition) {
        //float distance = Vector3.Distance(position, zombiePosition);
        //growl.volume = Mathf.Clamp((100 - distance*0.5f) / 100.0f, 0, 1);
        //growl.Play();
        //print((100 - distance) / 100);
    }

    public void ManageBackgroundAudio()
    {
        // @TODO:
        // use information about distance from player and zombiePosition here to adjust tracks

    }

    bool MovementHasOccurred()
    {
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            return true;
        }
        return false;
    }

    Vector3 AdjustedLocalSpeedVector()
    {
        float lowSpeed = speed * Mathf.Sqrt(2f) / 2f;
        Vector3 rightVector = -Vector3.Cross(headingVector, upVector);
        //
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return 0* headingVector;
        }
        // D (right)
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return speed* rightVector;
        }
        // S (back)
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return -speed* headingVector;
        }
        // SD (back right)
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return -lowSpeed* headingVector +
                lowSpeed* rightVector; // diagonal
        }
        // A (left)
        if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return -speed* rightVector;
        }
        // AD
        if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return 0* headingVector;
        }
        // AS (back left)
        if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return -lowSpeed* headingVector +
                -lowSpeed* rightVector; // diagonal
        }
        // ASD (back)
        if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return -speed* headingVector;
        }
        // W (forward)
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return speed* headingVector;
        }
        // WD (forward right)
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return lowSpeed* headingVector +
                lowSpeed* rightVector; // diagonal
        }
        // WS
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return 0* headingVector;
        }
        // WSD (right)
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return speed* rightVector;
        }
        // WA (forward left)
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return lowSpeed* headingVector +
                -lowSpeed* rightVector; // diagonal
        }
        // WAD (forward)
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return speed* headingVector;
        }
        // WAS (left)
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            return -speed* rightVector;
        }
        // WASD
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) &&
            Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            return 0* headingVector;
        }
        return 0* headingVector;
    }
}

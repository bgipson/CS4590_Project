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

    static Vector3 worldCoordinateBaseOffset = new Vector3(-1000, 5, 0);

    public AudioSource growl;
    public AudioSource baseAmbientTrack;
    public AudioSource moreIntenseAmbientTrack;
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
                float volume = Mathf.Clamp((distance - 30.0f) / 100.0f, 0, 1);
                print(volume);
                baseAmbientTrack.volume = volume;
                moreIntenseAmbientTrack.volume = Mathf.Clamp((2.0f*(100 - distance)) / 100, 0, 1);
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
        float distance = Vector3.Distance(position, zombiePosition);
        growl.volume = Mathf.Clamp((100 - distance) / 100, 0, 1);
        growl.Play();
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

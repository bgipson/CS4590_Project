using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    public enum Behavior
    {
        PATH,
        CHASE
    }

    
    //-1000, 0, 60
    //-987.44, 0, 47.13
    //-969.27, 0, 47.13
    //-969.27, 0, 102.45
    //-1133.7, 0, 102.45
    //-1133.7, 0, 42

    public PlayerInfo.Type type;
    public Behavior behavior;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 lastRotation;
    public string name;
    public bool canMove;
    public int waypoint;

    List<GameObject> waypoints;
    GameObject waypointActive;
    GameObject waypointNext;
    GameObject waypointLast;
    bool returnFromChase;
    Vector3 lastChasePosition;
    float speed = 10;

    // human info
    Vector3 hPosition;

    public NPC()
    {
        
    }

    public Vector3 getHumanPosition() { return hPosition; }

    public void setHumanPosition(Vector3 pIn)
    {
        hPosition.x = pIn.x;
        hPosition.y = pIn.y;
        hPosition.z = pIn.z;
    }

    public Vector3 getPosition() { return position; }

    public void setPosition(Vector3 pIn)
    {
        position.x = pIn.x;
        position.y = pIn.y;
        position.z = pIn.z;
    }

    public Vector3 getRotation() { return rotation; }

    public void setRotation(Vector3 rotateIn)
    {
        rotation.x = rotateIn.x;
        rotation.y = rotateIn.y;
        rotation.z = rotateIn.z;
    }

    public string getName() { return name; }

    public void setName(string nameIn)
    {
        name = nameIn;
    }

    public bool getCanMove() { return canMove; }

    public void setCanMove(bool moveIn)
    {
        canMove = moveIn;
    }

    // Use this for initialization
    void Start () {
        hPosition = new Vector3(-1000, 0, 0);
        lastChasePosition = new Vector3(-1110, 0, 450);
        returnFromChase = false;
        StartRoutine();
    }

    public void StartRoutine()
    {
        string zombieBehavior = "";
        if (behavior == Behavior.PATH)
        {
            zombieBehavior = "Path ";
        }
        if (behavior == Behavior.CHASE)
        {
            zombieBehavior = "Chase ";
        }
        waypoint = 0;
        waypoints = new List<GameObject>();
        lastRotation.y = rotation.y;
        int wpx = 0;
        GameObject wp = GameObject.Find(zombieBehavior + wpx);
        if (wp != null)
        {
            while (wp != null)
            {
                if (waypoints.Count == 0)
                {
                    waypointActive = wp;
                }
                if (waypoints.Count == 1)
                {
                    waypointNext = wp;
                }
                waypointLast = wp;
                waypoints.Add(wp);
                wpx++;
                wp = GameObject.Find(zombieBehavior + wpx);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (canMove)
        {
            if (behavior == Behavior.PATH)
            {
                if (waypoints.Count > 1)
                {
                    MoveBetweenPoints();
                }
            }
            if (behavior == Behavior.CHASE)
            {
                //@TODO: logic here for chase
                Vector3 localGoal = Destination.goal;
                localGoal.y = 0;
                float distanceToGoal = Vector3.Distance(hPosition, localGoal);
                if (distanceToGoal > 25)
                {
                    if (waypoints.Count > 1)
                    {
                        float distance = Vector3.Distance(hPosition, position);
                        if (distance < 50)
                        {
                            returnFromChase = true;
                            Vector3 lookat = hPosition;
                            lookat.y = 0;
                            transform.LookAt(lookat);
                            rotation = transform.forward;
                            UpdateLookHeading();
                            position = position + transform.forward * speed * Time.deltaTime;
                            lastChasePosition = position;
                        }
                        else
                        {
                            MoveBetweenPoints();
                        }
                    }
                }
            }
            transform.position = position;
            transform.eulerAngles = rotation;
        }
	}

    void UpdateLookHeading()
    {
        rotation = Quaternion.LookRotation(transform.forward, Vector3.up).eulerAngles;
    }

    Vector3 GetFromWaypoint()
    {
        if (returnFromChase)
        {
            return lastChasePosition;
        }
        return waypointActive.GetComponent<Transform>().position;
    }

    void MoveBetweenPoints()
    {
        Vector3 a = GetFromWaypoint();
        Vector3 b = waypointNext.GetComponent<Transform>().position;
        float edgeDistance = Vector3.Distance(a, b);
        float distSoFar = Vector3.Distance(a, position);
        Vector3 direction = b - a;
        Vector3 unitDirection = direction.normalized;
        position = position + unitDirection * speed*Time.deltaTime;
        float totalDist = Vector3.Distance(a, position);

        float proportion = totalDist / edgeDistance;
        
        if (proportion < 0.1)
        {
            float lerpTo = ComputeHeading(unitDirection);
            float lerpFrom = lastRotation.y;
            if (lerpTo - lerpFrom >= 180.0)
            {
                lerpFrom += 360f;
            }
            else if (lerpFrom - lerpTo >= 180.0)
            {
                lerpFrom -= 360f;
            }
            rotation.y = lerpFrom + (proportion/0.1f)*(lerpTo-lerpFrom);
        } else
        {
            rotation.y = ComputeHeading(unitDirection);
        }

        if (totalDist > edgeDistance)
        {
            returnFromChase = false;
            position = b;
            lastRotation.y = rotation.y;
            waypoint++;
            if (waypoint >= waypoints.Count)
            {
                waypoint = 0;
            }
            waypointActive = waypoints[waypoint];
            waypointNext = waypoints[NextWp(waypoint)];
        }
    }

    float ComputeHeading(Vector3 heading)
    {
        float dir = Mathf.Atan2(heading.x, heading.z)*180/Mathf.PI;
        return dir;
    }

    int LastWp(int wpIndex)
    {
        if (wpIndex - 1 < 0)
        {
            return waypoints.Count - 1;
        }
        return wpIndex - 1;
    }

    int NextWp(int wpIndex)
    {
        if (wpIndex + 1 >= waypoints.Count)
        {
            return 0;
        }
        return wpIndex + 1;
    }
}

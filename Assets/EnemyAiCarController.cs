using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAiCarController : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();
    public List<Transform> boundarySensors;

    [SerializeField]
    bool boundarySafe = true;

    public float driveTorque = 35f;
    float origTorque;

    public Transform playerTransform;

    [SerializeField]
    float crossVert;

    bool dying;
    bool colliding;
    float collideSteer;
    float collisionTimer;

    Coroutine dieCrt = null;

    void Start()
    {
        origTorque = driveTorque;
    }

    public void Init(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.tag == "Player" && Random.Range(0f,1f) > 0.66f)
        {
            StartCoroutine(CollideBehavior());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            collisionTimer += Time.fixedDeltaTime;

            if(collisionTimer > 4f)
            {
                driveTorque = origTorque * 0.75f;
                collisionTimer = 0f;

                if(Random.Range(0f, 1f) > 0.66f)
                {
                    StartCoroutine(CollideBehavior());
                }
            }
            if (collisionTimer > 2f)
            {
                driveTorque = origTorque * 1.5f;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            driveTorque = origTorque;
            collisionTimer = 0f;
        }
    }
    void FixedUpdate()
    {
        #region Drive

        SenseBoundary();

        float torque = 0f;
        float steerAngle = 0f;

        Vector3 fromToPlayer = playerTransform.position - transform.position;
        crossVert = Vector3.Cross(transform.forward.normalized, fromToPlayer.normalized).y;

        if (colliding)
        {
            torque = -driveTorque;
            steerAngle = collideSteer;
        }
        else
        {
            if (crossVert > 0.1f) // Player is to the right    
            {
                steerAngle = 45f;
                collideSteer = -steerAngle;
                torque = driveTorque;
            }
            else if (crossVert < -0.1f)
            {
                steerAngle = -45f;
                collideSteer = -steerAngle;
                torque = driveTorque;
            }
            else
            {
                steerAngle = 0f;
                collideSteer = 0f;
                torque = driveTorque;
            }
        }

        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = torque;
            wheel.steerAngle = steerAngle;
        }

        #endregion

        #region Life Controller

        
        WheelHit hit;
        bool isTouching = false;

        foreach (WheelCollider w in wheels)
        {
            if(w.GetGroundHit(out hit))
            {
                isTouching = true;
            }
        }

        if (!isTouching) // All wheels off the ground (flipped)
        {
            if(!dying)
                dieCrt = StartCoroutine(Die(2f));
        }
        else if(dying)
        {
            StopCoroutine(dieCrt);
            dieCrt = null;
            dying = false;
            Debug.Log("Enemy stop dying");
        }

        #endregion
    }

    private IEnumerator Die(float delay)
    {
        Debug.Log("Enemy Dying crt started");
        dying = true;

        yield return new WaitForSeconds(delay);

        List<Transform> allChildren = GetAllChildren(transform);

        allChildren.Remove(transform);
        Destroy(gameObject, 2.5f);

        foreach (Transform t in allChildren)
        {
            t.gameObject.AddComponent<MeshCollider>().convex = true;
            Rigidbody chRb = t.GetComponent<Rigidbody>();
            
            if (!chRb)
            {
                chRb = t.gameObject.AddComponent<Rigidbody>();
            }

            chRb.AddForce(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);

            Destroy(t.gameObject, 2.5f);
            t.parent = null;
        }
    }

    private IEnumerator CollideBehavior()
    {
        Debug.Log("CollideBehavior");
        colliding = true;

        yield return new WaitForSeconds(Random.Range(0.33f, 1f));

        colliding = false;
    }

    List<Transform> GetAllChildren(Transform toSearch)
    {
        List<Transform> children = new List<Transform>();

        foreach (Transform t in toSearch)
        {
            children.Add(t);
            children.AddRange(GetAllChildren(t));
        }

        return children;
    }

    void SenseBoundary()
    {
        RaycastHit hit;
        boundarySafe = true;

        foreach (Transform t in boundarySensors)
        {
            if(!Physics.Raycast(t.position, -t.up, 100f))
            {
                boundarySafe = false;
            }
        }
    }
}
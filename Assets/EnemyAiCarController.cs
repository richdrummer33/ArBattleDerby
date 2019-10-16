using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAiCarController : MonoBehaviour//
{
    public List<WheelCollider> wheels = new List<WheelCollider>();
    public List<Transform> boundarySensorsFront;
    public List<Transform> boundarySensorsBack;

    [SerializeField]
    bool backSafe = true;

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

    Rigidbody rb;

    [Range(1f, 4f)]
    public float aggressionModifier = 1f;

    void Start()
    {
        origTorque = driveTorque;
        StartCoroutine(SpawnInvincibility());
        rb = GetComponent<Rigidbody>();
    }

    public void Init(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    float pushTime;
    private void OnTriggerEnter(Collider other)
    {
        float rand = Random.Range(0f, 1f);

        if (other.tag == "Enemy" && rand > 0.66f)
        {
            StartCoroutine(CollideBehavior());
        }
        else if(other.transform.tag == "Player" && rand > 0.66f + (aggressionModifier - 1f) / 4f * 0.33f)
        {
            pushTime = Random.Range(0.5f, Mathf.Sqrt(1f + aggressionModifier));
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

                if(collisionTimer > pushTime)
                {
                    StartCoroutine(CollideBehavior());
                }
            }
            if (collisionTimer > 2f)
            {
                driveTorque = origTorque * 1.75f;
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
        
        float torque = 0f;
        float steerAngle = 0f;

        Vector3 fromToPlayer = playerTransform.position - transform.position;
        crossVert = Vector3.Cross(transform.forward.normalized, fromToPlayer.normalized).y;
      
        if (colliding && SenseBoundary(boundarySensorsBack))
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

        if (!isTouching && !cantDie) // All wheels off the ground (flipped)
        {
            if(!dying)
                dieCrt = StartCoroutine(Die(3f, 0.5f));
        }
        else if(dying && dieCrt != null)
        {
            StopCoroutine(dieCrt);
            dieCrt = null;
            dying = false;
            Debug.Log("Enemy stop dying");
        }

        /*
        if (!rb.useGravity && !zerogDie)
        {
            if (dying) // Check, just in case
            {
                StopCoroutine(dieCrt);
            }

            dieCrt = StartCoroutine(Die(4f));
            zerogDie = true;
        }
        else
        {
            zerogDie = false;
        }
        */

        #endregion
    }
    //
    bool cantDie;
    bool zerogDie;

    private IEnumerator SpawnInvincibility()
    {
        cantDie = true;
        yield return new WaitForSeconds(2f);
        cantDie = false;
    }
    
    public void Explode()
    {
        cantDie = true; // Prevent double call
        StartCoroutine(Die(0f, 2.5f));
    }

    private IEnumerator Die(float delay, float forceMod)
    {
        Debug.Log("Enemy Dying crt started");
        dying = true;
        
        yield return new WaitForSeconds(delay);

        if (ArCarController.instance)
            ArCarController.instance.EnemyDeath(this.gameObject);

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

            chRb.AddForce(Random.insideUnitSphere * forceMod, ForceMode.Impulse);
            
            Destroy(t.gameObject, 2.5f);
            t.parent = null;
        }
    }

    private IEnumerator CollideBehavior()
    {
        Debug.Log("CollideBehavior");
        colliding = true;

        yield return new WaitForSeconds(Random.Range(0.33f, 1f / Mathf.Sqrt(aggressionModifier)));

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

    bool SenseBoundary(List<Transform> boundarySensors)
    {
        foreach (Transform t in boundarySensors)
        {
            if(!Physics.Raycast(t.position, -t.up, 100f))
            {
                backSafe = false;
                return false;                
            }
        }

        backSafe = true;
        return true;
    }
}
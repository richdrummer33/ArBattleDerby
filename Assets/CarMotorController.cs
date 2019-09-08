using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMotorController : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();
    public List<WheelCollider> rearWheels = new List<WheelCollider>();

    public bool kbDrive = false;

    public Animator nearDeathAnim;

    public GameObject ExplodyBitsPrefab;

    [SerializeField]
    List<GameObject> enemyCars = new List<GameObject>();

    public float driveTorque = 35f;
    float origTorque;

    bool bumped;
    bool forced;

    public AudioSource boomForceSource;
    float boomCharge;

    Vector3 positionStart;
    Quaternion rotStart;

    bool resetting;

    Vector3 lastUp;
    [SerializeField]
    float rotDir;
    int rotCt;

    Rigidbody rb;

    Coroutine crt;

    int ct = 0;
           
    void Start()
    {
        positionStart = transform.position;
        rotStart = transform.rotation;
        rb = transform.root.GetComponent<Rigidbody>();
        origTorque = driveTorque;
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Enemy")
        {
            driveTorque = origTorque * 1.5f;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            driveTorque = origTorque;
        }
    }

    void FixedUpdate()
    {
        if (!resetting)
        {
            if (rotCt > 25)
            {
                rotDir = Vector3.Cross(transform.up, lastUp).normalized.y; // Positive is clockwise

                if (rotDir < 0f)
                    rotDir = -1f;
                else
                    rotDir = 1f;

                lastUp = transform.up;
                rotCt = 0;
            }

            rotCt++;
        }

        WheelHit hit;

        bool hitL = wheels[0].GetGroundHit(out hit);
        bool hitR = wheels[1].GetGroundHit(out hit);
        bool hitLb = rearWheels[0].GetGroundHit(out hit);
        bool hitRb = rearWheels[1].GetGroundHit(out hit);

        if ((hitL && hitR && hitLb && hitRb))
        {
            nearDeathAnim.SetBool("Near Death", false);

            if (crt != null)
            {
                StopCoroutine(crt);
                resetting = false;
            }
        }
        else if (!hitL && !hitR && !resetting)
        {
            ct++;

            if (ct > 30)
            {
                nearDeathAnim.SetBool("Near Death", true);
            }

            if (ct > 60)
            {
                crt = StartCoroutine(Reset());
                ct = 0;
            }
        }

        if (kbDrive)
        {
            KbDrive();
        }
    }

    void Update()
    {
        if(enemyCars.Count > 1)
        {
            ArCarController.instance.ChargeTheBoom();
        }
        else
        {
            ArCarController.instance.DrainTheBoom();
        }
    }

    private IEnumerator Reset()
    {
        resetting = true;

        yield return new WaitForSeconds(2f);

        GameObject explodyBitsInst = Instantiate(ExplodyBitsPrefab, transform.position, transform.rotation, null);
         

        // Effects
        foreach (Transform t in explodyBitsInst.transform)
        {
            Destroy(t.gameObject, 2.5f);

            Rigidbody chRb = t.GetComponent<Rigidbody>();

            if (chRb)
            {
                chRb.AddForce(Random.insideUnitSphere * 7.5f, ForceMode.Impulse);
                //t.parent = null;                
            }
        }
        
        rb.velocity = Vector3.zero;
        transform.position = positionStart;
        transform.rotation = rotStart;

        nearDeathAnim.SetBool("Near Death", false);
        resetting = false;

        ArCarController.instance.PlayerDeath();
    }

    #region Movement

    public void SteerLeft()
    {
        if (resetting)
        {
            rb.AddTorque(100000f * transform.forward);
            //rb.AddForce(100000f * Vector3.Cross(Vector3.up, transform.forward));
        }
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = driveTorque;
            wheel.steerAngle = -45f;
        }        
    }

    public void SteerLeftBackward()
    {
        if (resetting)
        {
            rb.AddTorque(100000f * transform.forward);
            //rb.AddForce(100000f * Vector3.Cross(Vector3.up, transform.forward));
        }
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = -driveTorque;
            wheel.steerAngle = -45f;
        }
    }

    public void SteerRight()
    {
        if (resetting)
        {
            rb.AddTorque(100000f * -transform.forward);
            //rb.AddForce(100000f * -Vec1tor3.Cross(Vector3.up, transform.forward));
        }
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = driveTorque;
            wheel.steerAngle = 45f;
        }        
    }

    public void SteerRightBackward()
    {
        if (resetting)
        {
            rb.AddTorque(100000f * -transform.forward);
            //rb.AddForce(100000f * -Vector3.Cross(Vector3.up, transform.forward));
        }
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = -driveTorque;
            wheel.steerAngle = 45f;
        }
    }

    public void SteerStraight(bool flip)
    {
        if (resetting && !bumped && flip)
        {
            StartCoroutine(Bump());
        }

        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = driveTorque * 45f / 35f;
            wheel.steerAngle = 0f;
        }        
    }

    public void SteerBackwards()
    {
        if (resetting)
        {
            rb.AddForce(100f * transform.forward * rotDir, ForceMode.Impulse);
        }
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = -45f;
        }
    }

    public void StopCar()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 0f;
            wheel.steerAngle = 0f;
        }
    }

    public void MakeBoom()
    {
        foreach (GameObject en in enemyCars)
        {
            Rigidbody enRb = en.GetComponent<Rigidbody>();

            Vector3 dir = (en.transform.position - transform.position).normalized + Vector3.up;

            enRb.AddForce(dir * Random.Range(0.75f, 1.25f) * 2500f * boomForceModifier, ForceMode.Impulse);
            enRb.AddTorque(Random.insideUnitSphere * Random.Range(0.75f, 1.25f) * 3500f * boomForceModifier, ForceMode.Impulse);
        }

        rb.AddForce(Random.insideUnitSphere * Random.Range(0.75f, 1.25f) * 500f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * Random.Range(0.75f, 1.25f) * 500f, ForceMode.Impulse);

        boomForceModifier = 1f;

        boomForceSource.Play();        
    }

    float boomForceModifier = 1f;
    public void ChargeBoom()
    {
        boomForceModifier = Mathf.Clamp(boomForceModifier + Time.deltaTime * 0.25f, 0f, 2.5f);
    }

    #endregion

    #region Keyboard

    private void KbDrive()
    {
        float torque = 0f;
        float steer = 0f;


        if (Input.GetKey(KeyCode.A)) // Left/Right
        {
            SteerLeft();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            SteerRight();
        }

        else if (Input.GetKey(KeyCode.W)) // Fwd/Back
        {
            SteerStraight(true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            SteerBackwards();
        }
        else
            StopCar();

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!forced)
                StartCoroutine(ForceSurroundingEnemies());
        }
    }

    #endregion

    private IEnumerator Bump()
    {
        bumped = true;

        rb.AddForce(Vector3.up * 2500f, ForceMode.Impulse);
        rb.AddTorque(transform.forward * 2500f * rotDir, ForceMode.Impulse);

        yield return new WaitForSeconds(1f);

        bumped = false;
    }

    private IEnumerator ForceSurroundingEnemies()
    {
        if (enemyCars.Count > 0)
        {
            forced = true;
            boomCharge = 0f;

            foreach (GameObject en in enemyCars)
            {
                Rigidbody enRb = en.GetComponent<Rigidbody>();

                Vector3 dir = (en.transform.position - transform.position).normalized + Vector3.up;

                enRb.AddForce(dir * Random.Range(0.75f, 1.25f) * 4000f, ForceMode.Impulse);
                enRb.AddTorque(Random.insideUnitSphere * Random.Range(0.75f, 1.25f) * 4500f, ForceMode.Impulse);
            }

            boomForceSource.Play();

            while(boomCharge < 1f)
            {
                boomCharge += Time.deltaTime;
            }

            yield return new WaitForSeconds(1f);

            forced = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Enemy")
        {
            if (!enemyCars.Contains(other.gameObject))
                enemyCars.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            enemyCars.Remove(other.gameObject);
        }
    }

}

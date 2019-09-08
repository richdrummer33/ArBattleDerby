using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMotorController : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();
    public List<WheelCollider> rearWheels = new List<WheelCollider>();

    public bool kbDrive = false;

    Vector3 positionStart;
    Quaternion rotStart;

    bool resetting;

    Rigidbody rb;

    Coroutine crt;

    int ct = 0;
           
    void Start()
    {
        positionStart = transform.position;
        rotStart = transform.rotation;
        rb = transform.root.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        WheelHit hit;

        bool hitL = wheels[0].GetGroundHit(out hit);
        bool hitR = wheels[1].GetGroundHit(out hit);
        bool hitLb = rearWheels[0].GetGroundHit(out hit);
        bool hitRb = rearWheels[1].GetGroundHit(out hit);

        if ((hitL && hitR && hitLb && hitRb) && crt != null)
        {
            StopCoroutine(crt);
            resetting = false;
        }
        else if (!hitL && !hitR && !resetting)
        {
            ct++;

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

    private IEnumerator Reset()
    {
        resetting = true;

        yield return new WaitForSeconds(1f);

        transform.position = positionStart;
        transform.rotation = rotStart;

        resetting = false;
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
            wheel.motorTorque = 35f;
            wheel.steerAngle = -45f;
            Debug.Log("steering L now");
        }        
    }

    public void SteerRight()
    {
        if (resetting)
        {
            rb.AddTorque(100000f * -transform.forward);
            //rb.AddForce(100000f * -Vector3.Cross(Vector3.up, transform.forward));
        }
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 35f;
            wheel.steerAngle = 45f;
            Debug.Log("steering R now");
        }        
    }

    public void SteerStraight()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 45f;
            Debug.Log("straight");
        }        
    }

    public void SteerBackwards()
    {
        if (resetting)
        {
            rb.AddForce(100f * transform.forward, ForceMode.Impulse);
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
            Debug.Log("steering R now");
        }
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
            SteerStraight();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            SteerBackwards();
        }
        else
            StopCar();
    }

    #endregion
}

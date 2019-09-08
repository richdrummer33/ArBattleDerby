﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMotorController : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();
    public List<WheelCollider> rearWheels = new List<WheelCollider>();

    public bool kbDrive = false;

    public Animator nearDeathAnim;

    public float driveTorque = 35f;
    float origTorque;

    bool bumped;

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

    private IEnumerator Reset()
    {
        resetting = true;

        yield return new WaitForSeconds(2f);

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

    public void SteerStraight()
    {
        if (resetting && !bumped)
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

    private IEnumerator Bump()
    {
        bumped = true;

        rb.AddForce(Vector3.up * 2500f, ForceMode.Impulse);
        rb.AddTorque(transform.forward * 2500f * rotDir, ForceMode.Impulse);

        yield return new WaitForSeconds(1f);

        bumped = false;
    }

}

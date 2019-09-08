using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAiCarController : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();
    public List<WheelCollider> rearWheels = new List<WheelCollider>();

    public Transform playerTransform;

    bool dying;
    Coroutine dieCrt = null;

    public void Init(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    void FixedUpdate()
    {
        #region Drive

        float torque = 0f;
        float steerAngle = 0f;

        Vector3 fromToPlayer = playerTransform.position - transform.position;
        float crossVert = Vector3.Cross(transform.forward.normalized, fromToPlayer.normalized).y;

        if(crossVert > 0.1f) // Player is to the right    
        {
            steerAngle = 35f;
            torque = 35f;
        }
        else if (crossVert > -0.1f)
        {
            steerAngle = -35f;
            torque = 35f;
        }
        else
        {
            steerAngle = 0f;
            torque = 65f;
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

        if (!isTouching && dieCrt != null) // All wheels off the ground (flipped)
        {
            dieCrt = StartCoroutine(Die());
        }
        else if(dying)
        {
            StopCoroutine(dieCrt);
            dieCrt = null;
            dying = false;
        }

        #endregion
    }

    private IEnumerator Die()
    {
        dying = true;

        yield return new WaitForSeconds(2f);

        foreach(Transform t in transform)
        {
            t.parent = null;
            Rigidbody chRb = t.gameObject.AddComponent<Rigidbody>();
            chRb.AddForce(Random.insideUnitSphere * 100f, ForceMode.Impulse);
            Destroy(t.gameObject, 3f);
        }
    }
}

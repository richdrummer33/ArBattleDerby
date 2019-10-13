using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSoundController : MonoBehaviour
{
    [SerializeField]
    float topSpeed = 10f;
    float speed;

    [SerializeField]
    AudioSource source;



    Rigidbody rb;

    CarMotorController motor;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        motor = GetComponent<CarMotorController>();
    }

    // Update is called once per frame
    void Update()
    {
        speed = rb.velocity.sqrMagnitude;

        if (motor.grounded)
        {
            source.pitch = speed / topSpeed + 0.5f;
            source.volume = Mathf.Clamp(speed / topSpeed + 0.5f, 0.5f, 1f);
        }
    }
}

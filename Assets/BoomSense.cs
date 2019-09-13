using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomSense : MonoBehaviour
{
    CarMotorController motor;

    void Start()
    {
        motor = GetComponentInParent<CarMotorController>();
    }

    private void OnTriggerEnter(Collider other) // previously "stay"
    {
        if (other.tag == "Enemy")
        {
            motor.AddEnemy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            motor.RemoveEnemy(other.gameObject);
        }
    }
}

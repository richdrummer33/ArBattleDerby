using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySucker : MonoBehaviour
{
    public float suckForce = 50f;

    bool suckActive = false;

    List<Transform> enemiesInRange = new List<Transform>();

    void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Enemy")
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        enemiesInRange.Remove(other.transform);
    }

    public void Suck()
    {
        suckActive = true;
    }

    public void UnSuck()
    {
        suckActive = false;

        foreach (Transform en in enemiesInRange)
        {
            Rigidbody rb = en.GetComponent<Rigidbody>();
            rb.drag = 0f;
            rb.useGravity = true;
            
            Vector3 dir = (en.transform.position - transform.position).normalized + Vector3.up * 0.2f;

            rb.AddForce(dir * Random.Range(0.75f, 1.25f) * 10000f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(0.75f, 1.25f) * 4500f, ForceMode.Impulse);
        }
    }

    void Update()
    {
        if(suckActive)
        {
            foreach (Transform en in enemiesInRange)
            {
                Debug.Log("Suck this " + en.name);

                Rigidbody rb = en.GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.drag = 4f;

                Vector3 fromTo = transform.position- en.transform.position;
                rb.AddForce(fromTo * suckForce);

            }
        }
    }
}

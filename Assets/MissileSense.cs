using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Due to layer setup not to interact with Player, need this on GO on another layer 
public class MissileSense : MonoBehaviour
{
    [SerializeField]
    public List<Rigidbody> inRangeRbs = new List<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.transform.root.GetComponentInChildren<Rigidbody>();

        if (!inRangeRbs.Contains(rb))
        {
            if (other.tag == "Enemy" || other.tag == "Player")
            {
                inRangeRbs.Add(rb);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy" || other.tag == "Player")
        {
            inRangeRbs.Remove(other.transform.root.GetComponentInChildren<Rigidbody>());
        }
    }
}

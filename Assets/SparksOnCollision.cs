using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparksOnCollision : MonoBehaviour
{
    public ParticleSystem sparkPrefab;
    ParticleSystem sparkInst;

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("ASDASD HITT");

        if (!sparkInst)
            sparkInst = Instantiate(sparkPrefab);

        if (other.transform.tag == "Enemy")
        {
            if (!sparkInst.isPlaying)
            {
                sparkInst.transform.position = other.GetContact(0).point;
                sparkInst.transform.rotation = Quaternion.LookRotation(other.GetContact(0).normal);
                sparkInst.Play();
            }
            else
            {
                ParticleSystem tempParticle = Instantiate(sparkPrefab, other.GetContact(0).point, Quaternion.LookRotation(other.GetContact(0).normal));
                Destroy(tempParticle.gameObject, tempParticle.main.duration + 0.25f);
            }
        }
    }
}

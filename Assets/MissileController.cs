using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    [SerializeField]
    float explosionForce = 1.25f;
    
    [SerializeField]
    Transform target;
    float cruiseSpeed = 3f;
    float speed;
    public SphereCollider explodeSphere;

    [SerializeField]
    List<Rigidbody> inRangeRbs = new List<Rigidbody>();

    bool physMotion = false;
    Vector3 _initialVelocity = Vector3.zero;
    float lifetime;
    Vector3 noTargetDestination;

    [SerializeField]
    ParticleSystem smokeFx;
    [SerializeField]
    List<ParticleSystem> explodeFx = new List<ParticleSystem>();

    private void Start()
    {
        noTargetDestination = transform.position + Random.insideUnitSphere * 1000f;

        Rigidbody carRb = transform.root.GetComponent<Rigidbody>();
        if (inRangeRbs.Contains(carRb))
        {
            inRangeRbs.Add(carRb);
        }

        Destroy(this.gameObject, 8f);
    }

    public void SetTarget(Transform enemyTarget, Vector3 initialVelocity)
    {
        target = enemyTarget;

        _initialVelocity = initialVelocity;
    }

    void Update()
    {
        Vector3 targetPos;

        if (target)
        {
            targetPos = target.position;
        }
        else
        {
            targetPos = noTargetDestination;
        }

        lifetime += Time.deltaTime;

        speed = Mathf.Clamp(speed + Mathf.Pow(lifetime, 2f) * Time.deltaTime, 0f, cruiseSpeed);

        transform.position += (transform.forward * speed + _initialVelocity * 1.5f / (lifetime + 1f)) * Time.deltaTime;

        Vector3 lookDir = targetPos - transform.position;

        Quaternion aimRotation = Quaternion.LookRotation(lookDir);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, 90f * Time.deltaTime);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Player")
        { 
            foreach (Rigidbody rb in inRangeRbs)
            {
                float distNorm = Mathf.Clamp(Mathf.Pow((explodeSphere.radius - Vector3.Distance(rb.transform.position, transform.position)), 1.25f) / explodeSphere.radius, 0f, 1f);

                if(rb.gameObject.tag == "Player")
                {
                    distNorm /= 3f;
                }

                Debug.Log("distNorm "  + distNorm);

                Vector3 forceDir = (rb.transform.position - transform.position + Vector3.up).normalized;

                rb.AddForce(forceDir * 5000f * explosionForce * distNorm, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * Mathf.RoundToInt(Random.Range(-1f, 1f)) * 400f * explosionForce * distNorm, ForceMode.Impulse);
            }           

            foreach(ParticleSystem sys in explodeFx)
            {
                sys.transform.parent = null;
                sys.transform.rotation = Quaternion.LookRotation(Vector3.up);
                Destroy(sys.gameObject, sys.main.startLifetime.constant);
                sys.Play();
            }

            smokeFx.transform.parent = null; // Smoke trail
            smokeFx.Stop();
            Destroy(smokeFx.gameObject, smokeFx.main.startLifetime.constantMax);

            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy" || other.tag == "Player")
        {
            inRangeRbs.Add(other.transform.root.GetComponentInChildren<Rigidbody>());
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

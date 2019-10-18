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
    
    MissileSense missileSense;

    bool physMotion = false;
    Vector3 _initialVelocity = Vector3.zero;
    float lifetime;
    Vector3 noTargetDestination;

    [SerializeField]
    ParticleSystem smokeFx;
    [SerializeField]
    List<ParticleSystem> explodeFx = new List<ParticleSystem>();

    float maxLifetime = 5f;

    private void Start()
    {
        missileSense = GetComponentInChildren<MissileSense>();

        noTargetDestination = transform.position + Random.insideUnitSphere * 1000f;
        noTargetDestination.y = Mathf.Clamp(noTargetDestination.y, transform.position.y, Mathf.Infinity);

        Rigidbody carRb = transform.root.GetComponent<Rigidbody>();
        if (missileSense.inRangeRbs.Contains(carRb))
        {
            missileSense.inRangeRbs.Add(carRb);
        }

        StartCoroutine(DestroyDelayed(maxLifetime));
    }

    IEnumerator DestroyDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        smokeFx.transform.parent = null; // Smoke trail
        smokeFx.Stop();
        Destroy(smokeFx.gameObject, smokeFx.main.startLifetime.constantMax);

        Destroy(this.gameObject);
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

            if (lifetime > maxLifetime * 0.5f)
                targetPos += Vector3.up * 0.2f + Random.insideUnitSphere * 0.2f; // Makes the missile go crazy when it passes thru target pos without exploding HAHAHA
        }
        else
        {
            targetPos = noTargetDestination;
        }

        lifetime += Time.deltaTime;

        speed = Mathf.Clamp(speed + Mathf.Pow(lifetime, 2f) * Time.deltaTime, 0.5f, cruiseSpeed);

        transform.position += (transform.forward * speed + _initialVelocity * 1.5f / (lifetime + 1f)) * Time.deltaTime;

        Vector3 lookDir = targetPos - transform.position;

        if (lifetime > 0.33f)
        {
            Quaternion aimRotation = Quaternion.LookRotation(lookDir);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, 110f * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            foreach (Rigidbody rb in missileSense.inRangeRbs)
            {
                //Debug.Log("explodeSphere.radius " + explodeSphere.radius);
                //Debug.Log("Vector3.Distance(rb.transform.position, transform.position) " + Vector3.Distance(rb.transform.position, transform.position));

                float distNorm = Mathf.Clamp(Mathf.Pow((explodeSphere.radius - Vector3.Distance(rb.transform.position, transform.position)), 1.66f) / explodeSphere.radius, 0.01f, 1f);

                EnemyAiCarController enemy = rb.gameObject.GetComponent<EnemyAiCarController>();

                if (rb.transform == collision.transform)
                {
                    enemy.Explode();
                }
                else if (rb.gameObject.tag == "Player")
                {
                    distNorm = distNorm / 3f;
                }

                //Debug.Log("distNorm "  + distNorm);

                Vector3 forceDir = (rb.transform.position - transform.position + Vector3.up).normalized;

                rb.AddForce(forceDir * 3000f * explosionForce * distNorm, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * Mathf.RoundToInt(Random.Range(-1f, 1f)) * 400f * explosionForce * distNorm, ForceMode.Impulse);
            }

            foreach (ParticleSystem sys in explodeFx)
            {
                sys.transform.parent = null;
                sys.transform.rotation = Quaternion.LookRotation(Vector3.up);
                StartCoroutine(RecycleDelayed(sys.gameObject, sys.main.startLifetime.constant)); // Destroy(sys.gameObject, sys.main.startLifetime.constant);
                sys.Play();
            }

            smokeFx.transform.parent = null; // Smoke trail
            smokeFx.Stop();
        }

        IEnumerator RecycleDelayed(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);

            ObjectPool.Recycle(obj);
        }
    }
}

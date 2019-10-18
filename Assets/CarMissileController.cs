using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Launcher
public class CarMissileController : WeaponController
{
    [SerializeField]
    GameObject missilePrefab;

    [SerializeField]
    List<Transform> launchTubes;

    int currentTube = 0;

    [SerializeField]
    List<Transform> enemiesInRange = new List<Transform>();

    Rigidbody carRb;

    private void Start()
    {
        carRb = transform.root.GetComponent<Rigidbody>();

        ObjectPool.CreatePool(missilePrefab, 10);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            LaunchMissile();
        }
    }

    public void LaunchMissile()
    {
        if (ammoCount > 0)
        { 
            GameObject missile = ObjectPool.Spawn(missilePrefab, launchTubes[currentTube].position, Quaternion.LookRotation(launchTubes[currentTube].up)); // Instantiate(missilePrefab, launchTubes[currentTube].position, Quaternion.LookRotation(launchTubes[currentTube].up), null);

            Transform target = null;

            if (enemiesInRange.Count > 0)
            {
                int randIndex = Mathf.RoundToInt(Random.Range(0f, enemiesInRange.Count - 1f));

                target = enemiesInRange[randIndex];
            }

            missile.GetComponent<MissileController>().SetTarget(target, carRb.velocity + (carRb.transform.forward + carRb.transform.up * 0.15f) * 0.5f);

            currentTube = (currentTube + 1) % launchTubes.Count;

            ammoCount--;
        }
    }

    public override void PickupAmmo(int quantity)
    {
        base.PickupAmmo(quantity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            enemiesInRange.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            enemiesInRange.Remove(other.transform);
        }
    }
}

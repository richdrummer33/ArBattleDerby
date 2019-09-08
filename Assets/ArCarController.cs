using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArCarController : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    public GameObject carPrefab;

    public GameObject enemyPrefab;
    
    public Text frLifeCt;
    public Text enDeathCount;
    public Text gameOver;

    GameObject currentEnemy;

    int enCt;
    int enDeaths;
    bool gameOverStatus;
    
    public int playerLives = 5;

    private GameObject currentCar;

    public ARPlaneManager planeManager;

    Vector3 enemySpawn = Vector3.zero;

    ARPlane spawnPlane;

    CarMotorController motor;

    List<ARPlane> allPlanes = new List<ARPlane>();

    public static ArCarController instance;

    void OnEnable()
    {
        planeManager.planesChanged += OnPlaneAdded;
        instance = this;
        frLifeCt.text = "Lives Remaining: " + playerLives;
        enDeathCount.text = "Enemys Destroyed: " + enDeaths;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlaneAdded;
    }

    void Update()
    {
        if (playerLives == 0)
        {
            if (!gameOverStatus)
            {
                StartCoroutine(GameOver());
            }
        }
        else
        {
            Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

            if (Input.touchCount == 1) // Touching screen
            {
                if (hits.Count > 0)
                {
                    if (currentCar == null)
                    {
                        currentCar = Instantiate(carPrefab, hits[0].pose.position, Quaternion.identity);
                        motor = currentCar.GetComponent<CarMotorController>();

                        planeManager.trackables.TryGetTrackable(hits[0].trackableId, out spawnPlane);
                        StartCoroutine(RandomSpawnGenerator());

                        Debug.Log("Car placed");
                    }
                }

                if (motor != null)
                {
                    var touch = Input.GetTouch(0);

                    if (touch.position.x < Screen.width / 3f)
                    {
                        if (touch.position.y > Screen.height * (1f / 8f))
                            motor.SteerLeft();
                        else
                            motor.SteerLeftBackward();
                    }
                    else if (touch.position.x > Screen.width * (2f / 3f))
                    {
                        if(touch.position.y > Screen.height * (1f / 8f))
                            motor.SteerRight();
                        else
                            motor.SteerRightBackward();
                    }
                    else if (touch.position.y > Screen.height * (1f / 8f))
                    {
                        motor.SteerStraight();
                    }
                    else
                    {
                        motor.SteerBackwards();
                    }
                }
            }
            else if (Input.touchCount > 1 && motor != null)
            {
                motor.SteerStraight();
            }
            else if (motor)
            {
                motor.StopCar();
            }

            if (!currentEnemy && allPlanes.Count > 0 && currentCar && enemySpawn != Vector3.zero)
            {
                currentEnemy = SpawnEnemy();
            }
        }
    }

    private GameObject SpawnEnemy()
    {
        Debug.Log("Attempt Spawn");

        Vector3 spawnPos = enemySpawn;

        //Vector3 innerDirection = spawnPlane.center - spawnPos; // Vector that points from the extremity to the center of the plane

        //Quaternion spawnRot = Quaternion.Euler(innerDirection);

        Quaternion spawnRot = Quaternion.identity;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        newEnemy.GetComponent<EnemyAiCarController>().Init(currentCar.transform);

        Debug.Log("Spawned!!!");

        enCt++;

        return newEnemy;
    }

    private IEnumerator RandomSpawnGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f)));

            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

            if (hits.Count > 0)
            {
                enemySpawn = hits[0].pose.position;
            }
        }
    }

    void OnPlaneAdded(ARPlanesChangedEventArgs eventArgs)
    {
        foreach (ARPlane plane in eventArgs.added)
        {
            Debug.Log("Added plane");
            allPlanes.Add(plane);
        }
    }

    public void EnemyDeath()
    {
        StartCoroutine(SpawnDelayed());

        enDeaths++;
        enCt = Mathf.RoundToInt(Mathf.Clamp(enCt - 1, 0f, Mathf.Infinity));

        enDeathCount.text = "Enemys Destroyed: " + enDeaths + " ct " + enCt;
        enDeathCount.GetComponent<Animator>().SetTrigger("Increment");
    }

    public void PlayerDeath()
    {
        playerLives--;

        frLifeCt.text = "Lives Remaining: " + playerLives;
        frLifeCt.GetComponent<Animator>().SetTrigger("Increment");
    }

    private IEnumerator SpawnDelayed()
    {
        yield return new WaitForSeconds(enCt * 2f);

        if (enDeaths % 3f == 0 && enCt < 3) // (enCt < Mathf.RoundToInt((float) enDeaths / 2f) && enCt < 3f)
            SpawnEnemy();
    }

    private IEnumerator GameOver()
    {
        gameOverStatus = true;
        gameOver.gameObject.SetActive(true);
        gameOver.gameObject.GetComponent<Animator>().SetBool("Game Over", true);

        yield return new WaitForSeconds(5f);
        
        /*
        gameOver.gameObject.GetComponent<Animator>().SetBool("Game Over", false);
        gameOver.gameObject.SetActive(false);
        gameOverStatus = false;

        playerLives = 5;
        enCt = 0;
        enDeaths = 0;
        */

        SceneManager.LoadScene(0);
    }
}

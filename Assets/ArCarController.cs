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
    public Image boomChargeSlider;

    public GameObject boomPanel;
    Animator boomPanelAnim;
    List<GameObject> allEnemyCars = new List<GameObject>();
    float boomChargeTimer;

    GameObject currentEnemy;

    int enCt;
    public int enDeaths;
    bool gameOverStatus;
    
    public int playerLives = 5;

    private GameObject currentCar;

    public ARPlaneManager planeManager;

    ARPlane spawnPlane;

    CarMotorController motor;

    List<ARPlane> allPlanes = new List<ARPlane>();

    public static ArCarController instance;

    bool touched;

    void OnEnable()
    {
        planeManager.planesChanged += OnPlaneAdded;
        instance = this;
        frLifeCt.text = playerLives.ToString();
        enDeathCount.text = enDeaths.ToString();
        boomChargeSlider.fillAmount = 0f; 
        boomPanel.GetComponent<Animator>().SetBool("Boom Charged", false);
        boomPanelAnim = boomPanel.GetComponent<Animator>();
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlaneAdded;
    }

    void Update()
    {
        enDeathCount.text = enDeaths.ToString();

        if (!ScoreTracker.instance)
            Debug.Log("NO TRACKEr");

        if(boomChargeTimer > 4f)
            ChargeTheBoom();

        boomChargeTimer += Time.deltaTime;

        if (playerLives == 0)
        {
            if (!gameOverStatus)
            {
                if (ScoreTracker.instance != null)
                    gameOver.gameObject.GetComponentInChildren<Text>().text = "Game Over!" + "\r\n" + "Your Score: " + ScoreTracker.instance.GetScore() + "\r\n" + "High Score: " + ScoreTracker.instance.GetHighScore();
                else
                    Debug.LogError("No score tracker!");

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
            }

            if (!currentEnemy && allPlanes.Count > 0 && currentCar && spawnPoints.Count > 0)
            {
                currentEnemy = SpawnEnemy();
                enCt++;
            }            
        }
        
    }

    public void StartBoomCharge()
    {
        if (boomChargeSlider.fillAmount > 0.99f)
        {
            ChargeBoom();
            boomChargeTimer = 0f;
        }
    }

    public void EndBoomCharge()
    {
        if (boomForceModifier > 1f) // 1f is minimum
        {
            boomPanelAnim.SetTrigger("Make Boom");
            boomChargeSlider.fillAmount = 0f;
            motor.MakeBoom(boomForceModifier);
            boomForceModifier = 1f;
            motor.UnSuckEnemies();
        }

        boomChargeTimer = 0f;
    }

    void OvertimeBoom()
    {
        boomPanelAnim.SetTrigger("Make Boom");
        boomChargeSlider.fillAmount = 0f;
        boomForceModifier = 1f;
        motor.UnSuckEnemies();
    }

    public void SteerLeft()
    {
        motor.SteerLeft();
        Debug.Log("steer left");
    }

    public void SteerRight()
    {
        motor.SteerRight();
        Debug.Log("steer right");
    }

    public void SteerForward()
    {
        motor.SteerStraight(false);
    }

    public void SteerBack()
    {
        motor.SteerBackwards();
    }

    public void Stop()
    {
        motor.StopCar();
    }

    public void ResetWheels()
    {
        motor.ResetWheels();
        Debug.Log("Reset wheels");
    }

    private GameObject SpawnEnemy()
    {
        Debug.Log("Attempt Spawn");

        Vector3 spawnPos = spawnPoints[Mathf.RoundToInt(Random.Range(0, spawnPoints.Count - 1))];

        //Vector3 innerDirection = spawnPlane.center - spawnPos; // Vector that points from the extremity to the center of the plane

        //Quaternion spawnRot = Quaternion.Euler(innerDirection);

        Quaternion spawnRot = Quaternion.identity;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        newEnemy.GetComponent<EnemyAiCarController>().Init(currentCar.transform);

        Debug.Log("Spawned!!!");

        allEnemyCars.Add(newEnemy);

        return newEnemy;
    }

    List<Vector3> spawnPoints = new List<Vector3>();

    private IEnumerator RandomSpawnGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f); // Just a number

            Ray ray = new Ray(Camera.current.transform.position, Random.insideUnitSphere);

            //Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f)));

            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            raycastManager.Raycast(ray, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

            if (hits.Count > 0)
            {
                spawnPoints.Add(hits[0].pose.position);

                if (spawnPoints.Count > 20)
                {
                    spawnPoints.RemoveAt(0);
                }
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

    public void EnemyDeath(GameObject deadCar)
    {
        enDeaths++;
        enCt--; // = Mathf.RoundToInt(Mathf.Clamp(enCt - 1, 0f, Mathf.Infinity));
        allEnemyCars.Remove(deadCar);

        StartCoroutine(SpawnDelayed());
        
        //enDeathCount.text = "Enemies Destroyed: " + enDeaths + " ct " + enCt;
        enDeathCount.GetComponent<Animator>().SetTrigger("Increment");
    }

    public void PlayerDeath()
    {
        playerLives--;

        frLifeCt.text = playerLives.ToString();
        frLifeCt.GetComponent<Animator>().SetTrigger("Increment");
    }

    private IEnumerator SpawnDelayed()
    {
        if (allEnemyCars.Count < Mathf.RoundToInt((float)enDeaths / 2f) && allEnemyCars.Count < 3f) // (enDeaths % 3f == 0 && enCt < 3)
        {
            if (allEnemyCars.Count < 3)
            {
                enCt++;
                yield return new WaitForSeconds(enCt * 2f);
                
                SpawnEnemy();
            }
        }
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

    public void ChargeTheBoom() // Filling the NOS, essentially
    {
        boomChargeSlider.fillAmount = Mathf.Clamp(boomChargeSlider.fillAmount + Time.deltaTime * 0.1f, 0f, 1f);

        if (boomChargeSlider.fillAmount > 0.99f)
        {
            if (!boomPanel.GetComponent<Animator>().GetBool("Boom Charged"))
            {
                boomPanel.GetComponent<Animator>().SetBool("Boom Charged", true);
            }
        }
        else
        {
            boomPanel.GetComponent<Animator>().SetBool("Boom Charged", false);
        }
    }

    public void DrainTheBoom() // UNUSED
    {
        if (boomChargeSlider.fillAmount < 0.99f)
        {
            boomForceModifier = 1f;
            boomChargeSlider.fillAmount = Mathf.Clamp(boomChargeSlider.fillAmount - Time.deltaTime * 0.66f, 0f, 1f);
        }
    }

    float boomForceModifier = 1f;
    public void ChargeBoom() // Hold fiunger down to charge extra power
    {
        float boomForceModLimit = 4f;

        if (boomForceModifier < boomForceModLimit)
        {
            boomForceModifier = Mathf.Clamp(boomForceModifier + Time.deltaTime * 1.33f, 1f, boomForceModLimit);

            if (boomForceModifier / boomForceModLimit > 0.2f)
            {
                motor.SuckEnemies();
            }
        }
        else
        {
            OvertimeBoom();
        }
    }
}

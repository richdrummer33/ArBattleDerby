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
    public Slider boomChargeSlider;

    public GameObject boomPanel;
    Animator boomPanelAnim;
    List<GameObject> allEnemyCars = new List<GameObject>();

    GameObject currentEnemy;

    int enCt;
    public int enDeaths;
    bool gameOverStatus;
    
    public int playerLives = 5;

    private GameObject currentCar;

    public ARPlaneManager planeManager;

    Vector3 enemySpawn = Vector3.zero;

    ARPlane spawnPlane;

    CarMotorController motor;

    List<ARPlane> allPlanes = new List<ARPlane>();

    public static ArCarController instance;

    bool touched;

    void OnEnable()
    {
        planeManager.planesChanged += OnPlaneAdded;
        instance = this;
        frLifeCt.text = "Lives Remaining: " + playerLives;
        enDeathCount.text = "Enemies Destroyed: " + enDeaths;
        boomChargeSlider.value = 0f;
        boomPanel.GetComponent<Animator>().SetBool("Boom Charged", false);
        boomPanelAnim = boomPanel.GetComponent<Animator>();
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlaneAdded;
    }

    void Update()
    {
        enDeathCount.text = "Enemies Destroyed: " + enDeaths + " ct " + allEnemyCars.Count;

        if (!ScoreTracker.instance)
            Debug.Log("NO TRACKEr");

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

                if (motor != null)
                {
                    var touch = Input.GetTouch(0);

                    if (touch.position.x < Screen.width / 3f)
                    {
                        if (touch.position.y > Screen.height * (1f / 8f) && touch.position.y < Screen.height * 0.55f)
                            motor.SteerLeft();
                        else if (touch.position.y < Screen.height * (1f / 8f))
                            motor.SteerLeftBackward();
                    }
                    else if (touch.position.x > Screen.width * (2f / 3f))
                    {
                        if(touch.position.y > Screen.height * (1f / 8f) && touch.position.y < Screen.height * 0.55f)
                            motor.SteerRight();
                        else if (touch.position.y < Screen.height * (1f / 8f))
                            motor.SteerRightBackward();
                    }
                    else if (touch.position.y > Screen.height * (1f / 8f))
                    {
                        motor.SteerStraight(true);
                    }
                    else
                    {
                        motor.SteerBackwards();
                    }                   
                }
            }
            else if (Input.touchCount > 1 && motor != null)
            { 
                motor.SteerStraight(false);               
            }
            else if (motor)
            {
                motor.StopCar();
            }

            if (!currentEnemy && allPlanes.Count > 0 && currentCar && enemySpawn != Vector3.zero)
            {
                currentEnemy = SpawnEnemy();
                enCt++;
            }

            #region Boom Control

            if (Input.touchCount > 0)
            {
                touched = true;
                foreach (Touch touch in Input.touches)
                {
                    if (boomPanelAnim.GetBool("Boom Charged"))
                    {
                        if (touch.position.x > Screen.width * (2f / 3f) && touch.position.y > Screen.height * 0.55f)
                        {
                            ChargeBoom();
                        }
                    }
                }
            }
            else
            {
                touched = false;
            }

            if (boomForceModifier > 1f && !touched)
            {
                boomPanelAnim.SetTrigger("Make Boom");
                boomChargeSlider.GetComponent<Animator>().speed = 1f;
                boomChargeSlider.GetComponent<Animator>().SetBool("Boom Charged", false);
                boomChargeSlider.value = 0f;
                motor.MakeBoom(boomForceModifier);
                boomForceModifier = 1f;
            }
        }

        #endregion
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

        allEnemyCars.Add(newEnemy);

        return newEnemy;
    }

    
    private IEnumerator RandomSpawnGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // Just a number

            Ray ray = new Ray(Camera.current.transform.position, Random.insideUnitSphere);

            //Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f)));

            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            raycastManager.Raycast(ray, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

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

        frLifeCt.text = "Lives Remaining: " + playerLives;
        frLifeCt.GetComponent<Animator>().SetTrigger("Increment");
    }

    private IEnumerator SpawnDelayed()
    {
        if (allEnemyCars.Count < Mathf.RoundToInt((float)enDeaths / 2f) && allEnemyCars.Count < 3f) // (enDeaths % 3f == 0 && enCt < 3)
        {
            enCt++;
            yield return new WaitForSeconds(enCt * 2f);

            if (allEnemyCars.Count < 3)
            {
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

    public void ChargeTheBoom()
    {
        boomChargeSlider.value = Mathf.Clamp(boomChargeSlider.value + Time.deltaTime * 0.33f, 0f, 1f);

        if (boomChargeSlider.value > 0.99f)
        {
            if (!boomPanel.GetComponent<Animator>().GetBool("Boom Charged"))
            {
                boomChargeSlider.GetComponent<Animator>().SetTrigger("Animate");
                boomPanel.GetComponent<Animator>().SetBool("Boom Charged", true);
            }
        }
        else
        {
            boomPanel.GetComponent<Animator>().SetBool("Boom Charged", false);
        }
    }

    public void DrainTheBoom()
    {
        if (boomChargeSlider.value < 0.99f)
        {
            boomForceModifier = 1f;
            boomChargeSlider.value = Mathf.Clamp(boomChargeSlider.value - Time.deltaTime * 0.66f, 0f, 1f);
        }
    }

    float boomForceModifier = 1f;
    public void ChargeBoom() // Hold fiunger down to charge extra power
    {
        boomForceModifier = Mathf.Clamp(boomForceModifier + Time.deltaTime, 1.33f, 4f);

        boomChargeSlider.GetComponent<Animator>().SetBool("Boom Charged", true);
        boomChargeSlider.GetComponent<Animator>().speed = boomForceModifier / 4f;
    }
}

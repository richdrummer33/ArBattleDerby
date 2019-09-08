using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker instance;
    int lastCt;

    void Start()
    {
        instance = this;

        if (!PlayerPrefs.HasKey("Highscore"))
        {
            PlayerPrefs.SetInt("Highscore", 0);
            Debug.Log("Created Highscore param");

        }
        
        PlayerPrefs.SetInt("CurrentScore", 0);
                
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (ArCarController.instance.enDeaths > PlayerPrefs.GetInt("Highscore"))
        {
            PlayerPrefs.SetInt("Highscore", ArCarController.instance.enDeaths);
            Debug.Log("Highscore reached!");
        }

        if (ArCarController.instance.enDeaths != lastCt)
        {
            PlayerPrefs.SetInt("CurrentScore", ArCarController.instance.enDeaths);
            Debug.Log("Increment score");
            lastCt = ArCarController.instance.enDeaths;
        }
    }

    public int GetHighScore()
    {
        PlayerPrefs.Save();
        return PlayerPrefs.GetInt("Highscore");
    }

    public int GetScore()
    {
        PlayerPrefs.Save();
        return PlayerPrefs.GetInt("CurrentScore");
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    void OnApplicationPause()
    {
        PlayerPrefs.Save();
    }
}

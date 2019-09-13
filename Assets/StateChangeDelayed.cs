using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChangeDelayed : MonoBehaviour
{
    public static StateChangeDelayed instance;

    void Start()
    {
        instance = this;
    }

    public void DelayedStateChange(Animator anim, string animParam, float delay)
    {
        Debug.Log("state chage?");
        StartCoroutine(DelayToSwitch(anim, animParam, delay));
    }

    IEnumerator DelayToSwitch(Animator anim, string animParam, float delay)
    {
        Debug.Log("state chage in 3");
        yield return new WaitForSeconds(3f);
        anim.SetBool(animParam, !anim.GetBool(animParam));
    }
}

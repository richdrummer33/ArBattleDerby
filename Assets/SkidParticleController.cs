using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkidParticleController : MonoBehaviour
{
    private List<WheelCollider> wheels;
    private List<ParticleSystem> particles;

    public TrailRenderer skidMarkPrefab;
    TrailRenderer currentSkidMark;
    List<TrailRenderer> allSkidMarks = new List<TrailRenderer>();

    Dictionary<WheelCollider, ParticleSystem> wheelToParticle = new Dictionary<WheelCollider, ParticleSystem >();
    Dictionary<WheelCollider, TrailRenderer> wheelToSkid = new Dictionary<WheelCollider, TrailRenderer>();
    public int maxSkids = 15;

    public float sideSkidThreshold = 0.05f;
    public float fwdSkidThreshold = 0.5f;
    public float turnSkidMarkThreshold = 0.1f;

    [SerializeField]
    float skidAmt;

    [SerializeField]
    float aveSkid;

    [SerializeField]
    float maxSideSkid;

    int ct;

    void Start()
    {
        wheels = GetComponentsInChildren<WheelCollider>().ToList<WheelCollider>();

        foreach(WheelCollider wheel in wheels)
        {
            wheelToParticle.Add(wheel, wheel.transform.GetComponentInChildren<ParticleSystem>());
        }
        
    }

    float skidTimer;
    
    //test
    void FixedUpdate()
    {
        WheelHit hit;
        ParticleSystem sys;
        TrailRenderer skid;

        foreach (WheelCollider wheel in wheels)
        {
            wheelToParticle.TryGetValue(wheel, out sys);
            bool gotSkid = wheelToSkid.TryGetValue(wheel, out skid);

            if (wheel.GetGroundHit(out hit))
            {
                if (hit.collider.tag == "Ground")
                {
                    float slip = Mathf.Abs(hit.forwardSlip);
                    float sideSlip = Mathf.Abs(hit.sidewaysSlip);

                    skidAmt = slip;
                    aveSkid += skidAmt / (float)ct;

                    if (sideSlip > maxSideSkid)
                    {
                        maxSideSkid = sideSlip;
                    }


                    if (skidTimer > 0.5f)
                    {
                        Debug.Log("newSkidOnTheBlock ??");
                        if (sideSlip > turnSkidMarkThreshold && !gotSkid)
                        {
                            Debug.Log("newSkidOnTheBlock ");
                            TrailRenderer newSkidOnTheBlock = Instantiate(skidMarkPrefab, wheel.transform.position, skidMarkPrefab.transform.rotation, wheel.transform);
                            wheelToSkid.Add(wheel, newSkidOnTheBlock);
                            allSkidMarks.Add(newSkidOnTheBlock);
                        }
                        else
                        {
                            if (gotSkid)
                            {
                                wheelToSkid.Remove(wheel);
                                skid.transform.parent = null;
                            }
                        }

                        if(allSkidMarks.Count > maxSkids && !fading) // Don't hog resources
                        {
                            StartCoroutine(FadeDestroySkids(allSkidMarks[0]));

                            // TrailRenderer oldSkid = allSkidMarks[0];
                            // allSkidMarks.Remove(oldSkid);
                            // Destroy(oldSkid.gameObject);                            
                        }

                        skidTimer = 0f;
                    }

                    if (sideSlip > sideSkidThreshold || slip > fwdSkidThreshold)
                    {
                        sys.Play();
                    }
                    else
                    {
                        sys.Stop();
                    }
                }
            }
            else
            {
                sys.Stop();

                wheelToSkid.Remove(wheel);

                if(skid)
                    skid.transform.parent = null;
            }

            skidTimer += Time.fixedDeltaTime;
            ct++;
        }
    }

    bool fading;

    private IEnumerator FadeDestroySkids(TrailRenderer skidToFade)
    {
        Debug.Log("START Destroying skid " + skidToFade.name);
        fading = true;

        float alphaStep = skidToFade.colorGradient.alphaKeys[0].alpha * 0.05f;

        GradientAlphaKey[] alphaKeys = skidToFade.colorGradient.alphaKeys;
        Gradient newG = new Gradient();

        while (skidToFade.colorGradient.alphaKeys[0].alpha > 0.1)
        {
            for (int i = 0; i < skidToFade.colorGradient.alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha -= alphaStep * Mathf.Clamp(((float) allSkidMarks.Count / (float) maxSkids), 1f, 20f);
                newG.SetKeys(skidToFade.colorGradient.colorKeys, alphaKeys);
                skidToFade.colorGradient = newG;
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("FIN Destroying skid " + skidToFade.name);
        allSkidMarks.Remove(skidToFade);
        Destroy(skidToFade.gameObject);

        fading = false;
    }
}





/*
for (int i = 0; i < oldSkid.colorGradient.alphaKeys.Length; i++)
{
    oldSkid.colorGradient.alphaKeys[i].alpha *= 0.9f;

    if (oldSkid.colorGradient.alphaKeys[0].alpha < 0.05f)
    {
        allSkidMarks.Remove(oldSkid);
        Destroy(oldSkid.gameObject);
    }

    yield return new WaitForSeconds(0.1f);
}
*/

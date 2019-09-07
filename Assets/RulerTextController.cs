using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RulerTextController : MonoBehaviour
{
    public LineRenderer line;
    public TextMeshPro textMesh;
    
    void Update()
    {
        textMesh.text = System.Math.Round(Vector3.Distance(line.GetPosition(0), line.GetPosition(1)), 2) + " m";

        textMesh.transform.position = (line.GetPosition(0) + line.GetPosition(1)) / 2f;

        textMesh.transform.LookAt(Camera.current.transform.position);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Extensions.PickupItem itemType;

    public int ammoCount = 10;

    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 360f * 0.33f);
    }
}

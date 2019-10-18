using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Probably should be "FeatureController" or "SystemController"
public class WeaponController : MonoBehaviour
{
    public int ammoCount = 0;

    public virtual void PickupAmmo(int quantity)
    {
        ammoCount += quantity;
    }
}

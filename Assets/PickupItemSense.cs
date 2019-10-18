using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemSense : MonoBehaviour
{
    public Extensions.PickupItem itemType;

    public WeaponController weaponController;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Item " + other.name + " nada");

        PickupItem item = other.GetComponent<PickupItem>();

        if (item)
        {
            Debug.Log("Item " + item.name + " hit");

            if (item.itemType == itemType)
            {
                weaponController.PickupAmmo(item.ammoCount);

                Destroy(other.gameObject);
            }
        }
    }
}

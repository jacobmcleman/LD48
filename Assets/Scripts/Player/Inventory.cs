using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int gold;
    public int iron;
    public int copper;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Pickup")
        {
            Pickup pick = other.GetComponent<Pickup>();
            if(pick == null)
            {
                Debug.LogWarningFormat("Pickup {0} had tag but not component!", other.name);
                return;
            }
            
            switch(pick.pickupType)
            {
                case Pickup.Type.GoldOre:
                    gold += pick.amount;
                    break;
                default:
                    Debug.LogWarningFormat("You haven't implemented pickup for {0} yet dummy", pick.pickupType);
                    break;
            }

            Destroy(other.gameObject);
        }
    }
}

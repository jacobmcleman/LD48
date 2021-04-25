using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefuelButton : MonoBehaviour
{
    public void TryRefuel()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if(pc.NeedFuel() && FindObjectOfType<Inventory>().TryPurchase(1, 0, 0))
        {
            pc.Refuel();
        }
    }
}

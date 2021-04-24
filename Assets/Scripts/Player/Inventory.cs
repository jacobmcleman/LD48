using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int gold;
    public GameObject goldNuggetPrefab;

    public int iron;
    public int copper;

    private PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!playerController.isDead && other.tag == "Pickup")
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
                case Pickup.Type.AirBubble:
                    playerController.AddAir(pick.amount);
                    break;
                default:
                    Debug.LogWarningFormat("You haven't implemented pickup for {0} yet dummy", pick.pickupType);
                    break;
            }

            pick.OnPickup.Invoke(pick.gameObject);
            Destroy(other.gameObject);
        }
    }

    public void DropAll()
    {
        while(gold > 0)
        {
            GameObject nugget = Instantiate(goldNuggetPrefab, transform.position, Quaternion.identity);
            Pickup nuggetPickup = nugget.GetComponent<Pickup>();
            if(gold - nuggetPickup.amount >= 0) 
            {
                gold -= nuggetPickup.amount;
            }
            else 
            {
                nuggetPickup.amount = gold;
                gold = 0;
            }
        }
    }
}

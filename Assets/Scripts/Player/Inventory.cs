using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int gold;
    public GameObject goldNuggetPrefab;

    public int iron;
    public GameObject ironNuggetPrefab;
    public int copper;
    public GameObject copperNuggetPrefab;

    private PlayerController playerController;

    public AudioSource feedbackAudio;

    public AudioClip[] pickupPops;

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
                case Pickup.Type.IronOre:
                    iron += pick.amount;
                    break;
                case Pickup.Type.CopperOre:
                    copper += pick.amount;
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

            feedbackAudio.PlayOneShot(pickupPops[Random.Range(0, pickupPops.Length)]);
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

        while(copper > 0)
        {
            GameObject nugget = Instantiate(copperNuggetPrefab, transform.position, Quaternion.identity);
            Pickup nuggetPickup = nugget.GetComponent<Pickup>();
            if(copper - nuggetPickup.amount >= 0) 
            {
                copper -= nuggetPickup.amount;
            }
            else 
            {
                nuggetPickup.amount = copper;
                copper = 0;
            }
        }

        while(iron > 0)
        {
            GameObject nugget = Instantiate(ironNuggetPrefab, transform.position, Quaternion.identity);
            Pickup nuggetPickup = nugget.GetComponent<Pickup>();
            if(iron - nuggetPickup.amount >= 0) 
            {
                iron -= nuggetPickup.amount;
            }
            else 
            {
                nuggetPickup.amount = iron;
                iron = 0;
            }
        }
    }
}

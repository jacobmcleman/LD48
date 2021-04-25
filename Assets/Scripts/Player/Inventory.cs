using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public AudioClip[] craftingSounds;
    public AudioClip[] gasFillupSounds;

    public AudioClip[] bubbleSounds;

    public UnityEvent inventoryChanged;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();

        inventoryChanged.Invoke();
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
                    feedbackAudio.PlayOneShot(bubbleSounds[Random.Range(0, bubbleSounds.Length)]);
                    break;
                case Pickup.Type.Fuel:
                    playerController.Refuel();
                    feedbackAudio.PlayOneShot(gasFillupSounds[Random.Range(0, gasFillupSounds.Length)]);
                    break;
                default:
                    Debug.LogWarningFormat("You haven't implemented pickup for {0} yet dummy", pick.pickupType);
                    break;
            }

            pick.OnPickup.Invoke(pick.gameObject);
            Destroy(other.gameObject);

            feedbackAudio.PlayOneShot(pickupPops[Random.Range(0, pickupPops.Length)]);

            inventoryChanged.Invoke();
        }
    }

    public bool TryPurchase(int goldCost, int ironCost, int copperCost)
    {
        if(gold < goldCost || iron < ironCost || copper < copperCost) return false;

        gold -= goldCost;
        iron -= ironCost;
        copper -= copperCost;
        inventoryChanged.Invoke();

        if(goldCost > 1) feedbackAudio.PlayOneShot(craftingSounds[Random.Range(0, craftingSounds.Length)]);
        else feedbackAudio.PlayOneShot(gasFillupSounds[Random.Range(0, gasFillupSounds.Length)]);

        return true;
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

        inventoryChanged.Invoke();
    }
}

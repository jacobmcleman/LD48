
using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    public enum Type 
    {
        GoldOre,
        IronOre,
        CopperOre,
        AirBubble,
        Fuel
    }

    public Type pickupType;

    public int amount;

    public UnityEvent<GameObject> OnPickup;

    private void Awake()
    {
        if(gameObject.tag != "Pickup")
        {
            Debug.LogWarningFormat("PICKUP {0} IS MISSING ITS TAG", gameObject.name);
        }
    }
}

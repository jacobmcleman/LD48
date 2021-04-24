
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum Type 
    {
        GoldOre,
        AirBubble
    }

    public Type pickupType;

    public int amount;

    private void Awake()
    {
        if(gameObject.tag != "Pickup")
        {
            Debug.LogWarningFormat("PICKUP {0} IS MISSING ITS TAG", gameObject.name);
        }
    }
}

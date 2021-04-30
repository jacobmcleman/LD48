
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

    [System.Serializable]
    public struct ItemData
    {
        public Vector3Int position;
        public Type pickupType;
        public int amount;
    }

    private void Awake()
    {
        if(gameObject.tag != "Pickup")
        {
            Debug.LogWarningFormat("PICKUP {0} IS MISSING ITS TAG", gameObject.name);
        }
    }

    public ItemData GetData()
    {
        ItemData data = new ItemData();
        data.position = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        data.pickupType = pickupType;
        data.amount = amount;
        return data;
    }
}


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

    public bool mergeAllowed = true;

    private float maxSize = 4;

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

    public void SetData(ItemData data)
    {
        float curVolume = transform.localScale.x * transform.localScale.x;
        float density = curVolume / amount;
        float desiredVolume = curVolume + (data.amount * density);
        float newSize = Mathf.Sqrt(desiredVolume);
        if(newSize >= maxSize) mergeAllowed = false;
        newSize = Mathf.Clamp(Mathf.Sqrt(desiredVolume), 0, maxSize);
        transform.localScale = new Vector3(newSize, newSize, newSize);

        transform.position = data.position;
        pickupType = data.pickupType;
        amount = data.amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Pickup otherPickup = other.GetComponent<Pickup>();
        if(mergeAllowed && otherPickup && otherPickup.pickupType == pickupType && otherPickup.mergeAllowed)
        {
            otherPickup.mergeAllowed = false;
            amount += otherPickup.amount;

            float curVolume = transform.localScale.x * transform.localScale.x;
            float density = curVolume / amount;
            float desiredVolume = curVolume + (otherPickup.amount * density);
            float newSize = Mathf.Sqrt(desiredVolume);
            if(newSize >= maxSize) mergeAllowed = false;

            newSize = Mathf.Clamp(Mathf.Sqrt(desiredVolume), 0, maxSize);

            transform.localScale = new Vector3(newSize, newSize, newSize);
            
            
            otherPickup.OnPickup.Invoke(other.gameObject);
            Destroy(other.gameObject);
        }
    }
}

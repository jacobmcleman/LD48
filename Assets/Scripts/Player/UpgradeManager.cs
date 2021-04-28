using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public static float DigSpeedIncrease
    {
        get {return Instance.digSpeedIncrease; }
    }

    public static float SadDigIncrease
    {
        get {return Instance.sadDigIncrease; }
    }

    public static float DigRangeIncrease
    {
        get { return Instance.digRangeIncrease; }
    }

    public static float AirRegenerationRateIncrease
    {
        get { return Instance.airRegenIncrease; }
    }

    public static float AirCapacityIncrease
    {
        get { return Instance.airCapIncrease; }
    }

    public static float SwimSpeedIncrease
    {
        get { return Instance.swimSpeedIncrease; }
    }

    public static float DrillFuelIncrease
    {
        get { return Instance.drillFuelIncrease; }
    }

    public enum UpgradeType
    {
        Flippers, AirPropulsion, AirTank, AirIntakes, LongArms, DrillHead, FuelTank, IronHands
    }

    public Dictionary<UpgradeType, string> upgradeDisplayNames;
    public Dictionary<UpgradeType, string> upgradeDescriptions;
    public Dictionary<UpgradeType, Sprite> upgradeIcons;

    public Dictionary<UpgradeType, UpgradeValues> upgradeValues;

    [System.Serializable]
    public struct Upgrade
    {
        public UpgradeType type;

        public int tier;

        public int copperCost;
        public int ironCost;
        public int goldCost;
    }

    [System.Serializable]
    public struct UpgradeValues
    {
        public float digSpeedIncrease;
        public float digRangeIncrease;
         
        public float airRegenIncrease;
        public float airCapIncrease;

        public float swimSpeedIncrease;
        public float drillFuelIncrease;
        public float sadDigIncrease;
    }

    private float digSpeedIncrease = 0;
    private float digRangeIncrease = 0;
        
    private float airRegenIncrease = 0;
    private float airCapIncrease = 0;

    private float swimSpeedIncrease = 0;
    private float drillFuelIncrease = 0;
    private float sadDigIncrease = 0;

    private List<Upgrade> purchasedUpgrades;
    private List<Upgrade> currentShopUpgrades;
    private List<Upgrade> availableUpgrades;

    public Sprite flipperImage;
    public Sprite airJetImage;
    public Sprite airTankImage;
    public Sprite airIntakeImage;
    public Sprite longArmImage;
    public Sprite drillSpeedImage;
    public Sprite drillFuelImage;
    public Sprite sadDigImage;

    private void Awake()
    {
        Instance = this;

        purchasedUpgrades = new List<Upgrade>();
        currentShopUpgrades = new List<Upgrade>();
        availableUpgrades = new List<Upgrade>();

        InitUpgradeDictionaries();

        PopulateStoreWithInitialUpgrades();
    }

    private string SerializeUpgrade(Upgrade u)
    {
        return JsonUtility.ToJson(u);
    }

    private string SerializePurchasedUpgrades()
    {
        return JsonHelper.ToJson(purchasedUpgrades.ToArray(), false);
    }

    private string SerializeAvailableUpgrades()
    {
        return JsonHelper.ToJson(availableUpgrades.ToArray(), false);
    }

    private string SerializeStoreUpgrades()
    {
        return JsonHelper.ToJson(currentShopUpgrades.ToArray(), false);
    }

    private List<Upgrade> DeserializePurchasedUpgrades(string purchasedUpgrades)
    {
        Upgrade[] purchased = JsonHelper.FromJson<Upgrade>(purchasedUpgrades);
        return new List<Upgrade>(purchased);
    }

    private List<Upgrade> DeserializeAvailableUpgrades(string availableUpgrades)
    {
        Upgrade[] available = JsonHelper.FromJson<Upgrade>(availableUpgrades);
        return new List<Upgrade>(available);
    }

    private List<Upgrade> DeserializeShopUpgrades(string shopUpgrades)
    {
        Upgrade[] shop = JsonHelper.FromJson<Upgrade>(shopUpgrades);
        return new List<Upgrade>(shop);
    }

    public string SerializeUpgrades()
    {
        string upgrades = "Purchased: ";
        upgrades += SerializePurchasedUpgrades();
        upgrades += "\nStore: ";
        upgrades += SerializeStoreUpgrades(); 
        upgrades += "\nAvailable: ";
        upgrades += SerializeAvailableUpgrades(); 
        return upgrades;
    }

    public void DeserializeUpgrades(string serialized)
    {
        string[] lines = serialized.Split('\n');

        foreach(string line in lines)
        {
            int splitPoint = line.IndexOf(':');
            string tag = line.Substring(0, splitPoint);
            string data = line.Substring(splitPoint + 1);

            // Debug.LogFormat("{0} -> {1} | {2}", line, tag, data);

            switch(tag)
            {
                case "Purchased":
                    purchasedUpgrades = DeserializePurchasedUpgrades(data);
                    break;
                case "Available":
                    availableUpgrades = DeserializeAvailableUpgrades(data);
                    break;
                case "Store":
                    availableUpgrades = DeserializeShopUpgrades(data);
                    break;
                default:
                    Debug.LogError("Unrecognized upgrade set?!?!");
                    break;
            }
        }
    }

    public List<Upgrade> GetShopUpgrades()
    {
        while(currentShopUpgrades.Count < 4 && availableUpgrades.Count > 0)
        {
            int chosen = Random.Range(0, availableUpgrades.Count);
            currentShopUpgrades.Add(availableUpgrades[chosen]);
            availableUpgrades.RemoveAt(chosen);
        }
        return currentShopUpgrades;
    }

    public void PurchaseUpgrade(int shopIndex)
    {
        Upgrade purchased = currentShopUpgrades[shopIndex];
        currentShopUpgrades.RemoveAt(shopIndex);
        purchasedUpgrades.Add(purchased);

        UpgradeValues values = upgradeValues[purchased.type];

        digSpeedIncrease += values.digSpeedIncrease;
        digRangeIncrease += values.digRangeIncrease;
        airRegenIncrease += values.airRegenIncrease;
        airCapIncrease += values.airCapIncrease;
        swimSpeedIncrease += values.swimSpeedIncrease;
        drillFuelIncrease += values.drillFuelIncrease;
        sadDigIncrease += values.sadDigIncrease;

        int numToGenerate = Random.Range(0, 4) / 2; // 3 / 4 options become 1, 4th is 2
        if(numToGenerate == 0) numToGenerate = 1;
        while(numToGenerate > 0)
        {
            Upgrade nextTier = purchased;
            // If generate 2, make one of them probably cheaper
            nextTier.goldCost = (int)(nextTier.goldCost * Random.Range(1.5f, numToGenerate >= 2 ? 2.0f : 3.0f));
            nextTier.ironCost = (int)(nextTier.ironCost * Random.Range(1.5f, numToGenerate >= 2 ? 2.0f : 3.0f));
            nextTier.copperCost = (int)(nextTier.copperCost * Random.Range(1.5f, numToGenerate >= 2 ? 2.0f : 3.0f));
            nextTier.tier = purchased.tier + 1;
            availableUpgrades.Add(nextTier);
            numToGenerate--;
        }

        string serialized = SerializeUpgrades();
        Debug.Log(serialized);

        DeserializeUpgrades(serialized);
    }

    private void InitUpgradeDictionaries()
    {
        upgradeDescriptions = new Dictionary<UpgradeType, string>();
        upgradeDisplayNames = new Dictionary<UpgradeType, string>();
        upgradeIcons = new Dictionary<UpgradeType, Sprite>();
        upgradeValues = new Dictionary<UpgradeType, UpgradeValues>();

        UpgradeValues flipperValues = new UpgradeValues();
        flipperValues.swimSpeedIncrease = 0.1f;
        upgradeValues.Add(UpgradeType.Flippers, flipperValues);
        upgradeDisplayNames.Add(UpgradeType.Flippers, "Fancy Flippers");
        upgradeDescriptions.Add(UpgradeType.Flippers,  "Foot extensions seem to speed up swimming");
        upgradeIcons.Add(UpgradeType.Flippers, flipperImage);

        UpgradeValues airJetValues = new UpgradeValues();
        airJetValues.airCapIncrease = -5.0f;
        upgradeValues.Add(UpgradeType.AirPropulsion, airJetValues);
        upgradeDisplayNames.Add(UpgradeType.AirPropulsion, "Air Propulsion");
        upgradeDescriptions.Add(UpgradeType.AirPropulsion, "Swim much faster, but uses some of your air");
        upgradeIcons.Add(UpgradeType.AirPropulsion, airJetImage);

        UpgradeValues airTankValues = new UpgradeValues();
        airTankValues.airCapIncrease = 1.0f;
        upgradeValues.Add(UpgradeType.AirTank, airTankValues);
        upgradeDisplayNames.Add(UpgradeType.AirTank, "Air Tank");
        upgradeDescriptions.Add(UpgradeType.AirTank, "Turns out that in this case bigger is better");
        upgradeIcons.Add(UpgradeType.AirTank, airTankImage);

        UpgradeValues airIntakeValues = new UpgradeValues();
        airIntakeValues.airRegenIncrease = 0.3f;
        upgradeValues.Add(UpgradeType.AirIntakes, airIntakeValues);
        upgradeDisplayNames.Add(UpgradeType.AirIntakes, "Air Intakes");
        upgradeDescriptions.Add(UpgradeType.AirIntakes, "Not sure that this is the intended use for this thing, but air go brrr");
        upgradeIcons.Add(UpgradeType.AirIntakes, airIntakeImage);

        UpgradeValues longArmValues = new UpgradeValues();
        longArmValues.digRangeIncrease = 1;
        upgradeValues.Add(UpgradeType.LongArms, longArmValues);
        upgradeDisplayNames.Add(UpgradeType.LongArms, "Long Arms");
        upgradeDescriptions.Add(UpgradeType.LongArms, "Allows you to reach things slightly further away");
        upgradeIcons.Add(UpgradeType.LongArms, longArmImage);

        UpgradeValues digSpeedValues = new UpgradeValues();
        digSpeedValues.digSpeedIncrease = 0.25f;
        upgradeValues.Add(UpgradeType.DrillHead, digSpeedValues);
        upgradeDisplayNames.Add(UpgradeType.DrillHead, "Drill Head Upgrade");
        upgradeDescriptions.Add(UpgradeType.DrillHead, "Chews through material much faster");
        upgradeIcons.Add(UpgradeType.DrillHead, drillSpeedImage);

        UpgradeValues drillFuelValues = new UpgradeValues();
        drillFuelValues.drillFuelIncrease = 5f;
        upgradeValues.Add(UpgradeType.FuelTank, drillFuelValues);
        upgradeDisplayNames.Add(UpgradeType.FuelTank, "Fuel Tank Enhancement");
        upgradeDescriptions.Add(UpgradeType.FuelTank, "Sqeeze more gas in the can, what could go wrong?");
        upgradeIcons.Add(UpgradeType.FuelTank, drillFuelImage);

        UpgradeValues sadDigValues = new UpgradeValues();
        sadDigValues.sadDigIncrease = 0.1f;
        upgradeValues.Add(UpgradeType.IronHands, sadDigValues);
        upgradeDisplayNames.Add(UpgradeType.IronHands, "Iron Hands");
        upgradeDescriptions.Add(UpgradeType.IronHands, "Some might call this a mere shovel, and they're probably right");
        upgradeIcons.Add(UpgradeType.IronHands, sadDigImage);
    }

    private void PopulateStoreWithInitialUpgrades()
    {
        // ---------------------------------------------------------

        Upgrade flippers = new Upgrade();
        flippers.type = UpgradeType.Flippers;
        flippers.tier = 0;
        flippers.copperCost = 5;
        flippers.ironCost = 10;
        flippers.goldCost = 0;
        availableUpgrades.Add(flippers);
        
        // ---------------------------------------------------------

        Upgrade airJets = new Upgrade();
        airJets.type = UpgradeType.AirPropulsion;
        airJets.tier = 0;
        airJets.copperCost = 20;
        airJets.ironCost = 20;
        airJets.goldCost = 0;
        // availableUpgrades.Add(airJets); DISABLED BECAUSE EVIL BUG

        // ---------------------------------------------------------

        Upgrade airTank = new Upgrade();
        airTank.type = UpgradeType.AirTank;
        airTank.tier = 0;
        airTank.copperCost = 10;
        airTank.ironCost = 0;
        airTank.goldCost = 5;
        availableUpgrades.Add(airTank);

        // ---------------------------------------------------------

        Upgrade airIntake = new Upgrade();
        airIntake.type = UpgradeType.AirIntakes;
        airIntake.tier = 0;
        airIntake.copperCost = 8;
        airIntake.ironCost = 0;
        airIntake.goldCost = 4;
        availableUpgrades.Add(airIntake);

        // ---------------------------------------------------------

        Upgrade longArms = new Upgrade();
        longArms.type = UpgradeType.LongArms;
        longArms.tier = 0;
        longArms.copperCost = 10;
        longArms.ironCost = 0;
        longArms.goldCost = 20;
        availableUpgrades.Add(longArms);
        
        // ---------------------------------------------------------

        Upgrade digSpeed = new Upgrade();
        digSpeed.type = UpgradeType.DrillHead;
        digSpeed.tier = 0;
        digSpeed.copperCost = 10;
        digSpeed.ironCost = 0;
        digSpeed.goldCost = 20;
        availableUpgrades.Add(digSpeed);

        // ---------------------------------------------------------

        Upgrade drillFuel = new Upgrade();
        drillFuel.type = UpgradeType.FuelTank;
        drillFuel.tier = 0;
        drillFuel.copperCost = 10;
        drillFuel.ironCost = 0;
        drillFuel.goldCost = 20;
        availableUpgrades.Add(drillFuel);

        // ---------------------------------------------------------

        Upgrade sadDig = new Upgrade();
        sadDig.type = UpgradeType.IronHands;
        sadDig.tier = 0;
        sadDig.copperCost = 0;
        sadDig.ironCost = 10;
        sadDig.goldCost = 3;
        availableUpgrades.Add(sadDig);

        // ---------------------------------------------------------
    }
}

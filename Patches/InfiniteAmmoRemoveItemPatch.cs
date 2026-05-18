using HarmonyLib;

namespace InfiniteAmmoGunz.Patches;

[HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] {typeof(ItemDrop.ItemData), typeof(int)})]
public class InfiniteAmmoRemoveItemPatch
{
    /// <summary>
    /// Prevents infinite ammo from being removed from the inventory.
    /// </summary>
    /// <param name="item">Item</param>
    /// <param name="amount">amount of the item</param>
    /// <param name="__result">If the item has been used</param>
    /// <returns></returns>
    public static bool Prefix(ItemDrop.ItemData item, int amount, ref bool __result)
    {
        if (item == null) return true;
        
        // Check if the item is infinite
        if (item.m_shared.m_name.Contains("infinite"))
        {
            //Plugin.Logger.LogInfo($"firing item name: {item.m_shared.m_name}, type: {item.m_shared.m_itemType} and is marked as infinite");
            __result = true; // set to true to say the item has been used
            return false;
        }
        
        return true;
    }
}
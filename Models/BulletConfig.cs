namespace InfiniteAmmoGunz.Models;

/// <summary>
/// Configuration for a bullet.
/// </summary>
public struct BulletConfig
{
    /// <summary>
    /// Name of the item.
    /// </summary>
    public string ItemName;
    /// <summary>
    /// Prefab name of the prefab to be cloned.
    /// </summary>
    public string PrefabName;
    /// <summary>
    /// Item needed for the recipe.
    /// </summary>
    public string RecipeItem;
}

using System;
using BepInEx;
using BepInEx.Logging;
using Jotunn.Managers;
using Jotunn.Entities;
using System.IO;
using Jotunn.Configs;
using InfiniteAmmoGunz.Models;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace InfiniteAmmoGunz;

[BepInPlugin(ModGuid, ModName, ModVersion)]
[BepInDependency("blacks7ar.GunzNBullets")]
[BepInDependency(Jotunn.Main.ModGuid)]
public class Plugin : BaseUnityPlugin
{
    // Plugin info
    private const string ModGuid = "jawlessjman.InfiniteAmmoGunz";
    public const string ModName = "InfiniteAmmoGunz";
    public const string ModVersion = "1.0.0";
    
    // Config values
    private ConfigEntry<int> _requiredStackCraftSize;
    private ConfigEntry<float> _weight;
    private ConfigEntry<CraftingStationType> _craftingStation;
    
    internal new static ManualLogSource Logger;
    
    private Harmony _harmony;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {ModName}-{ModVersion} is loaded!");
        
        // Load config values
        _requiredStackCraftSize = Config.Bind(
            "General",
            "Bullet Crafting Amount",
            400,
            new ConfigDescription("The required amount of bullets to craft the infinite variant.", new AcceptableValueRange<int>(1, 9999))
            );

        _weight = Config.Bind(
            "General",
            "Infinite Bullet Weight",
            0.1f,
            new ConfigDescription("The weight of the infinite bullets.", new AcceptableValueRange<float>(0.1f, 300f))
        );

        _craftingStation = Config.Bind(
            "General",
            "Infinite Bullet Crafting Station",
            CraftingStationType.Forge,
            new ConfigDescription("The crafting station where the infinite bullets are crafted.")
        );
        
        // Load translations for English
        var assembly = Assembly.GetExecutingAssembly();
        
        const string resourceName = "InfiniteAmmoGunz.Assets.Translations.English.bullets.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            Logger.LogError($"Resource {resourceName} not found. English translations will not be loaded.");
        }
        else
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            Jotunn.Managers.LocalizationManager.Instance.GetLocalization().AddJsonFile("English", json);
        }

        // Load translations for other languages
        LoadTranslations();
        
        // Initialize Harmony
        _harmony = new Harmony(ModGuid);
        _harmony.PatchAll();
        
        // Add function to create infinite ammo bullets
        PrefabManager.OnVanillaPrefabsAvailable += AddInfiniteAmmoBullets;
    }
    
    /// <summary>
    /// Loads translations from the Translations folder.
    /// </summary>
    private void LoadTranslations()
    {
        var root = Path.Combine(
            Path.GetDirectoryName(Info.Location)!,
            "Translations"
        );

        if (!Directory.Exists(root))
        {
            return;
        }
        
        foreach (var file in Directory.GetFiles(root, "*.json", SearchOption.AllDirectories))
        {
            Logger.LogInfo($"Loading translation file: {file}");
            Jotunn.Managers.LocalizationManager.Instance.GetLocalization().AddFileByPath(file);
        }
    }
    
    /// <summary>
    /// Creates infinite ammo bullets.
    /// </summary>
    private void AddInfiniteAmmoBullets()
    {
        // Create infinite ammo bullets from the list up top
        foreach (var bullet in GetBullets())
        {
            CreateInfiniteAmmo(bullet.ItemName, bullet.RecipeItem, bullet.PrefabName);
        }
        
        // Remove the function to create infinite ammo bullets once they are created
        PrefabManager.OnVanillaPrefabsAvailable -= AddInfiniteAmmoBullets;
        Logger.LogInfo($"Infinite Ammo Bullets Loaded!");
    }

    /// <summary>
    /// Creates infinite ammo.
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <param name="recipeItem">Item needed for the recipe</param>
    /// <param name="basePrefName">Base prefab name of the prefab to be cloned</param>
    private void CreateInfiniteAmmo(string itemName, string recipeItem, string basePrefName)
    {
        try
        {
            var prefabName = "BMM_" + itemName.Replace(" ", "_");
            
            // Create infinite ammo config
            var infiniteAmmoConfig = new ItemConfig
            {
                Name = "$bmm_bullet_" +
                       itemName.Replace("Infinite ", "").Replace(" Bullets", "").Replace(" ", "_").ToLower() +
                       "_infinite",
                Description = "$item_" + itemName.Replace(" ", "_").ToLower() + "_description",
                CraftingStation = GetCraftingStation(),
                Weight = _weight.Value,
                StackSize = 1
            };
            
            // Add infinite ammo recipe
            infiniteAmmoConfig.AddRequirement(recipeItem, _requiredStackCraftSize.Value);
            
            // Add infinite ammo to the game
            var infiniteAmmo = new CustomItem(prefabName, basePrefName, infiniteAmmoConfig);
            Jotunn.Managers.ItemManager.Instance.AddItem(infiniteAmmo);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Infinite Ammo Failed: {itemName}\n{ex}");
        }
    }

    /// <summary>
    /// Loads the bullets from the JSON file.
    /// </summary>
    /// <returns>List of the bullets</returns>
    private BulletConfig[] GetBullets()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "InfiniteAmmoGunz.Assets.Data.bulletData.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            Logger.LogError($"Resource {resourceName} not found. No bullets will be loaded.");
            return [];
        }

        using var reader = new StreamReader(stream);
        
        var json = reader.ReadToEnd();
        var bullets = JsonConvert.DeserializeObject<BulletConfig[]>(json);
        return bullets ?? [];
    }
    
    /// <summary>
    /// Gets the crafting station based on the config value.
    /// </summary>
    /// <returns>Crafting station value</returns>
    private string GetCraftingStation()
    {
        return _craftingStation.Value switch
        {
            CraftingStationType.Workbench => CraftingStations.Workbench,
            CraftingStationType.Forge => CraftingStations.Forge,
            CraftingStationType.BlackForge => CraftingStations.BlackForge,
            CraftingStationType.ArtisanTable => CraftingStations.ArtisanTable,
            CraftingStationType.GaldrTable => CraftingStations.GaldrTable,
            CraftingStationType.Cauldron => CraftingStations.Cauldron,
            CraftingStationType.Stonecutter => CraftingStations.Stonecutter,
            CraftingStationType.FoodPreparationTable => CraftingStations.FoodPreparationTable,
            CraftingStationType.MeadKetill => CraftingStations.MeadKetill,
            CraftingStationType.None => CraftingStations.None,
            _ => CraftingStations.Forge
        };
    }
}
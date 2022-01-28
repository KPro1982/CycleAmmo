// CycleAmmo
// a Valheim mod skeleton using J�tunn
// 
// File:    CycleAmmo.cs
// Project: CycleAmmo

using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace CycleAmmo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class CycleAmmo : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.CycleAmmo";
        public const string PluginName = "CycleAmmo";
        public const string PluginVersion = "0.0.1";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
        
        
        // Variable BepInEx Shortcut backed by a config
        private ConfigEntry<KeyboardShortcut> ShortcutConfig;
        private ButtonConfig ShortcutButton;
        private ConfigEntry<bool> ShowSelection;
        
        

        private void Awake()
        {
            // Jotunn comes with MonoMod Detours enabled for hooking Valheim's code
            // https://github.com/MonoMod/MonoMod
            On.FejdStartup.Awake += FejdStartup_Awake;
            
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("CycleAmmo has landed");
            
            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html
            
            CreateConfigValues();
            AddInputs();
        }

        private void FejdStartup_Awake(On.FejdStartup.orig_Awake orig, FejdStartup self)
        {
            // This code runs before Valheim's FejdStartup.Awake
            Jotunn.Logger.LogInfo("FejdStartup is going to awake");

            // Call this method so the original game method is invoked
            orig(self);

            // This code runs after Valheim's FejdStartup.Awake
            Jotunn.Logger.LogInfo("FejdStartup has awoken");
        }

        private void AddInputs()
        {
            ShortcutButton = new ButtonConfig
            {
                Name = "CycleAmmo",
                ShortcutConfig = ShortcutConfig,
                HintToken = "Cycle Ammo"
            };
            InputManager.Instance.AddButton(PluginGUID, ShortcutButton);
        }
        
        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            ShortcutConfig = Config.Bind("Cycle Ammo Config", "Keycodes with modifiers",
                new KeyboardShortcut(KeyCode.C, KeyCode.LeftControl),
                new ConfigDescription("Cycle Ammo Key"));
            
            ShowSelection = Config.Bind("Cycle Ammo Config", "showmessage", true,
                new ConfigDescription("Show message indicating equipped ammo type"));

        }

        
        private void Update()
        {
            if (ShortcutButton != null)
            {
                if (ZInput.GetButtonDown(ShortcutButton.Name))
                {
                    DoCycleAmmo();
                }
            }
            else
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Cycle Ammo: Null Error");
            }
        }
        
        private void DoCycleAmmo()
        {
            
            Humanoid character = Player.m_localPlayer;
            var currentWeapon = character.GetCurrentWeapon();
            var currentAmmo = character.GetAmmoItem();
            List<ItemDrop.ItemData> availableAmmo = new List<ItemDrop.ItemData>();
           
      
            if (!string.IsNullOrEmpty(currentWeapon.m_shared.m_ammoType))
            {
                if (currentAmmo == null)
                {
                    currentAmmo = character.GetInventory().GetAmmoItem(currentWeapon.m_shared.m_ammoType);
                }
                else
                {
                    ItemDrop.ItemData nextAmmo = character.GetInventory().GetAmmoItem(currentWeapon.m_shared.m_ammoType);
                    
                    foreach (ItemDrop.ItemData inventoryItem in character.GetInventory().GetAllItems())
                    {
                        if (inventoryItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo )
                        {
                            availableAmmo.Add(inventoryItem);
                        }
                    }

                    if (availableAmmo.Count > 1)
                    {
                        int curIndex = availableAmmo.IndexOf(currentAmmo);
                        if (curIndex + 1 < availableAmmo.Count)
                        {
                            character.EquipItem(availableAmmo[curIndex + 1]);
                        }
                        else
                        {
                            character.EquipItem(availableAmmo[0]);
                        }
                    }

                    if (ShowSelection.Value)
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"{character.GetAmmoItem().m_shared.m_name} selected");
                    }
                    
                }
                

            }
           
        }
    }
}


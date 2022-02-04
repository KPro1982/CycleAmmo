using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace CycleAmmo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class CycleAmmo : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.CycleAmmo";
        public const string PluginName = "CycleAmmo";
        public const string PluginVersion = "1.3.0";


        // Variable BepInEx Shortcut backed by a config
        private ConfigEntry<KeyboardShortcut> ShortcutConfig;
        private ConfigEntry<bool> ShowSelection;
        private List<ItemDrop.ItemData> ammoTypesFound;
        private Humanoid curPlayer;


        private void Awake()
        {
            CreateConfigValues();
            ammoTypesFound = new List<ItemDrop.ItemData>();
        }


        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            ShortcutConfig = Config.Bind("Cycle Ammo Config", "Keycodes with modifiers",
                new KeyboardShortcut(KeyCode.G));

            ShowSelection = Config.Bind("Cycle Ammo Config", "showmessage", true);
        }


        private void Update()
        {
            if (ZInput.instance != null)
            {
                if (ShortcutConfig != null)
                {
                    if (ShortcutConfig.Value.IsDown())
                    {
                        DoCycleAmmo();
                    }
                }
                else
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        "Cycle Ammo: Null Error");
                }
            }
        }

        private void DoCycleAmmo()
        {
            curPlayer = Player.m_localPlayer;


            if (TestForValidConditions())
            {
                curPlayer.EquipItem(GetNextAmmoItem());

                if (ShowSelection.Value)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"{curPlayer.GetAmmoItem().m_shared.m_name} selected");
                }
            }
        }


        private bool TestForValidConditions()
        {
            if (string.IsNullOrEmpty(curPlayer.GetCurrentWeapon().m_shared.m_ammoType))
            {
                if (ShowSelection.Value)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"No bow is selected");
                }

                return false;
            }


            var ammoStacks = CountUniqueAmmoStacks();

            if (ammoStacks == 0)
            {
                if (ShowSelection.Value)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"You are out of ammo");
                }

                return false;
            }

            if (ammoStacks == 1)
            {
                if (ShowSelection.Value)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"You only have one type of ammo");
                }

                return false;
            }

            return true;
        }


        private int CountUniqueAmmoStacks()
        {
            int ammoStacks = 0;
            List<string> duplicateAmmoStacks = new List<string>();

            foreach (ItemDrop.ItemData itemData in curPlayer.GetInventory().m_inventory)
            {
                if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo)
                {
                    if (!duplicateAmmoStacks.Contains(itemData.m_shared.m_name))
                    {
                        duplicateAmmoStacks.Add(itemData.m_shared.m_name);
                        ammoStacks += 1;
                    }
                }
            }

            return ammoStacks;
        }


        public ItemDrop.ItemData GetNextAmmoItem()
        {
            var currentWeapon = curPlayer.GetCurrentWeapon();
            var currentAmmo = curPlayer.GetAmmoItem();
            List<string> duplicateAmmoStacks = new List<string>();

            if (currentAmmo == null)
            {
                return curPlayer.GetInventory().GetAmmoItem(currentWeapon.m_shared.m_ammoType);
            }


            int curNum = CalcNum(curPlayer.GetAmmoItem());
            int num = -1;

            ItemDrop.ItemData itemData = null;
            foreach (ItemDrop.ItemData itemData2 in curPlayer.GetInventory().m_inventory)
            {
                if ((itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo ||
                     itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable) &&
                    itemData2.m_shared.m_ammoType == currentWeapon.m_shared.m_ammoType)
                {
                    if (!duplicateAmmoStacks.Contains(itemData2.m_shared.m_name))
                    {
                        duplicateAmmoStacks.Add(itemData2.m_shared.m_name);
                        int num2 = CalcNum(itemData2);
                        if (num2 > curNum && num == -1)
                        {
                            num = num2;
                            itemData = itemData2;
                        }
                        else if (num2 < num && num2 > curNum)
                        {
                            num = num2;
                            itemData = itemData2;
                        }
                    }
                }
            }

            if (num == -1)
            {
                return curPlayer.GetInventory()
                    .GetAmmoItem(currentWeapon.m_shared
                        .m_ammoType); // vanilla method returns the farthest left
            }
            else
            {
                return itemData;
            }
        }

        public int CalcNum(ItemDrop.ItemData ammoItem)
        {
            return ammoItem.m_gridPos.y * Player.m_localPlayer.GetInventory().m_width +
                   ammoItem.m_gridPos.x;
        }
    }
}
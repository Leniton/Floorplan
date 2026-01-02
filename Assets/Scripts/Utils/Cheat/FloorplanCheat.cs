using Cheat;
using System;
using System.Collections;
using System.Collections.Generic;
using AddressableAsyncInstances;
using UnityEngine;
using Util;

namespace Floorplan.Cheat
{
    public static class FloorplanCheat
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ListenCheat()
        {
            CheatConsole.OnSetupDone += Setup;
        }

        private static void Setup()
        {
            CheatConsole.RegisterCommand("p", _ => GameObject.FindObjectOfType<MainMenu>()?.StartRun());
            CheatConsole.RegisterCommand("set", OnSetCommand);
            CheatConsole.RegisterCommand("give", OnGiveCommand);
            CheatConsole.RegisterCommand("copy", OnCopyCommand);
            CheatConsole.RegisterCommand("point", OnPointCommand);
            CheatConsole.RegisterCommand("house", OnHouseCommand);
            CheatConsole.RegisterCommand("create", OnCreateCommand);
        }

        private static List<string> setTypes = new()
        {
            "key",
            "coin",
            "step",
            "dice",
        };

        private static List<string> giveTypes = new() 
        {
            "key",
            "coin",
            "step",
            "dice",
            "wall",
            "ckey",
            "hammer",
        };

        private static List<string> copyTypes = new()
        {
            "this",
            "up",
            "down",
            "left",
            "right",
        };
        
        private static List<string> houseTypes = new()
        {
            "point",
            "coin",
        };

        private static void OnSetCommand(string[] parameters)
        {
            GetKeyValuePair(parameters, setTypes, out var type, out var amount);
            if (string.IsNullOrEmpty(type)) return;
            SetCommand(type, amount);
        }
        private static void SetCommand(string type, int? setAmount)
        {
            int amount = setAmount ?? 0;
            switch (type)
            {
                case "key":
                    Player.ChangeKeys(amount - Player.keys);
                    break;
                case "coin":
                    Player.ChangeCoins(amount - Player.coins);
                    break;
                case "step":
                    Player.ChangeSteps(amount - Player.steps);
                    break;
                case "dice":
                    Player.dices = amount;
                    break;
            }
        }

        private static void OnGiveCommand(string[] parameters)
        {
            GetKeyValuePair(parameters, giveTypes, out var type, out var amount);
            if (string.IsNullOrEmpty(type)) return;
            RoomCategory? category = null;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (category.HasValue) break;
                category = ParseCategory(parameters[i]);
            }
            GiveCommand(type, amount ?? 1, category);
        }

        private static void GiveCommand(string type, int amount, RoomCategory? category)
        {
            switch (type)
            {
                case "key":
                    Player.ChangeKeys(amount);
                    break;
                case "coin":
                    Player.ChangeCoins(amount);
                    break;
                case "step":
                    Player.ChangeSteps(amount);
                    break;
                case "dice":
                    Player.dices += amount;
                    break;
                case "wall":
                    for (int i = 0; i < amount; i++)
                        new CategoryWallpaper(category).PickUp();
                    break;
                case "ckey":
                    for (int i = 0; i < amount; i++)
                        new CategoryKey(category).PickUp();
                    break;
                case "hammer":
                    for (int i = 0; i < amount; i++)
                        new SledgeHammer().PickUp();
                    break;
            }
        }

        private static void OnCopyCommand(string[] parameters)
        {
            GetKeyValuePair(parameters, copyTypes, out var type, out var amount);
            if (string.IsNullOrEmpty(type)) type = "this";
            CopyRoomCommand(type, amount ?? 1);
        }
        private static void CopyRoomCommand(string type, int amount)
        {
            switch (type)
            {
                case "this":
                    CreateCopies(Helpers.CurrentRoom());
                    break;
                case "up":
                    GameManager.roomDict.TryGetValue(GridManager.instance.currentPosition + Vector2Int.up, out var room);
                    CreateCopies(room);
                    break;
                case "down":
                    GameManager.roomDict.TryGetValue(GridManager.instance.currentPosition + Vector2Int.down, out room);
                    CreateCopies(room);
                    break;
                case "left":
                    GameManager.roomDict.TryGetValue(GridManager.instance.currentPosition + Vector2Int.left, out room);
                    CreateCopies(room);
                    break;
                case "right":
                    GameManager.roomDict.TryGetValue(GridManager.instance.currentPosition + Vector2Int.right, out room);
                    CreateCopies(room);
                    break;
            }
            void CreateCopies(Room room)
            {
                if (room == null) return;
                for (int i = 0; i < amount; i++)
                    GameManager.DraftPool?.Add(room.CreateInstance(Vector2Int.up));
            }
        }

        private static void OnPointCommand(string[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!int.TryParse(parameters[i], out var result)) continue;
                PointsManager.AddPoints(result);
                return;
            }
        }
        
        private static void OnHouseCommand(string[] parameters)
        {
            GetKeyValuePair(parameters, houseTypes, out var type, out var amount);
            if (string.IsNullOrEmpty(type)) type = houseTypes[0];
            int value = amount ?? 1;
            switch (type)
            {
                case "point":
                    HouseStatsWindow.OnCheckBonus += PointBonus;
                    break;
                case "coin":
                    HouseStatsWindow.OnCheckBonus += CoinBonus;
                    break;
            }
            return;

            void PointBonus(SequenceManager sequence) =>
                sequence.Add(new SequenceManager().Add(HouseStatsWindow.PointBonusSequence("Cheat", value))
                    .Add(HouseStatsWindow.delaySequence));
            void CoinBonus(SequenceManager sequence) =>
                sequence.Add(new SequenceManager().Add(HouseStatsWindow.CoinBonusSequence("Cheat", value))
                    .Add(HouseStatsWindow.delaySequence));
        }

        private static void OnCreateCommand(string[] parameters)
        {
            var key = string.Empty;
            int? paramValue = null;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!paramValue.HasValue && int.TryParse(parameters[i], out var result))
                    paramValue = result;
                else if (string.IsNullOrEmpty(key))
                    key = parameters[i];
            }

            int amount = paramValue ?? 1;
            if (string.IsNullOrEmpty(key))
            {
                CreateRoom(Helpers.CreateSpareRoom());
                return;
            }

            Debug.Log($"looking for room: {key}");
            try { AAAsset<Room>.LoadAsset(key, CreateRoom); }
            catch (Exception e) { Debug.LogWarning($"Room not found\n\n{e.Message}"); }
            void CreateRoom(Room room)
            {
                if (room == null) return;
                for (int i = 0; i < amount; i++)
                    GameManager.DraftPool?.Add(room.CreateInstance(Vector2Int.up));
            }
        }
        
        private static RoomCategory? ParseCategory(string category)
        {
            switch (category)
            {
                case "curse": return RoomCategory.CursedRoom;
                case "fancy": return RoomCategory.FancyRoom;
                case "rest": return RoomCategory.RestRoom;
                case "hall": return RoomCategory.Hallway;
                case "item": return RoomCategory.StorageRoom;
                case "shop": return RoomCategory.Shop;
                case "myst": return RoomCategory.MysteryRoom;
                case "blank": return RoomCategory.Blank;
            }
            return null;
        }

        private static void GetKeyValuePair(string[] parameters, List<string> keyLookUp, out string key, out int? value)
        {
            key = string.Empty;
            value = null;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!value.HasValue && int.TryParse(parameters[i], out var result))
                    value = result;
                else if (string.IsNullOrEmpty(key) && keyLookUp.Contains(parameters[i]))
                    key = parameters[i];
            }
        }
    }
}
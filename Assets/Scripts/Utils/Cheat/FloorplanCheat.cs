using Cheat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            CheatConsole.OnCommandSubmit += ParseCommand;
        }

        /// <summary>
        /// Commands are separated in:
        /// Command code
        /// Parameters
        /// </summary>
        private static void ParseCommand(string command)
        {
            string[] commandParams = command.Split(' ');
            switch (commandParams[0])
            {
                case "give":
                    if (commandParams.Length < 3) return;
                    GiveCommand(commandParams[1], commandParams[2]);
                    break;
                case "set":
                    if (commandParams.Length < 3) return;
                    SetCommand(commandParams[1], commandParams[2]);
                    break;
                case "copy":
                    CopyRoomCommand(commandParams.GetParam(1), commandParams.GetParam(2));
                    break;
                case "item":
                    ItemCommand(commandParams[1], commandParams.GetParam(2));
                    break;
            }
        }

        private static void GiveCommand(string type, string amount)
        {
            switch (type)
            {
                case "key":
                    ValueMethod(amount, Player.ChangeKeys);
                    break;
                case "coin":
                    ValueMethod(amount, Player.ChangeCoins);
                    break;
                case "step":
                    ValueMethod(amount, Player.ChangeSteps);
                    break;
                case "dice":
                    ValueMethod(amount, d => Player.dices += d);
                    break;
            }
        }
        private static void SetCommand(string type, string amount)
        {
            switch (type)
            {
                case "key":
                    ValueMethod(amount, d => Player.keys = d);
                    break;
                case "coin":
                    ValueMethod(amount, d => Player.coins = d);
                    break;
                case "step":
                    ValueMethod(amount, d => Player.steps = d);
                    break;
                case "dice":
                    ValueMethod(amount, d => Player.dices = d);
                    break;
            }
        }
        private static void CopyRoomCommand(string type, string amount)
        {
            if (string.IsNullOrEmpty(type)) type = "this";
            if (string.IsNullOrEmpty(amount)) amount = "1";
            Room targetRoom = null;
            switch (type)
            {
                case "this":
                    targetRoom = Helpers.CurrentRoom();
                    ValueMethod(amount,CreateCopies);
                    break;
            }
            void CreateCopies(int amount)
            {
                if (targetRoom == null) return;
                for (int i = 0; i < amount; i++)
                    GameManager.DraftPool?.Add(targetRoom.CreateInstance(Vector2Int.up));
            }
        }
        private static void ItemCommand(string type, string amount)
        {
            if (string.IsNullOrEmpty(amount)) amount = "1";
            Func<Item> item = null;
            if (type.StartsWith("wall"))
            {
                string[] wallParam = type.Split('-');
                var category = ParseCategory(wallParam.GetParam(1));
                item += () => new CategoryWallpaper(category);
                ValueMethod(amount, GiveItem);
            }
            else if (type.StartsWith("key"))
            {
                string[] keyParam = type.Split('-');
                var category = ParseCategory(keyParam.GetParam(1));
                item += () => new CategoryKey(category);
                ValueMethod(amount, GiveItem);
            }
            else if(type == "hammer")
            {
                item += () => new SledgeHammer();
                ValueMethod(amount, GiveItem);
            }

            void GiveItem(int quantity)
            {
                if(item == null) return;
                for (int i = 0; i < quantity; i++)
                    item.Invoke().PickUp();
            }
        }

        private static string GetParam(this string[] collection, int id)
        {
            if (collection is { Length: 0 }) return string.Empty;
            if (id < 0 || id >= collection.Length) return string.Empty;
            return collection[id];
        }

        private static void ValueMethod(string valueString, Action<int> giveAction)
        {
            if (!int.TryParse(valueString, out var amount)) return;
            giveAction?.Invoke(amount);
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
    }
}
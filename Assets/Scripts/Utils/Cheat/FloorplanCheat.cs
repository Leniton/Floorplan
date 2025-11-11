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
                case "g":
                    if (commandParams.Length < 3) return;
                    GiveCommand(commandParams[1], commandParams[2]);
                    break;
                case "s":
                    if (commandParams.Length < 3) return;
                    SetCommand(commandParams[1], commandParams[2]);
                    break;
            }
        }

        private static void GiveCommand(string type, string amount)
        {
            switch (type)
            {
                case "k":
                    ValueMethod(amount, Player.ChangeKeys);
                    break;
                case "c":
                    ValueMethod(amount, Player.ChangeCoins);
                    break;
                case "s":
                    ValueMethod(amount, Player.ChangeSteps);
                    break;
                case "d":
                    ValueMethod(amount, d => Player.dices += d);
                    break;
                default:
                    return;
            }
        }
        private static void SetCommand(string type, string amount)
        {
            switch (type)
            {
                case "k":
                    ValueMethod(amount, d => Player.keys = d);
                    break;
                case "c":
                    ValueMethod(amount, d => Player.coins = d);
                    break;
                case "s":
                    ValueMethod(amount, d => Player.steps = d);
                    break;
                case "d":
                    ValueMethod(amount, d => Player.dices = d);
                    break;
                default:
                    return;
            }
        }

        private static void ValueMethod(string valueString,Action<int> giveAction)
        {
            if (!int.TryParse(valueString, out var amount)) return;
            giveAction?.Invoke(amount);
        }
    }
}
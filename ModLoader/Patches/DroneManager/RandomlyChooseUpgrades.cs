using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ModLoader.Modding;
using UnityEngine;
using Random = System.Random;

namespace ModLoader.Patches.DroneManager
{
    [HarmonyPatch(typeof(global::DroneManager))]
    [HarmonyPatch("RandomlyChooseUpgrades", typeof(List<Drone>), typeof(int), typeof(int), typeof(bool), typeof(bool),
        typeof(Random))]
    public class RandomlyChooseUpgrades
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
                if (codes[i].opcode.Equals(OpCodes.Ldc_I4_S))
                {
                    Debug.Log($"found possible injection point {codes[i]}");
                    if (codes[i + 1].opcode.Equals(OpCodes.Callvirt))
                    {
                        Debug.Log(codes[i + 1].ToString());
                        codes[i].operand = ModUpgradeManager.Manager.GetUpgradeCount();
                    }
                }

            return codes.AsEnumerable();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using DSMFramework.Modding;
using HarmonyLib;
using UnityEngine;
using Random = System.Random;

namespace DSMFramework.Patches.DroneManager
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
                if (Matches(codes, i))
                { 
                    Debug.logger.Log("[DSMF] Patching RandomlyChooseUpgrades");
                    Patch(codes, i);
                }

            return codes.AsEnumerable();
        }

        private static bool Matches(List<CodeInstruction> code, int index)
        {
            if (index + 2 < code.Count && index > 5)
            {
                if (code[index].opcode.Equals(OpCodes.Ldc_I4_S) &&
                    code[index + 1].opcode.Equals(OpCodes.Callvirt))
                    return true;
            }

            return false;
        }

        public static void Patch(List<CodeInstruction> code, int i)
        {
            // Replace rng first param from 1 to 0 (original ldc.i4.1)
            code[i - 1] = new CodeInstruction(OpCodes.Ldc_I4_0);
            // Calls "DroneUpgradeFactory.UpgradeDefinitions.Count" and leaves the return value as second argument
            {
                code[i] = new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(global::DroneUpgradeFactory), "get_UpgradeDefinitions"));
                code.Insert(i+1,new CodeInstruction(OpCodes.Callvirt, 
                    AccessTools.DeclaredMethod(typeof(List<DroneUpgradeDefinition>),"get_Count")));
            }
            
            //Inserts the following code "num3 = (int)DroneUpgradeFactory.UpgradeDefinitions[num3].Type"
            var toInsert = new List<CodeInstruction>();
            toInsert.Add(new CodeInstruction(OpCodes.Call,
                AccessTools.DeclaredMethod(typeof(global::DroneUpgradeFactory), "get_UpgradeDefinitions")));
            toInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, (short) 15)); //num3 index is 15
            toInsert.Add(new CodeInstruction(OpCodes.Callvirt, 
                AccessTools.DeclaredMethod(typeof(List<DroneUpgradeDefinition>),"get_Item", new []{typeof(int)})));
            toInsert.Add(new CodeInstruction(OpCodes.Callvirt,
                AccessTools.DeclaredMethod(typeof(DroneUpgradeDefinition),"get_Type")));
            toInsert.Add(new CodeInstruction(OpCodes.Stloc_S, (short) 15)); // and store the module type
            
            // Walk backwards to insert our new code after "num3" is already stored into the local field index
            for (var j = toInsert.Count - 1; j >= 0; j--)
            {
                code.Insert(i+4, toInsert[j]);
            }
        }
    }
}
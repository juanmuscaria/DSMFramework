using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            // Convert into a linked list for fast search and modification.
            // Converting to linked list is as fast as a for each.
            var codes = new LinkedList<CodeInstruction>(instructions);
            for (var node = codes.First; node != null; node = node.Next)
                if (Matches(node))
                {
                    // Found instruction set match, patch it.
                    Plugin.LOGGER.LogInfo("Patching RandomlyChooseUpgrades");
                    Patch(node);
                    break;
                }

            return codes.AsEnumerable(); // Linked list is already enumerable, no real performance lost here
        }

        private static bool Matches(LinkedListNode<CodeInstruction> node)
        {
            // Matches the following instruction set
            // ldc.i4.1
            // ldc.i4.s     22 // 0x16  << Current node must be here
            // callvirt     instance int32 [mscorlib]System.Random::Next(int32, int32)
            return node.Value.opcode.Equals(OpCodes.Ldc_I4_S) &&
                   node.Next != null && node.Next.Value.opcode.Equals(OpCodes.Callvirt) &&
                   ((MethodInfo) node.Next.Value.operand).Name.Equals("Next") &&
                   node.Previous != null && node.Previous.Value.opcode.Equals(OpCodes.Ldc_I4_1);
        }


        // On .net 3.5 "!" (null-forgiving) operator is not available, just ignore all the warnings for my own sanity.
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void Patch(LinkedListNode<CodeInstruction> node)
        {
            try
            {
                // Our current node ldc.i4.s 22
                
                {
                    // Replace rng first param from 1 to 0 (original ldc.i4.1)
                    node.Previous.Value = new CodeInstruction(OpCodes.Ldc_I4_0);
                    
                    // Calls "DroneUpgradeFactory.UpgradeDefinitions.Count" and leaves the return value as second argument
                    node.Value = new CodeInstruction(OpCodes.Call,
                        AccessTools.DeclaredMethod(typeof(global::DroneUpgradeFactory), "get_UpgradeDefinitions"));
                    node.List.AddAfter(node, new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.DeclaredMethod(typeof(List<DroneUpgradeDefinition>), "get_Count")));
                }
                
                // Shift the current node to the end of the last instruction set (stloc.s num3)
                node = node.Next.Next.Next;
                
                //Inserts the following code "num3 = (int)DroneUpgradeFactory.UpgradeDefinitions[num3].Type"
                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.DeclaredMethod(typeof(global::DroneUpgradeFactory), "get_UpgradeDefinitions")),
                    new CodeInstruction(OpCodes.Ldloc_S,
                        node.Value.operand), // node.Value.operand is the local index for num3
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.DeclaredMethod(typeof(List<DroneUpgradeDefinition>), "get_Item",
                            new[] {typeof(int)})),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.DeclaredMethod(typeof(DroneUpgradeDefinition), "get_Type")),
                    new CodeInstruction(OpCodes.Stloc_S, node.Value.operand) // and store the module type
                };

                // Walk backwards to insert our new code after "num3" is already stored into the local field index (stloc.s num3)
                for (var j = toInsert.Count - 1; j >= 0; j--)
                    // Linked list AddAfter is O(1) operation, no performance impact by doing this.
                    node.List.AddAfter(node, toInsert[j]);
            }
            catch (NullReferenceException e)
            {
                // This code block should be unreachable, but better handle any possible exception and make debugging easier if someday it is reached.
                Plugin.LOGGER.LogInfo("Unable to patch, unexpected null element in the linked list");
                Debug.LogException(e);
                throw;
            }
        }
    }
}
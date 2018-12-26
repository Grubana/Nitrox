using System;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class Constructable_Deconstruct_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct");

        public static readonly OpCode CONSTRUCTION_COMPLETE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object CONSTRUCTION_COMPLETE_INJECTION_OPERAND = TARGET_CLASS.GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Constructable __instance)
        {
            if (!__instance._constructed && __instance.constructedAmount > 0)
            {
              //  Log.InGame("Deconstruction changed " + __instance.gameObject.GetGuid()+ " " +__instance.constructedAmount);
                NitroxServiceLocator.LocateService<Building>().ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            }

            return true;
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount <= 0f)
            {
                Log.InGame("Deconstruction completed " + __instance.gameObject.GetGuid());
                NitroxServiceLocator.LocateService<Building>().DeconstructionComplete(__instance.gameObject);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

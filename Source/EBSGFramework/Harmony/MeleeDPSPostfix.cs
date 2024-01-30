using HarmonyLib;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    [HarmonyPatch(typeof(StatWorker_MeleeDPS), "GetMeleeDamage")]
    public class MeleeDPSPostfix
    {
        [HarmonyPostfix]
        public static float Postfix(float __result, StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is Pawn pawn)
            {
                __result *= pawn.GetStatValue(EBSGDefOf.EBSG_OutgoingDamageFactor);
            }
            return __result;
        }
    }
}

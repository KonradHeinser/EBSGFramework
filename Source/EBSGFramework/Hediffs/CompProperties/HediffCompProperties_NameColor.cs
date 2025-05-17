using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_NameColor : HediffCompProperties
    {
        public Color color;

        public HediffCompProperties_NameColor() 
        {
            compClass = typeof(HediffComp_NameColor);
        }
    }
}

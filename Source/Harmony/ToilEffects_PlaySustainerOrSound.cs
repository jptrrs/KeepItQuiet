using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace KeepItQuiet
{
    //[HarmonyPatch(typeof(ToilEffects), nameof(ToilEffects.PlaySustainerOrSound), new Type[] { typeof(Toil), typeof(Func<SoundDef>) })]
    class ToilEffects_PlaySustainerOrSound
    {
        //public static SoundDef currentSound;
        public static RecipeDef currentRecipe;

        public static void Prefix(Toil toil, Func<SoundDef> soundDefGetter)
        {
            //SoundDef soundDef = soundDefGetter.Invoke();
            //if (soundDef != null && soundDef.sustain) currentSound = soundDef;
            RecipeDef recipeDef = toil.GetActor().CurJob.RecipeDef;
            if (recipeDef != null) currentRecipe = recipeDef;
        }

        public static void Postfix()
        {
            //currentSound = null;
            currentRecipe = null;
        }
    }
}

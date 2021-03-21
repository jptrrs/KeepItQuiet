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
using System.Reflection;

namespace KeepItQuiet
{
    //[HarmonyPatch(typeof(ToilEffects), nameof(ToilEffects.PlaySustainerOrSound), new Type[] { typeof(Toil), typeof(Func<SoundDef>) })]
    class ToilEffects_PlaySustainerOrSound
    {
        private static FieldInfo actorInfo = AccessTools.Field(typeof(Toil), "actor");
        private static MethodInfo GetActorInfo = AccessTools.Method(typeof(Toil), "GetActor");
        public static Pawn currentPawn;
        public static RecipeDef currentRecipe;

        public static void Postfix(Toil __result)
        {
            Pawn actor = /*__result.GetActor();*/ (Pawn)GetActorInfo.Invoke(__result, new object[] { });
            if (actor != null)
            { 
                currentPawn = actor;
                RecipeDef recipeDef = actor.CurJob.RecipeDef;
                if (recipeDef != null) currentRecipe = recipeDef;
            }
            string pawnTest = currentPawn is object ? currentPawn.Label : "no pawn";
            string recipeTest = currentRecipe is object ? currentRecipe.label : "no recipe";
            string test = __result != null ? "ok":"bad";
            Log.Message($"ToilEffect: result is {__result}, {pawnTest}, {recipeTest}, stackTrace: {new StackTrace().GetFrames().ToStringSafeEnumerable()}");
        }

        public static void Postfix()
        {
            currentRecipe = null;
            currentPawn = null;
        }
    }
}

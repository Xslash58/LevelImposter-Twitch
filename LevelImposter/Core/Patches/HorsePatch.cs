﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;
using PowerTools;

namespace LevelImposter.Core
{
    /*
     *      Uncomment to disable the Horse Mod
     */
    /*
    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class HorsePatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
    */
}
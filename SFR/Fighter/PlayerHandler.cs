using HarmonyLib;
using SFD;
using SFD.Weapons;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Objects;
using SFR.Weapons.Rifles;

namespace SFR.Fighter;

/// <summary>
///     Class containing all patches regarding players and their state
///     <list type="bullet|table">
///         <listheader>
///             <term>State</term>
///             <description>Player state for server &amp; client sync</description>
///         </listheader>
///         <item>
///             <term>0</term>
///             <description>Standing on ground</description>
///         </item>
///         <item>
///             <term>1</term>
///             <description>In air</description>
///         </item>
///         <item>
///             <term>2</term>
///             <description>Running</description>
///         </item>
///         <item>
///             <term>3</term>
///             <description>Sprinting</description>
///         </item>
///         <item>
///             <term>4</term>
///             <description>Falling</description>
///         </item>
///         <item>
///             <term>5</term>
///             <description>Crouching</description>
///         </item>
///         <item>
///             <term>6</term>
///             <description>Rolling</description>
///         </item>
///         <item>
///             <term>7</term>
///             <description>Diving</description>
///         </item>
///         <item>
///             <term>8</term>
///             <description>Laying on ground</description>
///         </item>
///         <item>
///             <term>9</term>
///             <description>Melee hit</description>
///         </item>
///         <item>
///             <term>10</term>
///             <description>Dazed</description>
///         </item>
///         <item>
///             <term>11</term>
///             <description>Dead</description>
///         </item>
///         <item>
///             <term>12</term>
///             <description>Staggering</description>
///         </item>
///         <item>
///             <term>13</term>
///             <description>Clouds disabled</description>
///         </item>
///         <item>
///             <term>14</term>
///             <description>Taking cover</description>
///         </item>
///         <item>
///             <term>15</term>
///             <description>Removed</description>
///         </item>
///         <item>
///             <term>16</term>
///             <description>Force kneel</description>
///         </item>
///         <item>
///             <term>17</term>
///             <description>Climbing</description>
///         </item>
///         <item>
///             <term>18</term>
///             <description>Throw charging</description>
///         </item>
///         <item>
///             <term>19</term>
///             <description>Chat active</description>
///         </item>
///         <item>
///             <term>20</term>
///             <description>Reloading</description>
///         </item>
///         <item>
///             <term>21</term>
///             <description>Reloading toggle</description>
///         </item>
///         <item>
///             <term>22</term>
///             <description>Walking</description>
///         </item>
///         <item>
///             <term>23</term>
///             <description>Ledge grabbing turn</description>
///         </item>
///         <item>
///             <term>24</term>
///             <description>Full landing</description>
///         </item>
///         <item>
///             <term>25</term>
///             <description>Burned</description>
///         </item>
///         <item>
///             <term>26</term>
///             <description>Burning inferno</description>
///         </item>
///         <item>
///             <term>27</term>
///             <description>Input enabled</description>
///         </item>
///         <item>
///             <term>28</term>
///             <description>Death kneeling</description>
///         </item>
///         <item>
///             <term>29</term>
///             <description>Can recover from fall</description>
///         </item>
///         <item>
///             <term>30</term>
///             <description>Recovery rolling</description>
///         </item>
///         <item>
///             <term>31</term>
///             <description>Throwing mode</description>
///         </item>
///         <item>
///             <term>32</term>
///             <description>Grab telegraphing</description>
///         </item>
///         <item>
///             <term>33</term>
///             <description>Grab charging</description>
///         </item>
///         <item>
///             <term>34</term>
///             <description>Grab attacking</description>
///         </item>
///         <item>
///             <term>35</term>
///             <description>Grab kicking</description>
///         </item>
///         <item>
///             <term>36</term>
///             <description>Grab throwing</description>
///         </item>
///         <item>
///             <term>37</term>
///             <description>Grab immunity</description>
///         </item>
///         <item>
///             <term>38</term>
///             <description>Exiting throwing mode</description>
///         </item>
///         <item>
///             <term>40</term>
///             <description>Extra melee state chainsaw active</description>
///         </item>
///         <item>
///             <term>41</term>
///             <description>Strength boost preparing</description>
///         </item>
///         <item>
///             <term>42</term>
///             <description>Strength boost active</description>
///         </item>
///         <item>
///             <term>43</term>
///             <description>Speed boost preparing</description>
///         </item>
///         <item>
///             <term>44</term>
///             <description>Speed boost active</description>
///         </item>
///         <item>
///             <term>45</term>
///             <description>Input mode</description>
///         </item>
///         <item>
///             <term>SFR: 0</term>
///             <description>Rage boost</description>
///         </item>
///         <item>
///             <term>SFR: 1</term>
///             <description>Preparing jetpack</description>
///         </item>
///         <item>
///             <term>SFR: 2</term>
///             <description>Jetpack type</description>
///         </item>
///         <item>
///             <term>SFR: 3</term>
///             <description>Jetpack fuel</description>
///         </item>
///     </list>
/// </summary>
[HarmonyPatch]
internal static class PlayerHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CheckLedgeGrab))]
    private static bool CheckLedgeGrab(Player __instance)
    {
        if (__instance.VirtualKeyboardLastMovement is PlayerMovement.Right or PlayerMovement.Left)
        {
            var data = __instance.LedgeGrabData?.ObjectData;
            if (data is ObjectDoor { IsOpen: true })
            {
                __instance.ClearLedgeGrab();
                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Movement), MethodType.Setter)]
    private static bool KeepMoving(PlayerMovement value, Player __instance)
    {
        var extendedPlayer = __instance.GetExtension();
        if (extendedPlayer.AdrenalineBoost && __instance.CurrentAction is PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2
                                           && __instance.Movement != PlayerMovement.Idle && value == PlayerMovement.Idle)
        {
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.HandlePlayerKeyHoldingPreUpdateEvent))]
    private static void UpdateKeyEvent(Player __instance)
    {
        var extendedPlayer = __instance.GetExtension();
        if (__instance.CurrentAction is PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2 && __instance.Movement != PlayerMovement.Idle && extendedPlayer.AdrenalineBoost)
        {
            if (__instance.VirtualKeyboard.PressingKey(2, true) || __instance.VirtualKeyboard.PressingKey(3, true))
            {
                __instance.CurrentTargetSpeed.X = __instance.LastDirectionX * __instance.GetTopSpeed();
            }
            else
            {
                __instance.m_movement = PlayerMovement.Idle;
            }
        }
    }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(Player), nameof(Player.CheckAttackKey))]
    // private static void CheckAttackKey(bool onKeyEvent, Player __instance, out float __state)
    // {
    //     __state = __instance.CurrentSpeed.X;
    // }
    //
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(Player), nameof(Player.CheckAttackKey))]
    // private static void CheckAttackKeyPost(bool onKeyEvent, Player __instance, float __state)
    // {
    //     var extendedPlayer = __instance.GetExtension();
    //     if (__instance.CurrentAction is PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2 && __instance.Movement != PlayerMovement.Idle && extendedPlayer.AdrenalineBoost)
    //     {
    //         __instance.CurrentSpeed.X = __instance.Movement == PlayerMovement.Left ? -__instance.GetTopSpeed() : __instance.GetTopSpeed();
    //     }
    // }


    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(Player), nameof(Player.CheckAttackKey))]
    // private static bool CheckAttackKey(bool onKeyEvent, Player __instance, ref bool __result)
    // {
    //     if (!__instance.CanAttack())
    //     {
    //         __result = false;
    //         return false;
    //     }
    //
    //     var extendedPlayer = __instance.GetExtension();
    //
    //     __instance.GetPlayerKeyActions(out bool playerAction, out bool serverAction);
    //     if (__instance.InThrowingMode)
    //     {
    //         if (serverAction)
    //         {
    //             if (__instance.CanStartThrowCharge())
    //             {
    //                 __instance.DisableFireWhileHoldingAttackKey = true;
    //                 __instance.BeginChargeThrow();
    //                 __instance.TimeSequence.DisableQueuedKey(4);
    //                 __result = true;
    //                 return false;
    //             }
    //
    //             if (__instance.ThrowableIsActivated)
    //             {
    //                 __result = true;
    //                 return false;
    //             }
    //         }
    //         else
    //         {
    //             if (__instance.CanStartThrowCharge())
    //             {
    //                 __instance.DisableFireWhileHoldingAttackKey = true;
    //                 __instance.TimeSequence.DisableQueuedKey(4);
    //                 var currentAction = __instance.CurrentAction;
    //                 if (currentAction != PlayerAction.Idle)
    //                 {
    //                     if (currentAction == PlayerAction.ManualAim)
    //                     {
    //                         if (!__instance.InAir)
    //                         {
    //                             __instance.Sprinting = false;
    //                         }
    //                     }
    //                 }
    //                 else if (!__instance.InAir)
    //                 {
    //                     __instance.Sprinting = false;
    //                 }
    //
    //                 __result = true;
    //                 return false;
    //             }
    //
    //             if (__instance.ThrowableIsActivated)
    //             {
    //                 __result = true;
    //                 return false;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         if (playerAction)
    //         {
    //             switch (__instance.CurrentWeaponDrawn)
    //             {
    //                 case WeaponItemType.Handgun:
    //                 case WeaponItemType.Rifle:
    //                     if (!__instance.Reloading)
    //                     {
    //                         var currentAction2 = __instance.CurrentAction;
    //                         if (currentAction2 == PlayerAction.Idle)
    //                         {
    //                             __instance.PreparingHipFire = __instance.GetAverageLatencyTime() + 50f;
    //                         }
    //                     }
    //
    //                     break;
    //             }
    //         }
    //
    //         if (serverAction)
    //         {
    //             switch (__instance.CurrentWeaponDrawn)
    //             {
    //                 case WeaponItemType.NONE:
    //                 case WeaponItemType.Melee:
    //                 {
    //                     var currentMeleeWeaponInUse = __instance.GetCurrentMeleeWeaponInUse();
    //                     if (currentMeleeWeaponInUse != null && currentMeleeWeaponInUse.Properties.Handling == MeleeHandlingType.Custom && currentMeleeWeaponInUse.CustomHandlingOnAttackKey(__instance, onKeyEvent))
    //                     {
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     if (!onKeyEvent)
    //                     {
    //                         __result = false;
    //                         return false;
    //                     }
    //
    //                     var currentAction3 = __instance.CurrentAction;
    //                     if (currentAction3 != PlayerAction.Idle)
    //                     {
    //                         switch (currentAction3)
    //                         {
    //                             case PlayerAction.MeleeAttack1:
    //                                 if (__instance.GetSubAnimations()[1].IsLastFrame() && __instance.GetSubAnimations()[1].GetFrameActiveTime() > __instance.MinimumMeleeHitFrameTime)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                     __instance.CurrentAction = PlayerAction.MeleeAttack2;
    //                                     __instance.MinimumMeleeHitFrameTime = 85f;
    //
    //                                     if (!extendedPlayer.AdrenalineBoost)
    //                                     {
    //                                         __instance.Movement = PlayerMovement.Idle;
    //                                         __instance.CurrentTargetSpeed.X = 0f;
    //                                     }
    //                                     
    //                                     __instance.ImportantUpdate = true;
    //                                     __instance.ClientMustInitiateMovement(false);
    //                                     __result = true;
    //                                     return false;
    //                                 }
    //
    //                                 break;
    //                             case PlayerAction.MeleeAttack2:
    //                                 if (__instance.GetSubAnimations()[1].IsLastFrame() && __instance.GetSubAnimations()[1].GetFrameActiveTime() > __instance.MinimumMeleeHitFrameTime)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                     __instance.CurrentAction = PlayerAction.MeleeAttack3;
    //                                     __instance.MinimumMeleeHitFrameTime = 85f;
    //
    //                                     if (!extendedPlayer.AdrenalineBoost)
    //                                     {
    //                                         __instance.Movement = PlayerMovement.Idle;
    //                                         __instance.CurrentTargetSpeed.X = 0f;
    //                                     }
    //                                     
    //                                     __instance.ImportantUpdate = true;
    //                                     __instance.ClientMustInitiateMovement(false);
    //                                     __result = true;
    //                                     return false;
    //                                 }
    //
    //                                 break;
    //                         }
    //
    //                         __result = false;
    //                         return false;
    //                     }
    //
    //                     if (__instance.InAir)
    //                     {
    //                         if (!__instance.TimeSequence.PostDropClimbAttackCooldown)
    //                         {
    //                             __instance.JumpAttack();
    //                         }
    //                     }
    //                     else
    //                     {
    //                         __instance.MeleeAttack1();
    //
    //                         if (!extendedPlayer.AdrenalineBoost)
    //                         {
    //                             __instance.Movement = PlayerMovement.Idle;
    //                             __instance.CurrentTargetSpeed.X = 0f;
    //                         }
    //
    //                         __instance.ImportantUpdate = true;
    //                     }
    //
    //                     __result = true;
    //                     return false;
    //                 }
    //                 case WeaponItemType.Handgun:
    //                 case WeaponItemType.Rifle:
    //                     if (__instance.Reloading)
    //                     {
    //                         __instance.StopReloadWeapon();
    //                         __result = false;
    //                         return false;
    //                     }
    //
    //                     if (!__instance.Reloading)
    //                     {
    //                         switch (__instance.CurrentAction)
    //                         {
    //                             case PlayerAction.Idle:
    //                                 if (!__instance.InAir)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                 }
    //
    //                                 __instance.CurrentAction = PlayerAction.HipFire;
    //                                 __result = true;
    //                                 return false;
    //                             case PlayerAction.HipFire:
    //                             case PlayerAction.ManualAim:
    //                                 if (!__instance.InAir)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                 }
    //
    //                                 if (__instance.FireSequence.CanShootInHipFire && __instance.CanFireWeapon())
    //                                 {
    //                                     __instance.FireWeapon();
    //                                 }
    //                                 else
    //                                 {
    //                                     __instance.FireSequence.HipFireAimTime = 250f;
    //                                 }
    //
    //                                 __result = true;
    //                                 return false;
    //                         }
    //                     }
    //
    //                     break;
    //                 case WeaponItemType.Thrown:
    //                     if (__instance.CanStartThrowCharge())
    //                     {
    //                         __instance.DisableFireWhileHoldingAttackKey = true;
    //                         __instance.BeginChargeThrow();
    //                         __instance.TimeSequence.DisableQueuedKey(4);
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     if (__instance.ThrowableIsActivated)
    //                     {
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     break;
    //             }
    //         }
    //         else
    //         {
    //             switch (__instance.CurrentWeaponDrawn)
    //             {
    //                 case WeaponItemType.NONE:
    //                 case WeaponItemType.Melee:
    //                 {
    //                     var currentMeleeWeaponInUse2 = __instance.GetCurrentMeleeWeaponInUse();
    //                     if (currentMeleeWeaponInUse2 != null && currentMeleeWeaponInUse2.Properties.Handling == MeleeHandlingType.Custom)
    //                     {
    //                         currentMeleeWeaponInUse2.CustomHandlingOnAttackKey(__instance, onKeyEvent);
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     if (!onKeyEvent)
    //                     {
    //                         __result = false;
    //                         return false;
    //                     }
    //
    //                     var currentAction4 = __instance.CurrentAction;
    //                     if (currentAction4 == PlayerAction.Idle)
    //                     {
    //                         if (__instance.InAir)
    //                         {
    //                             if (!__instance.TimeSequence.PostDropClimbAttackCooldown)
    //                             {
    //                                 __instance.JumpAttack();
    //                             }
    //                         }
    //                         else
    //                         {
    //                             __instance.MeleeAttack1();
    //                         }
    //
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     __result = false;
    //                     return false;
    //                 }
    //                 case WeaponItemType.Handgun:
    //                 case WeaponItemType.Rifle:
    //                     if (!__instance.Reloading)
    //                     {
    //                         switch (__instance.CurrentAction)
    //                         {
    //                             case PlayerAction.Idle:
    //                                 if (!__instance.InAir)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                 }
    //
    //                                 __result = true;
    //                                 return false;
    //                             case PlayerAction.HipFire:
    //                             case PlayerAction.ManualAim:
    //                                 if (!__instance.InAir)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                 }
    //
    //                                 __result = true;
    //                                 return false;
    //                         }
    //                     }
    //
    //                     break;
    //                 case WeaponItemType.Thrown:
    //                     if (__instance.CanStartThrowCharge())
    //                     {
    //                         __instance.DisableFireWhileHoldingAttackKey = true;
    //                         __instance.TimeSequence.DisableQueuedKey(4);
    //                         var currentAction5 = __instance.CurrentAction;
    //                         if (currentAction5 != PlayerAction.Idle)
    //                         {
    //                             if (currentAction5 == PlayerAction.ManualAim)
    //                             {
    //                                 if (!__instance.InAir)
    //                                 {
    //                                     __instance.Sprinting = false;
    //                                 }
    //                             }
    //                         }
    //                         else if (!__instance.InAir)
    //                         {
    //                             __instance.Sprinting = false;
    //                         }
    //
    //                         __instance.TimeSequence.DisableQueuedKey(4);
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     if (__instance.ThrowableIsActivated)
    //                     {
    //                         __result = true;
    //                         return false;
    //                     }
    //
    //                     break;
    //             }
    //         }
    //     }
    //
    //     __result = false;
    //     return false;
    // }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanActivateSprint))]
    private static bool ActivateSprint(Player __instance, ref bool __result)
    {
        if (__instance is { CurrentWeaponDrawn: WeaponItemType.Rifle, CurrentRifleWeapon: Barrett, StrengthBoostActive: false })
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanKick))]
    private static bool CanKick(float timeOffset, Player __instance, ref bool __result)
    {
        var extendedPlayer = __instance.GetExtension();
        // __result = (__instance.CurrentAction == PlayerAction.Idle || (__instance.CurrentAction == PlayerAction.HipFire && __instance.ThrowableIsActivated)) && (!__instance.Diving || extendedPlayer.AdrenalineBoost) && !__instance.Rolling && !__instance.Falling && !__instance.Climbing && __instance.PreparingHipFire <= 0f && __instance.FireSequence.KickCooldownTimer <= 800f + timeOffset && !__instance.TimeSequence.PostDropClimbAttackCooldown && !__instance.StrengthBoostPreparing && !__instance.SpeedBoostPreparing;
        __result = (__instance.CurrentAction == PlayerAction.Idle || (__instance.CurrentAction == PlayerAction.HipFire && __instance.ThrowableIsActivated)) && !((__instance.Diving && !extendedPlayer.AdrenalineBoost) || (extendedPlayer.GenericJetpack != null && extendedPlayer.GenericJetpack.State != JetpackState.Idling) || __instance.Rolling || __instance.Falling || __instance.Climbing || __instance.PreparingHipFire > 0f || __instance.FireSequence.KickCooldownTimer > 800f + timeOffset || __instance.TimeSequence.PostDropClimbAttackCooldown || __instance.StrengthBoostPreparing || __instance.SpeedBoostPreparing);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanAttack))]
    private static bool CanAttack(Player __instance, ref bool __result)
    {
        var extendedPlayer = __instance.GetExtension();

        __result = !((__instance.Diving && !extendedPlayer.AdrenalineBoost) || (extendedPlayer.GenericJetpack != null && extendedPlayer.GenericJetpack.State != JetpackState.Idling) || __instance.Rolling || __instance.Climbing || __instance.LedgeGrabbing || __instance.ThrowingModeToggleQueued || __instance.ClimbingClient || __instance.StrengthBoostPreparing || __instance.SpeedBoostPreparing);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.TimeSequence.Update))]
    private static void UpdateTimeSequence(float ms, float realMs, Player __instance)
    {
        var extendedPlayer = __instance.GetExtension();
        if (extendedPlayer.AdrenalineBoost)
        {
            extendedPlayer.Time.AdrenalineBoost -= ms;
            if (!extendedPlayer.AdrenalineBoost || __instance.IsDead)
            {
                extendedPlayer.DisableAdrenalineBoost();
            }
        }
    }
}
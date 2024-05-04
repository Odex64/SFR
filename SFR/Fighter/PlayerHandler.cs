using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Weapons;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Misc;
using SFR.Objects;
using SFR.Weapons;
using SFR.Weapons.Rifles;
using Math = System.Math;
using Player = SFD.Player;

namespace SFR.Fighter;

/// <summary>
/// Class containing all patches regarding players and their state
/// <list type="bullet|table">
/// <listheader>
/// <term>State</term>
/// <description>PlayerExt state for server &amp; client sync</description>
/// </listheader>
/// <item>
/// <term>0</term>
/// <description>Standing on ground</description>
/// </item>
/// <item>
/// <term>1</term>
/// <description>In air</description>
/// </item>
/// <item>
/// <term>2</term>
/// <description>Running</description>
/// </item>
/// <item>
/// <term>3</term>
/// <description>Sprinting</description>
/// </item>
/// <item>
/// <term>4</term>
/// <description>Falling</description>
/// </item>
/// <item>
/// <term>5</term>
/// <description>Crouching</description>
/// </item>
/// <item>
/// <term>6</term>
/// <description>Rolling</description>
/// </item>
/// <item>
/// <term>7</term>
/// <description>Diving</description>
/// </item>
/// <item>
/// <term>8</term>
/// <description>Laying on ground</description>
/// </item>
/// <item>
/// <term>9</term>
/// <description>Melee hit</description>
/// </item>
/// <item>
/// <term>10</term>
/// <description>Dazed</description>
/// </item>
/// <item>
/// <term>11</term>
/// <description>Dead</description>
/// </item>
/// <item>
/// <term>12</term>
/// <description>Staggering</description>
/// </item>
/// <item>
/// <term>13</term>
/// <description>Clouds disabled</description>
/// </item>
/// <item>
/// <term>14</term>
/// <description>Taking cover</description>
/// </item>
/// <item>
/// <term>15</term>
/// <description>Removed</description>
/// </item>
/// <item>
/// <term>16</term>
/// <description>Force kneel</description>
/// </item>
/// <item>
/// <term>17</term>
/// <description>Climbing</description>
/// </item>
/// <item>
/// <term>18</term>
/// <description>Throw charging</description>
/// </item>
/// <item>
/// <term>19</term>
/// <description>Chat active</description>
/// </item>
/// <item>
/// <term>20</term>
/// <description>Reloading</description>
/// </item>
/// <item>
/// <term>21</term>
/// <description>Reloading toggle</description>
/// </item>
/// <item>
/// <term>22</term>
/// <description>Walking</description>
/// </item>
/// <item>
/// <term>23</term>
/// <description>Ledge grabbing turn</description>
/// </item>
/// <item>
/// <term>24</term>
/// <description>Full landing</description>
/// </item>
/// <item>
/// <term>25</term>
/// <description>Burned</description>
/// </item>
/// <item>
/// <term>26</term>
/// <description>Burning inferno</description>
/// </item>
/// <item>
/// <term>27</term>
/// <description>Input enabled</description>
/// </item>
/// <item>
/// <term>28</term>
/// <description>Death kneeling</description>
/// </item>
/// <item>
/// <term>29</term>
/// <description>Can recover from fall</description>
/// </item>
/// <item>
/// <term>30</term>
/// <description>Recovery rolling</description>
/// </item>
/// <item>
/// <term>31</term>
/// <description>Throwing mode</description>
/// </item>
/// <item>
/// <term>32</term>
/// <description>Grab telegraphing</description>
/// </item>
/// <item>
/// <term>33</term>
/// <description>Grab charging</description>
/// </item>
/// <item>
/// <term>34</term>
/// <description>Grab attacking</description>
/// </item>
/// <item>
/// <term>35</term>
/// <description>Grab kicking</description>
/// </item>
/// <item>
/// <term>36</term>
/// <description>Grab throwing</description>
/// </item>
/// <item>
/// <term>37</term>
/// <description>Grab immunity</description>
/// </item>
/// <item>
/// <term>38</term>
/// <description>Exiting throwing mode</description>
/// </item>
/// <item>
/// <term>40</term>
/// <description>Extra melee state chainsaw active</description>
/// </item>
/// <item>
/// <term>41</term>
/// <description>Strength boost preparing</description>
/// </item>
/// <item>
/// <term>42</term>
/// <description>Strength boost active</description>
/// </item>
/// <item>
/// <term>43</term>
/// <description>Speed boost preparing</description>
/// </item>
/// <item>
/// <term>44</term>
/// <description>Speed boost active</description>
/// </item>
/// <item>
/// <term>45</term>
/// <description>Input mode</description>
/// </item>
/// <item>
/// <term>SFR: 0</term>
/// <description>Rage boost</description>
/// </item>
/// <item>
/// <term>SFR: 1</term>
/// <description>Preparing jetpack</description>
/// </item>
/// <item>
/// <term>SFR: 2</term>
/// <description>Jetpack type</description>
/// </item>
/// <item>
/// <term>SFR: 3</term>
/// <description>Jetpack fuel</description>
/// </item>
/// </list>
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    private static void Update(float ms, float realMs, Player __instance)
    {
        object weapon = __instance.GetCurrentWeapon();
        if (weapon is IExtendedWeapon wep)
        {
            wep.Update(__instance, ms, realMs);
        }

        var extendedPlayer = __instance.GetExtension();
        extendedPlayer.GenericJetpack?.Update(ms, extendedPlayer);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Movement), MethodType.Setter)]
    private static bool KeepMoving(PlayerMovement value, Player __instance)
    {
        var extendedPlayer = __instance.GetExtension();
        return !extendedPlayer.AdrenalineBoost || __instance.CurrentAction is not PlayerAction.MeleeAttack1 and not PlayerAction.MeleeAttack2
                                           || __instance.Movement == PlayerMovement.Idle || value != PlayerMovement.Idle;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Jump), [])]
    private static bool CanJump(Player __instance)
    {
        var extendedModifiers = __instance.m_modifiers.GetExtension();
        if (extendedModifiers.JumpHeightModifier != 1f)
        {
            float jumpForce = 7.55f * extendedModifiers.JumpHeightModifier;
            __instance.Jump(jumpForce);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.TestProjectileHit))]
    private static void CanProjectileHit(ref bool __result, Player __instance)
    {
        if (!__result)
        {
            return;
        }

        var extendedModifiers = __instance.m_modifiers.GetExtension();
        if (extendedModifiers.BulletDodgeChance > 0 && Globals.Random.NextDouble() < extendedModifiers.BulletDodgeChance)
        {
            __result = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.HandlePlayerKeyHoldingPreUpdateEvent))]
    private static void UpdateKeyEvent(Player __instance)
    {
        var extendedPlayer = __instance.GetExtension();
        if (__instance.CurrentAction is PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2 && __instance.Movement != PlayerMovement.Idle && extendedPlayer.AdrenalineBoost)
        {
            // __instance.CurrentTargetSpeed.X = __instance.LastDirectionX * __instance.GetTopSpeed();
            if (__instance.VirtualKeyboard.PressingKey(2, true) || __instance.VirtualKeyboard.PressingKey(3, true))
            {
                __instance.CurrentTargetSpeed.X = __instance.LastDirectionX * __instance.GetTopSpeed();
            }
            else
            {
                if (__instance.GameOwner != GameOwnerEnum.Client)
                {
                    __instance.m_movement = PlayerMovement.Idle;
                }
            }
        }
    }

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
    [HarmonyPatch(typeof(Player), nameof(Player.DrawDistanceArrow))]
    private static bool DrawAdditionTeamDistanceArrow(Area boundsArrows, Player __instance)
    {
        if ((__instance.DrawStatusInfo & Player.DrawStatusInfoFlags.Name) != Player.DrawStatusInfoFlags.Name)
        {
            return false;
        }

        var playerPosition = __instance.Position + new Vector2(0f, 8f);
        float distanceFromEdge = boundsArrows.GetDistanceFromEdge(playerPosition);
        if (distanceFromEdge > 10f)
        {
            int num = (int)Math.Round(distanceFromEdge / 12f);
            int num2 = 0;
            int num3 = 0;
            if (boundsArrows.Right < playerPosition.X)
            {
                num2 = 1;
            }
            else if (boundsArrows.Left > playerPosition.X)
            {
                num2 = -1;
            }

            if (boundsArrows.Top < playerPosition.Y)
            {
                num3 = 1;
            }
            else if (boundsArrows.Bottom > playerPosition.Y)
            {
                num3 = -1;
            }

            if (num2 != 0 || num3 != 0)
            {
                var vector2 = playerPosition;
                if (vector2.X > boundsArrows.Right)
                {
                    vector2.X = boundsArrows.Right;
                }
                else if (vector2.X < boundsArrows.Left)
                {
                    vector2.X = boundsArrows.Left;
                }

                if (vector2.Y > boundsArrows.Top)
                {
                    vector2.Y = boundsArrows.Top;
                }
                else if (vector2.Y < boundsArrows.Bottom)
                {
                    vector2.Y = boundsArrows.Bottom;
                }

                vector2 = Camera.ConvertWorldToScreen(vector2);
                vector2.X -= num2 * (Constants.DistanceArrow.Width + 10) / 2;
                vector2.Y += num3 * (Constants.DistanceArrow.Height + 10) / 2;
                float num4 = 0f;
                Texture2D texture2D;
                if (num2 != 0 && num3 != 0)
                {
                    texture2D = Constants.DistanceArrowD;
                    if (num2 == 1)
                    {
                        if (num3 == 1)
                        {
                            num4 = -1.5707964f;
                        }
                        else if (num3 == -1)
                        {
                            num4 = 0f;
                        }
                    }
                    else if (num3 == 1)
                    {
                        num4 = 3.1415927f;
                    }
                    else if (num3 == -1)
                    {
                        num4 = 1.5707964f;
                    }
                }
                else
                {
                    texture2D = Constants.DistanceArrow;
                    if (num2 == 1)
                    {
                        num4 = 0f;
                    }
                    else if (num2 == -1)
                    {
                        num4 = 3.1415927f;
                    }
                    else if (num3 == 1)
                    {
                        num4 = -1.5707964f;
                    }
                    else if (num3 == -1)
                    {
                        num4 = 1.5707964f;
                    }
                }

                var vector3 = new Vector2(texture2D.Width / 2f, texture2D.Height / 2f);
                float num5 = Math.Max(Camera.Zoom * 0.5f, 1f);
                __instance.m_spriteBatch.Draw(texture2D, vector2, null, Color.Gray, num4, vector3, num5, SpriteEffects.None, 0f);
                vector2.X -= num2 * (Constants.DistanceArrow.Width + 10) * num5;
                vector2.Y += num3 * (Constants.DistanceArrow.Height + 10) * num5;
                string text = $"{__instance.Name} ({num})";
                if (Constants.Font1Outline is not null)
                {
                    var texture2D2 = __instance.CurrentTeam switch
                    {
                        Team.Team1 => Constants.TeamIcon1,
                        Team.Team2 => Constants.TeamIcon2,
                        Team.Team3 => Constants.TeamIcon3,
                        Team.Team4 => Constants.TeamIcon4,
                        (Team)5 => Globals.TeamIcon5,
                        (Team)6 => Globals.TeamIcon6,
                        _ => null
                    };

                    var vector4 = Constants.MeasureString(Constants.Font1Outline, text);
                    float num6 = Camera.ConvertWorldToScreenX(boundsArrows.Left);
                    float num7 = Camera.ConvertWorldToScreenX(boundsArrows.Right);
                    if (texture2D2 is not null)
                    {
                        num6 += (texture2D2.Width + 4f) * num5;
                    }

                    if (num3 == 0)
                    {
                        num7 -= texture2D.Width;
                        num6 += texture2D.Width;
                    }

                    if (vector2.X + vector4.X / 3f * num5 > num7)
                    {
                        vector2.X = num7 - vector4.X / 3f * num5;
                    }

                    if (vector2.X - vector4.X / 3f * num5 < num6)
                    {
                        vector2.X = num6 + vector4.X / 3f * num5;
                    }

                    float num8 = vector2.X - vector4.X * 0.5f * (num5 * 0.5f);
                    _ = Constants.DrawString(__instance.m_spriteBatch, Constants.Font1Outline, text, vector2, __instance.CurrentTeamColor, 0f, vector4 * 0.5f, num5 * 0.5f, SpriteEffects.None, 0);
                    if (texture2D2 is not null)
                    {
                        __instance.m_spriteBatch.Draw(texture2D2, new(num8 - texture2D2.Width * num5, vector2.Y - vector4.Y * 0.25f * num5), null, Color.Gray, 0f, Vector2.Zero, num5, SpriteEffects.None, 1f);
                    }
                }
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanKick))]
    private static bool CanKick(float timeOffset, Player __instance, ref bool __result)
    {
        var extendedPlayer = __instance.GetExtension();
        // __result = (__instance.CurrentAction == PlayerAction.Idle || (__instance.CurrentAction == PlayerAction.HipFire && __instance.ThrowableIsActivated)) && (!__instance.Diving || extendedPlayer.AdrenalineBoost) && !__instance.Rolling && !__instance.Falling && !__instance.Climbing && __instance.PreparingHipFire <= 0f && __instance.FireSequence.KickCooldownTimer <= 800f + timeOffset && !__instance.TimeSequence.PostDropClimbAttackCooldown && !__instance.StrengthBoostPreparing && !__instance.SpeedBoostPreparing;
        __result = (__instance.CurrentAction == PlayerAction.Idle || __instance.CurrentAction == PlayerAction.HipFire && __instance.ThrowableIsActivated) && !(__instance.Diving && !extendedPlayer.AdrenalineBoost || extendedPlayer.GenericJetpack is not null && extendedPlayer.GenericJetpack.State != JetpackState.Idling || __instance.Rolling || __instance.Falling || __instance.Climbing || __instance.PreparingHipFire > 0f || __instance.FireSequence.KickCooldownTimer > 800f + timeOffset || __instance.TimeSequence.PostDropClimbAttackCooldown || __instance.StrengthBoostPreparing || __instance.SpeedBoostPreparing);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanAttack))]
    private static bool CanAttack(Player __instance, ref bool __result)
    {
        var extendedPlayer = __instance.GetExtension();

        __result = !(__instance.Diving && !extendedPlayer.AdrenalineBoost || extendedPlayer.GenericJetpack is not null && extendedPlayer.GenericJetpack.State != JetpackState.Idling || __instance.Rolling || __instance.Climbing || __instance.LedgeGrabbing || __instance.ThrowingModeToggleQueued || __instance.ClimbingClient || __instance.StrengthBoostPreparing || __instance.SpeedBoostPreparing);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.TimeSequence.Update))]
    private static void UpdateTimeSequence(float ms, Player __instance)
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
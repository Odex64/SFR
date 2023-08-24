using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.GUI;
using SFD.MenuControls;
using SFD.Objects;
using SFD.UserProgression;
using SFD.Weapons;
using SFDGameScriptInterface;

namespace SFR.Fighter;

[HarmonyPatch]
internal class ExtendedTeam
{
    private static readonly EventInfo LobbySlotValueChangedEvent = typeof(LobbySlotTeam).GetEvent("LobbySlotValueChanged", BindingFlags.Instance | BindingFlags.Public);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.SpawnUsers))]
    private static bool FixSpawnUsersWithAdditionalTeams(GameInfo __instance, IChallenge challenge = null)
    {
        if (__instance.GameWorld == null)
        {
            throw new Exception("GameWorld mustn't be null when calling GameInfo.SpawnUsers");
        }

        var gameUserSpawns = __instance.GetGameUserSpawns(challenge);
        var list = __instance.GameWorld.PlayerSpawnMarkers.Select(x => x.GetSpawnPackageData()).ToList();
        if (list.Count == 0)
        {
            list.Add(new ObjectPlayerSpawnMarker.SpawnPackageData());
        }

        var list2 = new List<ObjectPlayerSpawnMarker.SpawnPackageData>(list);
        var dictionary = new Dictionary<Team, int>
        {
            { Team.Independent, 1 },
            { Team.Team1, 1 },
            { Team.Team2, 1 },
            { Team.Team3, 1 },
            { Team.Team4, 1 },
            { (Team)5, 1 },
            { (Team)6, 1 }
        };
        var dictionary2 = new Dictionary<int, ObjectPlayerSpawnMarker.SpawnPackageData>();
        var mapType = __instance.GameWorld.ObjectWorldData.MapType;
        int l;
        for (l = 0; l < gameUserSpawns.Count; l++)
        {
            var gameUserSpawn = gameUserSpawns[l];
            var gameSlot = gameUserSpawn.GameSlot;
            var gameUser4 = gameUserSpawn.GameUser;
            if (gameUser4 != null && !gameUser4.JoinedAsSpectator)
            {
                ObjectPlayerSpawnMarker.SpawnPackageData spawnPackageData = null;
                List<ObjectPlayerSpawnMarker.SpawnPackageData> list3 = null;
                var gameSlotTeam4 = gameSlot.CurrentTeam == (Team)(-1) ? Team.Independent : gameSlot.CurrentTeam;
                int playerSpawnOrder2 = dictionary[gameSlotTeam4];
                Dictionary<Team, int> dictionary3;
                Team gameSlotTeam3;
                (dictionary3 = dictionary)[gameSlotTeam3 = gameSlotTeam4] = dictionary3[gameSlotTeam3] + 1;
                switch (mapType)
                {
                    case MapType.Versus:
                        list3 = list2.Where(x => x.SpawnTeamValue == (int)gameSlotTeam4 && x.SpawnOrderValue == playerSpawnOrder2).ToList();
                        break;
                    case MapType.Custom:
                    case MapType.Challenge:
                        list3 = list2.Where(x => x.SpawnOrderValue == playerSpawnOrder2).ToList();
                        break;
                    case MapType.Campaign:
                    case MapType.Survival:
                        list3 = list2.Where(x => x.SpawnOrderValue == l + 1).ToList();
                        break;
                }

                if (list3 != null && list3.Count > 0)
                {
                    int num = Constants.RANDOM.Next(0, list3.Count);
                    spawnPackageData = list3[num];
                    dictionary2.Add(gameSlot.GameSlotIndex, spawnPackageData);
                }

                list2.Remove(spawnPackageData);
                if (list2.Count == 0)
                {
                    list2.AddRange(list);
                }
            }
        }

        dictionary = new Dictionary<Team, int>();
        dictionary.Add(Team.Independent, 1);
        dictionary.Add(Team.Team1, 1);
        dictionary.Add(Team.Team2, 1);
        dictionary.Add(Team.Team3, 1);
        dictionary.Add(Team.Team4, 1);
        dictionary.Add((Team)5, 1);
        dictionary.Add((Team)6, 1);
        int i;
        for (i = 0; i < gameUserSpawns.Count; i++)
        {
            var gameUserSpawn2 = gameUserSpawns[i];
            var gameSlot2 = gameUserSpawn2.GameSlot;
            var gameUser2 = gameUserSpawn2.GameUser;
            if (gameUser2 != null && !gameUser2.JoinedAsSpectator && !dictionary2.ContainsKey(gameSlot2.GameSlotIndex))
            {
                List<ObjectPlayerSpawnMarker.SpawnPackageData> list4 = null;
                var gameSlotTeam = gameSlot2.CurrentTeam == (Team)(-1) ? Team.Independent : gameSlot2.CurrentTeam;
                int playerSpawnOrder = dictionary[gameSlotTeam];
                Dictionary<Team, int> dictionary4;
                Team gameSlotTeam2;
                (dictionary4 = dictionary)[gameSlotTeam2 = gameSlotTeam] = dictionary4[gameSlotTeam2] + 1;
                switch (mapType)
                {
                    case MapType.Versus:
                        list4 = list2.Where(x => x.SpawnTeamValue == (int)gameSlotTeam && (x.SpawnOrderValue == playerSpawnOrder || x.SpawnOrderValue == 0)).ToList();
                        break;
                    case MapType.Custom:
                    case MapType.Challenge:
                        list4 = list2.Where(x => x.SpawnOrderValue == playerSpawnOrder || x.SpawnOrderValue == 0).ToList();
                        break;
                    case MapType.Campaign:
                    case MapType.Survival:
                        list4 = list2.Where(x => x.SpawnOrderValue == i + 1 || x.SpawnOrderValue == 0).ToList();
                        break;
                }

                if (list4 == null || list4.Count == 0)
                {
                    list4 = list2;
                }

                int num2 = Constants.RANDOM.Next(0, list4.Count);
                var spawnPackageData2 = list4[num2];
                dictionary2.Add(gameSlot2.GameSlotIndex, spawnPackageData2);
                list2.Remove(spawnPackageData2);
                if (list2.Count == 0)
                {
                    list2.AddRange(list);
                }
            }
        }

        var array = new PlayerRoundStats[8];
        using (var enumerator = __instance.GetGameUsers().GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var gameUser = enumerator.Current;
                if (gameUserSpawns.All(x => x.GameUser != gameUser))
                {
                    if (gameUser != null)
                    {
                        gameUser.SpawnIndex = -1;
                    }
                }
                else if (gameUser != null && gameUser.SpawnIndex != -1)
                {
                    array[gameUser.SpawnIndex] = __instance.MapSessionData.PlayerRoundStats[gameUser.SpawnIndex];
                }
            }
        }

        foreach (var gameUserSpawn3 in gameUserSpawns)
        {
            var gameSlot3 = gameUserSpawn3.GameSlot;
            var gameUser3 = gameUserSpawn3.GameUser;
            if (gameUser3 != null && !gameUser3.JoinedAsSpectator)
            {
                gameUser3.AFKHasMadeAction = false;
                gameUser3.AFKTotalTime = 0f;
                gameUser3.AFKWarningShown = false;
                ObjectPlayerSpawnMarker.SpawnPackageData spawnPackageData3 = null;
                if (dictionary2.TryGetValue(gameSlot3.GameSlotIndex, out spawnPackageData3))
                {
                    var team = gameSlot3.CurrentTeam == (Team)(-1) ? Team.Independent : gameSlot3.CurrentTeam;
                    var playerSpawnPoint = __instance.GameWorld.GetPlayerSpawnPoint(spawnPackageData3.WorldPosition);
                    var profile = spawnPackageData3.ProfileInfo ?? gameUser3.Profile;
                    var team2 = (Team)spawnPackageData3.SpawnTeamValue;
                    switch (mapType)
                    {
                        case MapType.Versus:
                            team2 = spawnPackageData3.SpawnTeamValue == -1 ? team : (Team)spawnPackageData3.SpawnTeamValue;
                            break;
                        case MapType.Custom:
                            team2 = spawnPackageData3.SpawnTeamValue == -1 ? team : (Team)spawnPackageData3.SpawnTeamValue;
                            break;
                        case MapType.Campaign:
                        case MapType.Survival:
                            team2 = (Team)(spawnPackageData3.SpawnTeamValue == -1 ? 1 : spawnPackageData3.SpawnTeamValue);
                            break;
                        case MapType.Challenge:
                            team2 = (Team)(spawnPackageData3.SpawnTeamValue == -1 ? 0 : spawnPackageData3.SpawnTeamValue);
                            break;
                        default:
                            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, "SpawnUsers unknown map type");
                            break;
                    }

                    if (team2 == (Team)(-1))
                    {
                        team2 = Team.Independent;
                    }

                    if (team2 != team)
                    {
                        gameSlot3.CurrentTeam = team2;
                        if (__instance.GameOwner == GameOwnerEnum.Server && GameSFD.Handle.Server != null)
                        {
                            GameSFD.Handle.Server.SyncGameSlotInfo(gameSlot3);
                        }
                    }

                    if (gameUser3.SpawnIndex == -1)
                    {
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (array[j] == null)
                            {
                                array[j] = __instance.MapSessionData.PlayerRoundStats[j];
                                gameUser3.SpawnIndex = array[j].SpawnIndex;
                                break;
                            }
                        }
                    }

                    float num3 = __instance.MapSessionData.StatsToKeepData.MinHealth;
                    if (mapType == MapType.Survival && !__instance.MapSessionData.SurvivalWaveFinished)
                    {
                        num3 = Math.Max(num3, __instance.MapSessionData.StatsToKeepData.SurvivalDefeatMinHealth);
                    }

                    bool flag = __instance.MapSessionData.StatsToKeepEnabled && array[gameUser3.SpawnIndex].InUse;
                    if (!flag || array[gameUser3.SpawnIndex].HealthValue > 0f || num3 > 0f)
                    {
                        __instance.GameWorld.CreatePlayer(new SpawnObjectInformation(__instance.GameWorld.IDCounter.NextObjectData("PLAYER"), playerSpawnPoint), profile, team2);
                        var player = __instance.GameWorld.Players[__instance.GameWorld.Players.Count - 1];
                        player.CorrectSpawnPosition = true;
                        player.LastDirectionX = spawnPackageData3.FaceDirection;
                        player.SetModifiers(spawnPackageData3.PlayerModifiers);
                        player.ForceServerMovementState(20);
                        player.ObjectData.BodyData.IncreaseMoveSequence();
                        player.SetUser(gameUser3.UserIdentifier);
                        player.SetDrawStatusInfoVisible(spawnPackageData3.DrawStatusInfo);
                        if (gameUser3.IsBot)
                        {
                            var predefinedAiType = gameUser3.BotPredefinedAIType;
                            if (predefinedAiType == PredefinedAIType.None)
                            {
                                predefinedAiType = PredefinedAIType.BotA;
                            }

                            if (__instance.MapInfo != null && __instance.MapInfo.TypedMapType != MapType.Versus)
                            {
                                var predefinedAiType2 = predefinedAiType;
                                if (predefinedAiType2 != PredefinedAIType.BotA)
                                {
                                    switch (predefinedAiType2)
                                    {
                                        case PredefinedAIType.BotB:
                                            predefinedAiType = PredefinedAIType.CompanionB;
                                            break;
                                        case PredefinedAIType.BotC:
                                            predefinedAiType = PredefinedAIType.CompanionC;
                                            break;
                                        case PredefinedAIType.BotD:
                                            predefinedAiType = PredefinedAIType.CompanionD;
                                            break;
                                        default:
                                            predefinedAiType = PredefinedAIType.CompanionA;
                                            break;
                                    }
                                }
                                else
                                {
                                    predefinedAiType = PredefinedAIType.CompanionA;
                                }
                            }

                            player.SetIsBot(gameUser3.IsBot);
                            player.SetBotBehavior(new BotBehavior(true, predefinedAiType));
                        }

                        if (flag)
                        {
                            var playerRoundStats = array[gameUser3.SpawnIndex];
                            player.Health.CurrentValue = Math.Max(num3, playerRoundStats.HealthValue);
                            if ((__instance.MapSessionData.StatsToKeepData.ItemsToKeep & ItemsToKeep.Melee) == ItemsToKeep.Melee)
                            {
                                player.GrabWeaponMeleeItem(playerRoundStats.MeleeWeapon);
                            }

                            if ((__instance.MapSessionData.StatsToKeepData.ItemsToKeep & ItemsToKeep.Handgun) == ItemsToKeep.Handgun)
                            {
                                player.GrabWeaponHandgunItem(playerRoundStats.HandgunWeapon);
                            }

                            if ((__instance.MapSessionData.StatsToKeepData.ItemsToKeep & ItemsToKeep.Rifle) == ItemsToKeep.Rifle)
                            {
                                player.GrabWeaponRifleItem(playerRoundStats.RifleWeapon);
                            }

                            if ((__instance.MapSessionData.StatsToKeepData.ItemsToKeep & ItemsToKeep.Throwable) == ItemsToKeep.Throwable)
                            {
                                player.GrabWeaponThrownItem(playerRoundStats.ThrowableWeapon);
                            }

                            if ((__instance.MapSessionData.StatsToKeepData.ItemsToKeep & ItemsToKeep.Powerup) == ItemsToKeep.Powerup)
                            {
                                player.GrabWeaponPowerupItem(playerRoundStats.PowerupItem);
                            }
                        }
                        else
                        {
                            var weaponItem = spawnPackageData3.WpnRifleID == -1 ? null : WeaponDatabase.GetWeapon((short)spawnPackageData3.WpnRifleID);
                            var weaponItem2 = spawnPackageData3.WpnHandgunID == -1 ? null : WeaponDatabase.GetWeapon((short)spawnPackageData3.WpnHandgunID);
                            var weaponItem3 = spawnPackageData3.WpnMeleeID == -1 ? null : WeaponDatabase.GetWeapon((short)spawnPackageData3.WpnMeleeID);
                            var weaponItem4 = spawnPackageData3.WpnThrownID == -1 ? null : WeaponDatabase.GetWeapon((short)spawnPackageData3.WpnThrownID);
                            var weaponItem5 = spawnPackageData3.WpnPowerupID == -1 ? null : WeaponDatabase.GetWeapon((short)spawnPackageData3.WpnPowerupID);
                            if (weaponItem != null)
                            {
                                player.GrabWeaponItem(weaponItem);
                            }

                            if (weaponItem2 != null)
                            {
                                player.GrabWeaponItem(weaponItem2);
                            }

                            if (weaponItem3 != null)
                            {
                                player.GrabWeaponItem(weaponItem3);
                            }

                            if (weaponItem4 != null)
                            {
                                player.GrabWeaponItem(weaponItem4);
                            }

                            if (weaponItem5 != null)
                            {
                                player.GrabWeaponItem(weaponItem5);
                            }
                        }

                        if (spawnPackageData3.OwnerPlayerSpawnMarker != null && spawnPackageData3.OwnerPlayerSpawnMarker.SpawnedUserIdentifier == 0)
                        {
                            spawnPackageData3.OwnerPlayerSpawnMarker.SpawnedUserIdentifier = gameUser3.UserIdentifier;
                        }
                    }
                }
            }
        }

        return false;
    }

    private static void InvokeTeamDelegates(MulticastDelegate eventDelegate, LobbySlotTeam lobbySlotTeam, int index)
    {
        if (eventDelegate != null)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
            {
                handler.Method.Invoke(handler.Target, new object[] { lobbySlotTeam, index });
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(VictoryText), nameof(VictoryText.Draw))]
    private static bool DrawAdditionalTeam(SpriteBatch spriteBatch, float elapsed, GameWorld gameWorld, VictoryText __instance)
    {
        if (__instance.blinkTimer <= 0f)
        {
            __instance.blink = !__instance.blink;
            __instance.blinkTimer += __instance.blink ? 50f : 250f;
        }
        else
        {
            __instance.blinkTimer -= elapsed;
        }

        string text = string.Empty;
        bool flag = false;
        if (gameWorld.MapType == MapType.Challenge)
        {
            switch (gameWorld.GameOverData.GameOverType)
            {
                case GameWorld.GameOverType.Nobody:
                case GameWorld.GameOverType.PlayerWins:
                case GameWorld.GameOverType.TeamWins:
                    text = LanguageHelper.GetText(gameWorld.GameOverData.ChallengeCompleted ? "statusText.victory" : "statusText.defeat");

                    break;
                case GameWorld.GameOverType.TimesUp:
                    text = LanguageHelper.GetText("statusText.timesup");
                    break;
                case GameWorld.GameOverType.Custom:
                    text = gameWorld.GameOverData.Text;
                    break;
            }
        }
        else
        {
            switch (gameWorld.GameOverData.GameOverType)
            {
                case GameWorld.GameOverType.Nobody:
                    text = LanguageHelper.GetText("statusText.nobody");
                    flag = true;
                    break;
                case GameWorld.GameOverType.PlayerWins:
                    text = gameWorld.GameOverData.Text;
                    flag = true;
                    break;
                case GameWorld.GameOverType.TeamWins:
                    switch (gameWorld.GameOverData.Team)
                    {
                        case Team.Team1:
                            text = LanguageHelper.GetText("team.1");
                            break;
                        case Team.Team2:
                            text = LanguageHelper.GetText("team.2");
                            break;
                        case Team.Team3:
                            text = LanguageHelper.GetText("team.3");
                            break;
                        case Team.Team4:
                            text = LanguageHelper.GetText("team.4");
                            break;
                        case (Team)5:
                            text = "Team 5";
                            break;
                        case (Team)6:
                            text = "Team 6";
                            break;
                        default:
                            text = LanguageHelper.GetText("team.independent");
                            break;
                    }

                    flag = true;
                    break;
                case GameWorld.GameOverType.TimesUp:
                    text = LanguageHelper.GetText("statusText.timesup");
                    break;
                case GameWorld.GameOverType.Custom:
                    text = gameWorld.GameOverData.Text;
                    break;
                case GameWorld.GameOverType.SurvivalVictory:
                    text = LanguageHelper.GetText("statusText.wave", new string[] { gameWorld.GameInfo.MapSessionData.SurvivalWave.ToString() });
                    flag = true;
                    __instance.m_winText = LanguageHelper.GetText("statusText.victory");
                    break;
                case GameWorld.GameOverType.SurvivalLoss:
                    if (gameWorld.GameInfo.MapSessionData.SurvivalExtraLives > 0)
                    {
                        text = LanguageHelper.GetText("statusText.wave", new string[] { gameWorld.GameInfo.MapSessionData.SurvivalWave.ToString() });
                        flag = true;
                        __instance.m_winText = LanguageHelper.GetText("statusText.defeat");
                    }
                    else
                    {
                        text = LanguageHelper.GetText("statusText.wavegameover", new string[] { gameWorld.GameInfo.MapSessionData.SurvivalWave.ToString() });
                        flag = true;
                        __instance.m_winText = LanguageHelper.GetText("statusText.survivalfinalscore", new string[] { gameWorld.GameInfo.TotalScore.ToString() });
                    }

                    break;
            }
        }

        __instance.DrawWithOutline(text.ToUpperInvariant(), Constants.FontBig, spriteBatch, (float)(GameSFD.GAME_HEIGHT / 7), 2f, __instance.blink ? Microsoft.Xna.Framework.Color.White : Constants.COLORS.STATUS_TEXT);
        if (flag)
        {
            __instance.DrawWithOutline(__instance.m_winText, Constants.FontBig, spriteBatch, (float)(GameSFD.GAME_HEIGHT / 7) + Constants.MeasureString(Constants.FontBig, __instance.m_winText).Y * 2f, 2f, __instance.blink ? Microsoft.Xna.Framework.Color.White : Constants.COLORS.STATUS_TEXT);
        }

        if (gameWorld.GameInfo.VoteInfo.MapVoteInitiated)
        {
            __instance.DrawWithOutline(__instance.m_mapVoteText, Constants.Font1, spriteBatch, (float)(GameSFD.GAME_HEIGHT * 5 / 6 + 32), 1f, Constants.COLORS.MENU_ORANGE);
            return false;
        }

        __instance.DrawWithOutline(__instance.m_voteText, Constants.Font1, spriteBatch, (float)(GameSFD.GAME_HEIGHT * 5 / 6), 1f, Microsoft.Xna.Framework.Color.White);
        __instance.DrawWithOutline(__instance.m_timerText, Constants.Font1, spriteBatch, (float)(GameSFD.GAME_HEIGHT * 5 / 6 + 32), 1f, Constants.COLORS.MENU_ORANGE);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LobbySlotTeam), nameof(LobbySlotTeam.dropdown))]
    private static bool AddAdditionalTeamInDropdown(LobbySlotTeam __instance)
    {
        if (!__instance.Enabled)
        {
            return false;
        }

        var eventDelegate = (MulticastDelegate)typeof(LobbySlotTeam).GetField(LobbySlotValueChangedEvent.Name, BindingFlags.Instance | BindingFlags.Public)?.GetValue(__instance);

        var lobbySlotDropdownPanel = new LobbySlotDropdownPanel(__instance, new MenuItemButton[]
        {
            new(LanguageHelper.GetText("team.independent"), __instance.independent),
            new(LanguageHelper.GetText("team.1"), __instance.team1),
            new(LanguageHelper.GetText("team.2"), __instance.team2),
            new(LanguageHelper.GetText("team.3"), __instance.team3),
            new(LanguageHelper.GetText("team.4"), __instance.team4),
            new("Team 5", _ =>
            {
                __instance.SetValue(5);
                InvokeTeamDelegates(eventDelegate, __instance, 5);
            }),
            new("Team 6", _ =>
            {
                __instance.SetValue(6);
                InvokeTeamDelegates(eventDelegate, __instance, 6);
            })
        });

        __instance.ParentPanel.OpenSubPanel(lobbySlotDropdownPanel);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants.COLORS), nameof(Constants.COLORS.GetTeamColor), typeof(TeamIcon), typeof(Constants.COLORS.TeamColorType))]
    private static bool GetAdditionalTeamColor(ref Microsoft.Xna.Framework.Color __result, TeamIcon team)
    {
        switch ((int)team)
        {
            case 5:
                __result = Misc.Constants.Team5;
                return false;

            case 6:
                __result = Misc.Constants.Team6;
                return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamColor), typeof(int))]
    private static bool GetAdditionalTeamColor(int team, ref Microsoft.Xna.Framework.Color __result)
    {
        switch (team)
        {
            case 5:
                __result = Misc.Constants.Team5;
                return false;

            case 6:
                __result = Misc.Constants.Team6;
                return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamString), typeof(int))]
    private static bool GetAdditionalTeamString(int team, ref string __result)
    {
        switch (team)
        {
            case 5:
                __result = "Team 5";
                return false;

            case 6:
                __result = "Team 6";
                return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamIcon))]
    private static bool GetAdditionalTeamIcon(Team team, ref Texture2D __result)
    {
        switch ((int)team)
        {
            case 5:
                __result = Misc.Constants.TeamIcon5;
                return false;

            case 6:
                __result = Misc.Constants.TeamIcon6;
                return false;
        }

        return true;
    }
}
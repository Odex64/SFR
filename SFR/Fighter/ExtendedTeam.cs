using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.DedicatedServer;
using SFD.GUI;
using SFD.MapEditor;
using SFD.MenuControls;
using SFD.Objects;
using SFD.UserProgression;
using SFD.Weapons;
using SFDGameScriptInterface;
using Color = Microsoft.Xna.Framework.Color;
using MenuItem = System.Windows.Forms.MenuItem;
using Point = System.Drawing.Point;

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
            if (gameUser4 is { JoinedAsSpectator: false })
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

                if (list3 is { Count: > 0 })
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

        dictionary = new Dictionary<Team, int>
        {
            { Team.Independent, 1 },
            { Team.Team1, 1 },
            { Team.Team2, 1 },
            { Team.Team3, 1 },
            { Team.Team4, 1 },
            { (Team)5, 1 },
            { (Team)6, 1 }
        };
        int i;
        for (i = 0; i < gameUserSpawns.Count; i++)
        {
            var gameUserSpawn2 = gameUserSpawns[i];
            var gameSlot2 = gameUserSpawn2.GameSlot;
            var gameUser2 = gameUserSpawn2.GameUser;
            if (gameUser2 is { JoinedAsSpectator: false } && !dictionary2.ContainsKey(gameSlot2.GameSlotIndex))
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
            if (gameUser3 is { JoinedAsSpectator: false })
            {
                gameUser3.AFKHasMadeAction = false;
                gameUser3.AFKTotalTime = 0f;
                gameUser3.AFKWarningShown = false;
                if (dictionary2.TryGetValue(gameSlot3.GameSlotIndex, out var spawnPackageData3))
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

                        if (spawnPackageData3.OwnerPlayerSpawnMarker is { SpawnedUserIdentifier: 0 })
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
                    text = LanguageHelper.GetText("statusText.wave", gameWorld.GameInfo.MapSessionData.SurvivalWave.ToString());
                    flag = true;
                    __instance.m_winText = LanguageHelper.GetText("statusText.victory");
                    break;
                case GameWorld.GameOverType.SurvivalLoss:
                    if (gameWorld.GameInfo.MapSessionData.SurvivalExtraLives > 0)
                    {
                        text = LanguageHelper.GetText("statusText.wave", gameWorld.GameInfo.MapSessionData.SurvivalWave.ToString());
                        flag = true;
                        __instance.m_winText = LanguageHelper.GetText("statusText.defeat");
                    }
                    else
                    {
                        text = LanguageHelper.GetText("statusText.wavegameover", gameWorld.GameInfo.MapSessionData.SurvivalWave.ToString());
                        flag = true;
                        __instance.m_winText = LanguageHelper.GetText("statusText.survivalfinalscore", gameWorld.GameInfo.TotalScore.ToString());
                    }

                    break;
            }
        }

        __instance.DrawWithOutline(text.ToUpperInvariant(), Constants.FontBig, spriteBatch, GameSFD.GAME_HEIGHT / 7, 2f, __instance.blink ? Color.White : Constants.COLORS.STATUS_TEXT);
        if (flag)
        {
            __instance.DrawWithOutline(__instance.m_winText, Constants.FontBig, spriteBatch, GameSFD.GAME_HEIGHT / 7 + Constants.MeasureString(Constants.FontBig, __instance.m_winText).Y * 2f, 2f, __instance.blink ? Color.White : Constants.COLORS.STATUS_TEXT);
        }

        if (gameWorld.GameInfo.VoteInfo.MapVoteInitiated)
        {
            __instance.DrawWithOutline(__instance.m_mapVoteText, Constants.Font1, spriteBatch, GameSFD.GAME_HEIGHT * 5 / 6 + 32, 1f, Constants.COLORS.MENU_ORANGE);
            return false;
        }

        __instance.DrawWithOutline(__instance.m_voteText, Constants.Font1, spriteBatch, GameSFD.GAME_HEIGHT * 5 / 6, 1f, Color.White);
        __instance.DrawWithOutline(__instance.m_timerText, Constants.Font1, spriteBatch, GameSFD.GAME_HEIGHT * 5 / 6 + 32, 1f, Constants.COLORS.MENU_ORANGE);

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.GetTeamStatus))]
    private static IEnumerable<CodeInstruction> AdditionalTeamStatus(IEnumerable<CodeInstruction> instructions)
    {
        var teamsCount = instructions.ElementAt(15);
        teamsCount.opcode = OpCodes.Ldc_I4_7;

        return instructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectPlayerSpawnTrigger), nameof(ObjectPlayerSpawnTrigger.GetSpawnTeam))]
    private static IEnumerable<CodeInstruction> AdditionalPlayerSpawnTeam(IEnumerable<CodeInstruction> instructions)
    {
        var teamsCount = instructions.ElementAt(11);
        teamsCount.opcode = OpCodes.Ldc_I4_6;

        return instructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamColorString), typeof(int))]
    private static bool GetAdditionalTeamColorString(int team, ref string __result)
    {
        __result = team switch
        {
            1 => LanguageHelper.GetText("team.1.color"),
            2 => LanguageHelper.GetText("team.2.color"),
            3 => LanguageHelper.GetText("team.3.color"),
            4 => LanguageHelper.GetText("team.4.color"),
            5 => "Purple",
            6 => "Cyan",
            _ => string.Empty
        };

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDDedicatedServerForm), nameof(SFDDedicatedServerForm.listViewGameSlots_menuItem_Click))]
    private static bool DedicatedServerMenuClick(object sender, EventArgs e, SFDDedicatedServerForm __instance)
    {
        if (sender != null && __instance.m_gameSlotsContextMenu.SelectedItems != null && sender is MenuItem { Tag: string } menuItem)
        {
            bool flag = false;
            string text = "\n" + string.Join(", ", __instance.m_gameSlotsContextMenu.SelectedItems.Select(x => x.GameSlot.GameUser == null ? "?" : x.GameSlot.GameUser.GetProfileName()));
            foreach (var gameSlotListItem in __instance.m_gameSlotsContextMenu.SelectedItems)
            {
                int gameSlotIndex = gameSlotListItem.GameSlot.GameSlotIndex;
                switch ((string)menuItem.Tag)
                {
                    case "KICK":
                        if (gameSlotListItem.GameSlot.IsOccupied)
                        {
                            if (!flag)
                            {
                                flag = true;
                                if (MessageBox.Show(LanguageHelper.GetText("dedicatedserver.menu.kick.confirm.question") + text, LanguageHelper.GetText("dedicatedserver.menu.kick.confirm.title"), MessageBoxButtons.YesNo) != DialogResult.Yes)
                                {
                                    return false;
                                }
                            }

                            if (gameSlotListItem.GameSlot.GameUser != null)
                            {
                                __instance.SendChatMessage("/KICK_USER " + gameSlotListItem.GameSlot.GameUser.UserIdentifier);
                            }
                        }

                        break;
                    case "BAN":
                        if (gameSlotListItem.GameSlot.IsOccupied)
                        {
                            if (!flag)
                            {
                                flag = true;
                                if (MessageBox.Show(LanguageHelper.GetText("dedicatedserver.menu.ban.confirm.question") + text, LanguageHelper.GetText("dedicatedserver.menu.ban.confirm.title"), MessageBoxButtons.YesNo) != DialogResult.Yes)
                                {
                                    return false;
                                }
                            }

                            if (gameSlotListItem.GameSlot.GameUser != null)
                            {
                                __instance.SendChatMessage("/BAN_USER " + gameSlotListItem.GameSlot.GameUser.UserIdentifier);
                            }
                        }

                        break;
                    case "OPEN":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.State, 0)));
                        break;
                    case "CLOSE":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.State, 1)));
                        break;
                    case "BOTEASY":
                        if (!gameSlotListItem.GameSlot.IsOccupied || gameSlotListItem.GameSlot.IsOccupiedByBot)
                        {
                            DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.State, 4)));
                        }

                        break;
                    case "BOTNORMAL":
                        if (!gameSlotListItem.GameSlot.IsOccupied || gameSlotListItem.GameSlot.IsOccupiedByBot)
                        {
                            DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.State, 2)));
                        }

                        break;
                    case "BOTHARD":
                        if (!gameSlotListItem.GameSlot.IsOccupied || gameSlotListItem.GameSlot.IsOccupiedByBot)
                        {
                            DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.State, 5)));
                        }

                        break;
                    case "BOTEXPERT":
                        if (!gameSlotListItem.GameSlot.IsOccupied || gameSlotListItem.GameSlot.IsOccupiedByBot)
                        {
                            DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.State, 6)));
                        }

                        break;
                    case "TEAM_0":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 0)));
                        break;
                    case "TEAM_1":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 1)));
                        break;
                    case "TEAM_2":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 2)));
                        break;
                    case "TEAM_3":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 3)));
                        break;
                    case "TEAM_4":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 4)));
                        break;
                    case "TEAM_5":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 5)));
                        break;
                    case "TEAM_6":
                        DSCommand.Execute(new DSCommand.ChangeGameSlot(new NetMessage.GameInfo.GameSlotChange.Data(gameSlotIndex, NetMessage.GameInfo.GameSlotChange.DataChangeType.Team, 6)));
                        break;
                }
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDDedicatedServerForm), nameof(SFDDedicatedServerForm.listViewGameSlots_MouseClick))]
    private static bool DedicatedServerMenu(object sender, MouseEventArgs e, SFDDedicatedServerForm __instance)
    {
        if (e.Button == MouseButtons.Right)
        {
            __instance.m_gameSlotsContextMenu.CleanContextMenu();
            if (__instance.m_gameSlotsContextMenu.SetSelectedItems(e) > 0)
            {
                bool flag = __instance.m_gameSlotsContextMenu.SelectedItems.Any(x => !x.GameSlot.IsOpen);
                bool flag2 = __instance.m_gameSlotsContextMenu.SelectedItems.Any(x => !x.GameSlot.IsClosed);
                bool flag3 = __instance.m_gameSlotsContextMenu.SelectedItems.Any(x => !x.GameSlot.IsOccupied || x.GameSlot.IsOccupiedByBot);
                bool flag4 = __instance.m_gameSlotsContextMenu.SelectedItems.Any(x => x.GameSlot.IsOccupiedByUser);
                __instance.m_gameSlotsContextMenu.MenuItems = new List<MenuItem>();

                MenuItem menuItem;
                if (flag)
                {
                    menuItem = new MenuItem(LanguageHelper.GetText("dedicatedserver.menu.gameslot.open"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "OPEN";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                }

                if (flag2)
                {
                    menuItem = new MenuItem(LanguageHelper.GetText("dedicatedserver.menu.gameslot.close"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "CLOSE";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                }

                if (flag3)
                {
                    menuItem = new MenuItem(LanguageHelper.GetText("menu.lobby.slot.bot.easy"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "BOTEASY";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                    menuItem = new MenuItem(LanguageHelper.GetText("menu.lobby.slot.bot.normal"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "BOTNORMAL";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                    menuItem = new MenuItem(LanguageHelper.GetText("menu.lobby.slot.bot.hard"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "BOTHARD";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                    menuItem = new MenuItem(LanguageHelper.GetText("menu.lobby.slot.bot.expert"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "BOTEXPERT";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                }

                if (flag4)
                {
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(new MenuItem("-"));
                    menuItem = new MenuItem(LanguageHelper.GetText("dedicatedserver.menu.kick"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "KICK";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                    menuItem = new MenuItem(LanguageHelper.GetText("dedicatedserver.menu.ban"), __instance.listViewGameSlots_menuItem_Click);
                    menuItem.Tag = "BAN";
                    menuItem.Enabled = true;
                    __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                }

                menuItem = new MenuItem("Team", new List<MenuItem>
                {
                    new(GameSlotListItem.CreateTeamString(Team.Independent), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_0"
                    },
                    new(GameSlotListItem.CreateTeamString(Team.Team1), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_1"
                    },
                    new(GameSlotListItem.CreateTeamString(Team.Team2), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_2"
                    },
                    new(GameSlotListItem.CreateTeamString(Team.Team3), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_3"
                    },
                    new(GameSlotListItem.CreateTeamString(Team.Team4), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_4"
                    },
                    new(GameSlotListItem.CreateTeamString((Team)5), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_5"
                    },
                    new(GameSlotListItem.CreateTeamString((Team)6), __instance.listViewGameSlots_menuItem_Click)
                    {
                        Tag = "TEAM_6"
                    }
                }.ToArray());
                __instance.m_gameSlotsContextMenu.MenuItems.Add(new MenuItem("-"));
                __instance.m_gameSlotsContextMenu.MenuItems.Add(menuItem);
                __instance.m_gameSlotsContextMenu.ContextMenu = new ContextMenu(__instance.m_gameSlotsContextMenu.MenuItems.ToArray());
                __instance.m_gameSlotsContextMenu.ContextMenu.Show(__instance.listViewGameSlots, new Point(e.X, e.Y));
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDMapTestOptions.GameUserGUI), nameof(SFDMapTestOptions.GameUserGUI.Init))]
    private static bool AddAdditionalTeamsInTestOptions(SFDMapTestOptions.GameUserGUI __instance)
    {
        __instance.UserTeam.DropDownStyle = ComboBoxStyle.DropDownList;
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem(null));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem(Team.Independent));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem(Team.Team1));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem(Team.Team2));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem(Team.Team3));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem(Team.Team4));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem((Team)5));
        __instance.UserTeam.Items.Add(new SFDMapTestOptions.GameUserGUI.TeamItem((Team)6));
        __instance.SelectTeam(EditorMapTestData.Instance.TestUsers[__instance.Index].Team);
        __instance.UserTeam.SelectedIndexChanged += __instance.UserTeam_SelectedIndexChanged;
        __instance.UpdateNameLabel();
        __instance.UpdatePreviewPicture();

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static IEnumerable<CodeInstruction> AdditionalTeamCommands(IEnumerable<CodeInstruction> instructions)
    {
        var teamsCount = instructions.ElementAt(4175);
        teamsCount.opcode = OpCodes.Ldc_I4_6;

        return instructions;
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

        var lobbySlotDropdownPanel = new LobbySlotDropdownPanel(__instance,
            new MenuItemButton(LanguageHelper.GetText("team.independent"), __instance.independent),
            new MenuItemButton(LanguageHelper.GetText("team.1"), __instance.team1),
            new MenuItemButton(LanguageHelper.GetText("team.2"), __instance.team2),
            new MenuItemButton(LanguageHelper.GetText("team.3"), __instance.team3),
            new MenuItemButton(LanguageHelper.GetText("team.4"), __instance.team4),
            new MenuItemButton("Team 5", _ =>
            {
                __instance.SetValue(5);
                InvokeTeamDelegates(eventDelegate, __instance, 5);
            }),
            new MenuItemButton("Team 6", _ =>
            {
                __instance.SetValue(6);
                InvokeTeamDelegates(eventDelegate, __instance, 6);
            }));

        __instance.ParentPanel.OpenSubPanel(lobbySlotDropdownPanel);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants.COLORS), nameof(Constants.COLORS.GetTeamColor), typeof(TeamIcon), typeof(Constants.COLORS.TeamColorType))]
    private static bool GetAdditionalTeamColor(ref Color __result, TeamIcon team)
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
    private static bool GetAdditionalTeamColor(int team, ref Color __result)
    {
        switch (team)
        {
            case 5:
                __result = Misc.Constants.Team5 * 2;
                return false;

            case 6:
                __result = Misc.Constants.Team6 * 2;
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
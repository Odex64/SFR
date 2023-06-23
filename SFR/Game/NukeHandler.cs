using System.Collections.Generic;
using System.Linq;
using Box2D.XNA;
using HarmonyLib;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFDGameScriptInterface;
using SFR.Helper;
using SFR.Objects;
using SFR.Sync.Generic;
using Constants = SFR.Misc.Constants;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SFR.Game;

[HarmonyPatch]
internal static class NukeHandler
{
    private const float ScreenWipeTime = 5000f;
    private const float SoundInterval = 50;

    private const float BurnTime = 0.4f;

    private const float KillTime = 0.75f;
    internal const float FadeTime = 0.75f;

    private const float BgFgDifference = 0.08f;
    private const float ShakeIntensity = 12f;
    private const float MaxSaturation = 3f;
    private static float _screenWipeProgress;
    internal static bool IsActive;

    private static float _soundTimer;

    private static bool _death;
    private static PlayerTerminateType _terminateType;
    private static ObjectTerminateType _terminateObjects;
    private static string _gameOverText;

    private static GameWorld _gameWorld;

    private static List<ObjectNuke> _nukeObjects = new();

    private static void Finish()
    {
        _screenWipeProgress = 0f;
        IsActive = false;
        _death = false;
        foreach (var nuke in _nukeObjects)
        {
            nuke.Delete();
        }

        _nukeObjects.Clear();
    }

    internal static void CreateNuke(GameWorld gameWorld, PlayerTerminateType terminateType = PlayerTerminateType.Kill, string gameOverText = "", ObjectTerminateType objectType = ObjectTerminateType.Destroy)
    {
        if (IsActive)
        {
            return;
        }

        _gameWorld = gameWorld;
        if (gameWorld.GameOwner == GameOwnerEnum.Server) // == Client
        {
            var bgNuke = (ObjectNuke)ObjectData.CreateNew(new ObjectDataStartParams(gameWorld.IDCounter.NextID(), 1, 0, "BgNuke", gameWorld.GameOwner));
            gameWorld.CreateTile(new SpawnObjectInformation(bgNuke, new Vector2(0, 0)));
            var fgNuke = (ObjectNuke)ObjectData.CreateNew(new ObjectDataStartParams(gameWorld.IDCounter.NextID(), 2, 0, "FgNuke", gameWorld.GameOwner));
            gameWorld.CreateTile(new SpawnObjectInformation(fgNuke, new Vector2(0, 0)));

            // if (gameWorld.GameOwner == GameOwnerEnum.Server)
            // {
            GenericData.SendGenericDataToClients(new GenericData(DataType.Nuke, new[] { SyncFlag.MustSyncNewObjects }, fgNuke.ObjectID, bgNuke.ObjectID));
            gameWorld.SlowmotionHandler.Reset();
            gameWorld.SlowmotionHandler.AddSlowmotion(new Slowmotion(ScreenWipeTime * 0.25f, ScreenWipeTime * 0.8f, ScreenWipeTime * 0.2f, 0.2f, 0));
            // }

            List<ObjectNuke> nukeObjects = new()
            {
                fgNuke,
                bgNuke
            };

            _terminateType = terminateType;
            _terminateObjects = objectType;
            _gameOverText = gameOverText;

            Begin(nukeObjects);
        }
    }

    internal static void Begin(List<ObjectNuke> nukes)
    {
        _screenWipeProgress = 0f;
        IsActive = true;
        _nukeObjects = nukes;
        _death = false;
    }

    private static void Update(float ms)
    {
        if (IsActive)
        {
            if (_screenWipeProgress > ScreenWipeTime)
            {
                // Logger.LogDebug("1");
                Finish();
                return;
            }

            _screenWipeProgress += ms;
            float relativeProgress = _screenWipeProgress / ScreenWipeTime;
            for (int i = _nukeObjects.Count - 1; i >= 0; i--)
            {
                // Logger.LogDebug("2");
                if (_nukeObjects[i] is { Tile: not null })
                {
                    _nukeObjects[i].IsActive = true;
                    if (_nukeObjects[i].Tile.Name == "BgNuke")
                    {
                        _nukeObjects[i].Progress = relativeProgress + BgFgDifference;
                    }
                    else
                    {
                        _nukeObjects[i].Progress = relativeProgress;
                    }

                    // Logger.LogDebug("4");
                }
                else
                {
                    // Logger.LogDebug("5");
                    Finish();
                    return;
                }
            }

            // Logger.LogDebug("6");
            // BurnPlayers(); // Error?

            if (relativeProgress > KillTime && !_death)
            {
                // Logger.LogDebug("7");
                _death = true;
                if (_gameWorld != null)
                {
                    SetTheWorldOnFire();
                }

                // Logger.LogDebug("8");
            }

            float shake = relativeProgress;
            if (relativeProgress > FadeTime)
            {
                // Logger.LogDebug("9");
                float value = Math.InverseLerp(FadeTime, 1, relativeProgress);
                shake = Math.Lerp(1, 0, value);
            }

            Camera.m_shakeIntensity = shake * ShakeIntensity;

            _soundTimer += ms;
            if (_soundTimer > SoundInterval)
            {
                // Logger.LogDebug("10");
                if (Constants.Random.NextDouble() > 0.75)
                {
                    SoundHandler.PlaySound("Explosion", _gameWorld);
                }

                SoundHandler.PlaySound("MuffledExplosion", _gameWorld);
                _soundTimer = 0;
            }

            // Logger.LogDebug("11");
        }
    }

    private static void BurnPlayers()
    {
        float width = Camera.WorldRight - Camera.WorldLeft;
        float progress = _screenWipeProgress / ScreenWipeTime + BurnTime;

        double value = Camera.WorldLeft - width * (1 - progress * 1.5) + width / 2 + BurnTime * width;

        foreach (var player in _gameWorld.Players)
        {
            if (player is { IsRemoved: false })
            {
                if (!player.Burned && player.Position.X < value && _terminateType != PlayerTerminateType.None)
                {
                    player.Burned = true;
                    EffectHandler.PlayEffect("PLRB", player.Position, player.GameWorld, player.ObjectID);
                }
            }
        }
    }

    private static void SetTheWorldOnFire()
    {
        foreach (var player in _gameWorld.Players.Where(p => p is { IsRemoved: false }))
        {
            if (_gameWorld.GameOwner != GameOwnerEnum.Client)
            {
                switch (_terminateType)
                {
                    case PlayerTerminateType.Kill when !player.CheatInfiniteLife && !Cheat.InfiniteLife:
                        player.Kill();
                        break;
                    case PlayerTerminateType.Remove:
                        player.Remove();
                        break;
                    case PlayerTerminateType.Gib:
                        player.Gib();
                        break;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(_gameOverText))
        {
            _gameWorld.GameOverData.GameOverType = GameWorld.GameOverType.Custom;
            _gameWorld.GameOverData.Text = _gameOverText;
            _gameWorld.SetGameOver(GameWorld.GameOverReason.Custom);
        }
        else
        {
            _gameWorld.SetGameOver(GameWorld.GameOverReason.TimesUp);
        }

        AABB.Create(out var area, new Vector2((int)(Camera.WorldRight + Camera.WorldLeft) / 2, (int)(Camera.WorldBottom + Camera.WorldTop) / 2), (Camera.WorldRight - Camera.WorldLeft) / 2);
        var objects = _gameWorld.GetObjectDataByArea(area, false, PhysicsLayer.Active);
        // Logger.LogDebug("Active objects = " + objects.Count);
        if (_terminateObjects != ObjectTerminateType.None)
        {
            foreach (var obj in objects.Where(obj => obj is { IsPlayer: false }))
            {
                obj.SetMaxFire();
                if (obj.Destructable)
                {
                    switch (_terminateObjects)
                    {
                        case ObjectTerminateType.Destroy:
                            obj.Destroy();
                            break;
                        case ObjectTerminateType.Remove:
                            obj.Remove();
                            break;
                    }
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.HandleObjectUpdateCycle))]
    private static void HandleObjectUpdateCycle(float ms)
    {
        Update(ms);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.PrepareMainRenderTarget))]
    private static void HandleSaturation(GameSFD __instance)
    {
        if (IsActive)
        {
            float progress = _screenWipeProgress / ScreenWipeTime;
            float saturation = 1 + _screenWipeProgress / ScreenWipeTime * MaxSaturation;
            if (progress > FadeTime)
            {
                float value = Math.InverseLerp(FadeTime, 1, progress);
                saturation = Math.Lerp(GameSFD.Saturation, saturation, value);
            }

            GameSFD.Saturation = saturation;
        }
    }
}
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFR.Objects.Animal;

namespace SFR.Objects;

[HarmonyPatch]
internal static class ObjectsHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.CreateNew))]
    private static bool LoadObjects(ObjectDataStartParams startParams, ref ObjectData __result)
    {
        switch (startParams.MapObjectID)
        {
            case "WOODDOOR00":
                __result = new ObjectDoor(startParams);
                return false;
            case "WPNSNOWBALLTHROWN":
                __result = new ObjectSnowballThrown(startParams);
                return false;
            case "MARBLESTATUE00":
                __result = new ObjectDestructible(startParams, "MarbleStatue00_D", "MarbleStatueDebris00A", "MarbleStatueDebris00B")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE00_D":
                __result = new ObjectDestructible(startParams, "MarbleStatue00_DD", "MarbleStatueDebris00A", "MarbleStatueDebris00B")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE00_DD":
                __result = new ObjectDefault(startParams)
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE01":
                __result = new ObjectDestructible(startParams, "MarbleStatue01_D", "MarbleStatueDebris00A", "MarbleStatueDebris00B")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE01_D":
                __result = new ObjectDestructible(startParams, "MarbleStatue01_DD", "MarbleStatueDebris00B", "MarbleStatueDebris00B")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE01_DD":
                __result = new ObjectDefault(startParams)
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE02":
                __result = new ObjectDestructible(startParams, "MarbleStatue02_D", "MarbleStatueDebris00B", "MarbleStatueDebris00C")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE02_D":
                __result = new ObjectDestructible(startParams, "MarbleStatue02_DD", "MarbleStatueDebris00B", "MarbleStatueDebris00B")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "MARBLESTATUE02_DD":
                __result = new ObjectDefault(startParams)
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "BGPAINTING00A":
            case "BGPAINTING00B":
            case "BGPAINTING00C":
            case "BGPAINTING00D":
            case "BGPAINTING00E":
                __result = new ObjectDestructible(startParams, "BgPainting00_D")
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "BGPAINTING00_D":
                __result = new ObjectDefault(startParams)
                {
                    ProjectileTunnelingCheck = ProjectileTunnelingCheck.IgnoreAll
                };
                return false;
            case "HEADDEBRIS00A":
            case "HEADDEBRIS00B":
            case "HEADDEBRIS00C":
            case "ORGAN00":
            case "ORGAN01":
            case "ORGAN02":
            case "ORGAN03":
            case "ORGAN04":
            case "ORGAN05":
            case "BRAIN00":
            case "GIBLET05":
                __result = new ObjectGiblet(startParams);
                return false;
            case "HEAD00":
                __result = new ObjectHead(startParams);
                return false;
            case "WATERMELON00":
                __result = new ObjectDestructible(startParams, "Watermelon01", "Watermelon02", "Watermelon02");
                return false;
            case "BEACHBALL00":
                __result = new ObjectDestructible(startParams, "Beachball00_D");
                return false;
            case "WPNFRAGGRENADESTHROWN":
                __result = new ObjectFragGrenadeThrown(startParams);
                return false;
            case "WPNIMPACTGRENADESTHROWN":
                __result = new ObjectImpactGrenadeThrown(startParams);
                return false;
            case "CROSSBOWBOLT00":
            case "CROSSBOWBOLT01":
                __result = new ObjectCrossbowBolt(startParams);
                return false;
            case "STICKYBOMBDEBRIS1":
                __result = new ObjectShell(startParams);
                return false;
            case "WPNSTICKYBOMBTHROWN":
                __result = new ObjectStickyBombThrown(startParams);
                return false;
            case "JETPACKDEBRIS1":
            case "JETPACKDEBRIS2":
                __result = new ObjectShell(startParams);
                return false;
            case "FARBGTEXT":
                __result = new ObjectText(startParams);
                return false;
            case "PORTALWOODU":
            case "PORTALWOODD":
                __result = new ObjectPortal(startParams);
                return false;
            case "WOODBARRELEXPLOSIVE00":
                __result = new ObjectBarrelExplosive(startParams);
                return false;
            case "WOODBARREL02":
                __result = new ObjectDestructible(startParams, "", "WoodBarrelDebris00A", "WoodBarrelDebris00B", "WoodBarrelDebris00A", "WoodBarrelDebris00C");
                return false;
            case "CANNON00":
                __result = new ObjectCannon(startParams);
                return false;
            case "PIRATESHIPSMALL00":
            case "PIRATESHIPSMALL01":
                __result = new ObjectPirateShip(startParams);
                return false;
            case "PIRATESTEERINGWHEEL00":
                __result = new ObjectButtonTrigger(startParams);
                return false;
            case "CANNONBALLCRATE00":
                __result = new ObjectPirateItemGiver("BALL", startParams);
                return false;
            case "PIRATEARMORY00":
                __result = new ObjectPirateItemGiver("ARMORY", startParams);
                return false;
            case "PARROT00":
            case "PARROT01":
                __result = new ObjectBird(startParams);
                return false;
            case "MONKEY00":
                __result = new ObjectMonkey(startParams);
                return false;
            case "WPNCLAYMORETHROWN":
                __result = new ObjectClaymoreThrown(startParams);
                return false;
            case "FROGGY00":
                __result = new ObjectFroggy(startParams);
                return false;
            case "INVISIBLEBLOCKREALLYSMALL":
                __result = new ObjectInvisible(startParams);
                return false;
            case "PROJECTILESTICKY":
                __result = new ObjectStickyProjectile(startParams);
                return false;
            case "BGNUKE":
            case "FGNUKE":
                __result = new ObjectNuke(startParams);
                return false;
            case "NUKETRIGGER":
                __result = new ObjectNukeTrigger(startParams);
                return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.DrawBase))]
    private static bool DamageFlash(SpriteBatch spriteBatch, float ms, Color drawColor, ObjectData __instance)
    {
        __instance.GetDrawColor(ref drawColor);
        switch (__instance.MapObjectID)
        {
            case "WOODDOOR00":
                DrawAnimatedObject(spriteBatch, __instance, drawColor);
                return false;
        }

        return true;
    }

    private static void DrawAnimatedObject(SpriteBatch spriteBatch, ObjectData objectData, Color drawColor)
    {
        var value = Converter.ConvertBox2DToWorld(objectData.Body.GetPosition());
        var zero = Vector2.Zero;
        for (int j = 0; j < objectData.m_fixtureSizeXMultiplier; j++)
        {
            for (int k = 0; k < objectData.m_fixtureSizeYMultiplier; k++)
            {
                zero.X = j * objectData.CurrentAnimation.FrameWidth;
                zero.Y = -(float)(k * objectData.CurrentAnimation.FrameHeight);
                SFDMath.RotatePosition(ref zero, objectData.Body.GetAngle(), out zero);
                var vector = Camera.ConvertWorldToScreen(value + zero);
                objectData.CurrentAnimation.Draw(spriteBatch, objectData.Texture, vector, objectData.Body.GetAngle(), objectData.m_faceDirectionSpriteEffect, drawColor);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectProperties), nameof(ObjectProperties.AddProperty))]
    private static bool AddProperty(ObjectPropertyItem propertyItem)
    {
        switch (propertyItem.PropertyID)
        {
            //Add more portals to portal filter
            case 5:
                propertyItem.Filter = "targetSelf=false|targets=PortalD,PortalU,Portal,PortalWoodD,PortalWoodU";
                ObjectProperties.m_allProperties.Add(propertyItem.PropertyID, propertyItem);
                return false;
            case 81:
                propertyItem.Filter += ",NukeTrigger";
                ObjectProperties.m_allProperties.Add(propertyItem.PropertyID, propertyItem);
                return false;
            default:
                return true;
        }
    }
}
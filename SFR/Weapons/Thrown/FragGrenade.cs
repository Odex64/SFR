using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Helper;
using SFR.Objects;
using Constants = SFR.Misc.Constants;

namespace SFR.Weapons.Thrown;

internal sealed class FragGrenade : TWeapon
{
    internal FragGrenade()
    {
        TWeaponProperties weaponProperties = new(88, "Frag_Grenades", "WpnFragGrenades", false, WeaponCategory.Supply)
        {
            MaxCarriedTotalThrowables = 5,
            NumberOfThrowables = 3,
            ThrowObjectID = "WpnFragGrenadesThrown",
            ThrowDeadlineTimer = 2550f,
            DrawSoundID = "GrenadeDraw"
        };

        TWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("FragGrenadeM");
        weaponVisuals.SetDrawnTexture("FragGrenadeT");
        weaponVisuals.AnimDraw = "UpperDrawThrown";
        weaponVisuals.AnimManualAim = "ManualAimThrown";
        weaponVisuals.AnimManualAimStart = "ManualAimThrownStart";
        weaponVisuals.AnimCrouchUpper = "UpperCrouch";
        weaponVisuals.AnimIdleUpper = "UpperIdle";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKick";
        weaponVisuals.AnimJumpUpper = "UpperJump";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFalling";
        weaponVisuals.AnimKickUpper = "UpperKick";
        weaponVisuals.AnimStaggerUpper = "UpperStagger";
        weaponVisuals.AnimRunUpper = "UpperRun";
        weaponVisuals.AnimWalkUpper = "UpperWalk";
        weaponVisuals.AnimFullLand = "FullLandThrown";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Frag Grenades";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = Properties.NumberOfThrowables;
    }

    private FragGrenade(TWeaponProperties weaponProperties, TWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = weaponProperties.NumberOfThrowables;
    }

    public override void OnBeforeBeginCharge(TWeaponBeforeBeginChargeArgs e) { }

    public override void OnThrow(TWeaponOnThrowArgs e)
    {
        if (e.Player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("GrenadeThrow", e.Player.Position, e.Player.GameWorld);
        }

        if (e.Player.GameOwner != GameOwnerEnum.Client && e.ThrowableIsActivated)
        {
            var objectGrenadeThrown = (ObjectFragGrenadeThrown)e.ThrowableObjectData;
            objectGrenadeThrown.ExplosionTimer = e.ThrowableDeadlineTimer;
        }
    }

    public override void OnBeginCharge(TWeaponOnBeginChargeArgs e)
    {
        if (e.Player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("GrenadeSafe", e.Player.Position, e.Player.GameWorld);
            var worldPosition = e.Player.Position + new Vector2(-(float)e.Player.LastDirectionX * 5f, 7f);
            Vector2 linearVelocity = new(-(float)e.Player.LastDirectionX * 2f, 2f);
            e.Player.GameWorld.CreateLocalTile("WpnGrenadePin", worldPosition, Constants.Random.NextFloat(-3f, 3f), (short)e.Player.LastDirectionX, linearVelocity, Constants.Random.NextFloat(-3f, 3f));
        }
    }

    public override void OnDrop(TWeaponOnThrowArgs e)
    {
        if (e.Player.GameOwner != GameOwnerEnum.Client && e.ThrowableIsActivated)
        {
            var objectGrenadeThrown = (ObjectFragGrenadeThrown)e.ThrowableObjectData;
            objectGrenadeThrown.ExplosionTimer = e.ThrowableDeadlineTimer;
        }
    }

    public override void OnDeadline(TWeaponOnDeadlineArgs e)
    {
        e.Action = TWeaponDeadlineAction.Drop;
    }

    public override TWeapon Copy() => new FragGrenade(Properties, Visuals)
    {
        NumberOfThrowablesLeft = NumberOfThrowablesLeft
    };
}
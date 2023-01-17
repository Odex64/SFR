using SFD;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Thrown;

internal sealed class Snowball : TWeapon
{
    internal Snowball()
    {
        TWeaponProperties weaponProperties = new(90, "Snowball", "WpnSnowball", false, WeaponCategory.Supply)
        {
            MaxCarriedTotalThrowables = 8,
            NumberOfThrowables = 6,
            ThrowObjectID = "WpnSnowballThrown",
            ThrowDeadlineTimer = 2550f,
            DrawSoundID = "GrenadeDraw"
        };

        TWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("SnowballM");
        weaponVisuals.SetDrawnTexture("SnowballT");
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
        weaponProperties.VisualText = "Snowball";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = Properties.NumberOfThrowables;
    }

    private Snowball(TWeaponProperties weaponProperties, TWeaponVisuals weaponVisuals)
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
    }

    public override void OnBeginCharge(TWeaponOnBeginChargeArgs e) { }

    public override void OnDrop(TWeaponOnThrowArgs e) { }

    public override void OnDeadline(TWeaponOnDeadlineArgs e)
    {
        e.Action = TWeaponDeadlineAction.Drop;
    }

    public override TWeapon Copy() => new Snowball(Properties, Visuals)
    {
        NumberOfThrowablesLeft = NumberOfThrowablesLeft
    };
}
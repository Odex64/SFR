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
            DrawSoundID = "GrenadeDraw",
            VisualText = "Snowball"
        };

        TWeaponVisuals weaponVisuals = new()
        {
            AnimDraw = "UpperDrawThrown",
            AnimManualAim = "ManualAimThrown",
            AnimManualAimStart = "ManualAimThrownStart",
            AnimCrouchUpper = "UpperCrouch",
            AnimIdleUpper = "UpperIdle",
            AnimJumpKickUpper = "UpperJumpKick",
            AnimJumpUpper = "UpperJump",
            AnimJumpUpperFalling = "UpperJumpFalling",
            AnimKickUpper = "UpperKick",
            AnimStaggerUpper = "UpperStagger",
            AnimRunUpper = "UpperRun",
            AnimWalkUpper = "UpperWalk",
            AnimFullLand = "FullLandThrown",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("SnowballM");
        weaponVisuals.SetDrawnTexture("SnowballT");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        NumberOfThrowablesLeft = Properties.NumberOfThrowables;
    }

    private Snowball(TWeaponProperties weaponProperties, TWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = weaponProperties.NumberOfThrowables;
    }

    public override void OnBeforeBeginCharge(TWeaponBeforeBeginChargeArgs e)
    {
    }

    public override void OnThrow(TWeaponOnThrowArgs e)
    {
        if (e.Player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("GrenadeThrow", e.Player.Position, e.Player.GameWorld);
        }
    }

    public override void OnBeginCharge(TWeaponOnBeginChargeArgs e)
    {
    }

    public override void OnDrop(TWeaponOnThrowArgs e)
    {
    }

    public override void OnDeadline(TWeaponOnDeadlineArgs e) => e.Action = TWeaponDeadlineAction.Drop;

    public override TWeapon Copy() => new Snowball(Properties, Visuals)
    {
        NumberOfThrowablesLeft = NumberOfThrowablesLeft
    };
}
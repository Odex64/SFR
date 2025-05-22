using SFD;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Thrown;

internal sealed class Claymore : TWeapon
{
    internal Claymore()
    {
        TWeaponProperties weaponProperties = new(87, "Claymore", "WpnClaymore", false, WeaponCategory.Supply)
        {
            MaxCarriedTotalThrowables = 4,
            NumberOfThrowables = 2,
            ThrowObjectID = "WpnClaymoreThrown",
            DrawSoundID = "GrenadeDraw",
            VisualText = "Claymore"
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

        weaponVisuals.SetModelTexture("ClaymoreM");
        weaponVisuals.SetDrawnTexture("ClaymoreM");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        NumberOfThrowablesLeft = Properties.NumberOfThrowables;
    }

    private Claymore(TWeaponProperties weaponProperties, TWeaponVisuals weaponVisuals)
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
        if (e.Player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("GrenadeSafe", e.Player.Position, e.Player.GameWorld);
        }
    }

    public override void OnDrop(TWeaponOnThrowArgs e)
    {
    }

    public override void OnDeadline(TWeaponOnDeadlineArgs e) => e.Action = TWeaponDeadlineAction.Drop;

    public override TWeapon Copy() => new Claymore(Properties, Visuals)
    {
        NumberOfThrowablesLeft = NumberOfThrowablesLeft
    };
}
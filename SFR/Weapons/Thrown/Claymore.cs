using SFD;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Objects;

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
            DrawSoundID = "GrenadeDraw"
        };

        TWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("ClaymoreM");
        weaponVisuals.SetDrawnTexture("ClaymoreM");
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
        weaponProperties.VisualText = "Claymore";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = Properties.NumberOfThrowables;
    }

    private Claymore(TWeaponProperties weaponProperties, TWeaponVisuals weaponVisuals)
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
            var objectMineThrown = (ObjectClaymoreThrown)e.ThrowableObjectData;
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
        if (e.Player.GameOwner != GameOwnerEnum.Client && e.ThrowableIsActivated)
        {
            var objectMineThrown = (ObjectClaymoreThrown)e.ThrowableObjectData;
        }
    }

    public override void OnDeadline(TWeaponOnDeadlineArgs e)
    {
        e.Action = TWeaponDeadlineAction.Drop;
    }

    public override TWeapon Copy() => new Claymore(Properties, Visuals)
    {
        NumberOfThrowablesLeft = NumberOfThrowablesLeft
    };
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Helper;
using SFR.Objects;
using Constants = SFR.Misc.Constants;

namespace SFR.Weapons.Thrown;

internal sealed class StickyBomb : TWeapon
{
    private bool _primed;

    internal StickyBomb()
    {
        TWeaponProperties weaponProperties = new(91, "Sticky_Bombs", "WpnStickyBomb", false, WeaponCategory.Supply)
        {
            MaxCarriedTotalThrowables = 4,
            NumberOfThrowables = 2,
            ThrowObjectID = "WpnStickyBombThrown",
            ThrowDeadlineTimer = 2550f,
            DrawSoundID = "GrenadeDraw"
        };

        TWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("StickyBombM");
        weaponVisuals.SetDrawnTexture("StickyBombM");
        weaponVisuals.SetThrowingTexture("StickyBombT");
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
        weaponProperties.VisualText = "Sticky Bombs";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = Properties.NumberOfThrowables;
    }

    private StickyBomb(TWeaponProperties weaponProperties, TWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        NumberOfThrowablesLeft = weaponProperties.NumberOfThrowables;
    }

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        if (_primed && args.Player is { CurrentThrownWeapon: { } })
        {
            return Visuals.Throwing;
        }

        return base.GetDrawnTexture(ref args);
    }

    public override void OnBeforeBeginCharge(TWeaponBeforeBeginChargeArgs e) { }

    public override void OnThrow(TWeaponOnThrowArgs e)
    {
        _primed = false;
        if (e.Player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("GrenadeThrow", e.Player.Position, e.Player.GameWorld);
        }

        if (e.Player.GameOwner != GameOwnerEnum.Client && e.ThrowableIsActivated)
        {
            var objectGrenadeThrown = (ObjectStickyBombThrown)e.ThrowableObjectData;
            objectGrenadeThrown.ExplosionTimer = 4000;
        }
    }

    public override void OnBeginCharge(TWeaponOnBeginChargeArgs e)
    {
        if (e.Player.GameOwner != GameOwnerEnum.Server)
        {
            SoundHandler.PlaySound("GrenadeSafe", e.Player.Position, e.Player.GameWorld);
            var worldPosition = e.Player.Position + new Vector2(-(float)e.Player.LastDirectionX * 5f, 7f);
            Vector2 linearVelocity = new(-(float)e.Player.LastDirectionX * 2f, 2f);
            if (!_primed)
            {
                e.Player.GameWorld.CreateLocalTile("StickyBombDebris1", worldPosition, Constants.Random.NextFloat(-3f, 3f), (short)e.Player.LastDirectionX, linearVelocity, Constants.Random.NextFloat(-3f, 3f));
                e.Player.GameWorld.CreateLocalTile("StickyBombDebris1", worldPosition, Constants.Random.NextFloat(-3f, 3f), (short)e.Player.LastDirectionX, linearVelocity * 1.5f, Constants.Random.NextFloat(-3f, 3f));
                _primed = true;
            }
        }
    }

    public override void OnDrop(TWeaponOnThrowArgs e)
    {
        if (e.Player.GameOwner != GameOwnerEnum.Client && e.ThrowableIsActivated)
        {
            var objectGrenadeThrown = (ObjectStickyBombThrown)e.ThrowableObjectData;
            objectGrenadeThrown.ExplosionTimer = e.ThrowableDeadlineTimer;
        }
    }

    public override void OnDeadline(TWeaponOnDeadlineArgs e)
    {
        e.Action = TWeaponDeadlineAction.Drop;
    }

    public override TWeapon Copy() => new StickyBomb(Properties, Visuals)
    {
        NumberOfThrowablesLeft = NumberOfThrowablesLeft
    };
}
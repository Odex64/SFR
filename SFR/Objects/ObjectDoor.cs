using Box2D.XNA;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Projectiles;
using SFD.Sounds;

namespace SFR.Objects;

internal sealed class ObjectDoor : ObjectData
{
    internal bool IsOpen;

    internal ObjectDoor(ObjectDataStartParams startParams) : base(startParams) { }

    public override void Initialize()
    {
        Activateable = true;
        ActivateableHighlightning = true;
        ActivateRange = 28f;
        ToggleDoor(IsOpen);
    }

    public override void OnDestroyObject()
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            float radius = GetDecals()[0].Texture.Width * 0.25f;
            GameWorld.SpawnDebris(this, GetWorldPosition(), radius, ["WoodDebris00A", "WoodDebris00B", "WoodDebris00C"], FaceDirection);
            base.OnDestroyObject();
        }
    }

    public override void SetProperties() => Properties.Add(ObjectPropertyID.Common_hidden_bool_01);

    public override void PropertyValueChanged(ObjectPropertyInstance propertyChanged)
    {
        if (propertyChanged.Base.PropertyID == 13)
        {
            bool isOpen = (bool)propertyChanged.Value;
            ToggleDoor(isOpen);
        }
    }

    private void ToggleDoor(bool isOpen)
    {
        IsOpen = isOpen;
        Properties.Get(ObjectPropertyID.Common_hidden_bool_01).Value = isOpen;
        if (GameOwner != GameOwnerEnum.Server && !GameWorld.EditMode)
        {
            SoundHandler.PlaySound("ButtonPush1", GetWorldCenterPosition(), 1f, GameWorld);
        }

        Body.GetFixtureList().GetFilterData(out var filter);
        if (IsOpen)
        {
            filter.categoryBits = 0;
            filter.maskBits = 0;
        }
        else
        {
            filter.categoryBits = Tile.TileFixtures[0].Filter.box2DFilter.categoryBits;
            filter.maskBits = Tile.TileFixtures[0].Filter.box2DFilter.maskBits;

            foreach (var player in GameWorld.Players)
            {
                var playerArea = player.ObjectData.GetWorldAABB();
                var doorArea = GetWorldAABB();
                if (playerArea.Overlap(ref doorArea))
                {
                    player.SetNewWorldPosition(player.Position + new Vector2(player.LastDirectionX * 4f, 0f));
                }
            }
        }

        filter.kickable = !IsOpen;
        filter.punchable = !IsOpen;
        filter.blockFire = !IsOpen;
        filter.blockMelee = !IsOpen;
        filter.projectileHit = !IsOpen;
        filter.absorbProjectile = !IsOpen;
        Body.GetFixtureList().SetFilterData(ref filter);
        SyncedMethod(new(ObjectDataSyncedMethod.Methods.AnimationSetFrame, GameWorld.ElapsedTotalGameTime, IsOpen ? 1 : 0, true));
    }

    public override void Activate(ObjectData sender) => ToggleDoor(!IsOpen);

    public override void ProjectileHit(Projectile projectile, ProjectileHitEventArgs e)
    {
        if (!IsOpen)
        {
            base.ProjectileHit(projectile, e);
        }
    }

    public override void ImpactHit(ObjectData otherObject, ImpactHitEventArgs e)
    {
        if (!IsOpen)
        {
            base.ImpactHit(otherObject, e);
        }
    }

    public override void SolveImpactContactHit(Contact contact, ObjectData otherObject, Fixture myFixture, Fixture otherFixture,
        ref WorldManifold worldManifold, ref FixedArray2<PointState> pointStates, ref Manifold manifold)
    {
        if (!IsOpen)
        {
            base.SolveImpactContactHit(contact, otherObject, myFixture, otherFixture, ref worldManifold, ref pointStates, ref manifold);
        }
    }

    public override void PlayerMeleeHit(Player player, PlayerHitEventArgs e)
    {
        if (!IsOpen)
        {
            if (player.GameOwner != GameOwnerEnum.Client)
            {
                if (player.CurrentAction is PlayerAction.Kick or PlayerAction.JumpKick or PlayerAction.MeleeAttack3 && player.IsBot)
                {
                    ToggleDoor(true);
                }
            }

            base.PlayerMeleeHit(player, e);
        }
    }
}
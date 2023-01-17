using SFD;
using SFD.Objects;

namespace SFR.Objects;

internal sealed class ObjectInvisibleBlockWeak : ObjectDestructible
{
    internal ObjectInvisibleBlockWeak(ObjectDataStartParams startParams) : base(startParams, "", "") { }

    public override void Initialize()
    {
        base.Initialize();
        DoDraw = GameWorld.EditMode;
    }
}
using SFD;
using SFD.Objects;
using SFDGameScriptInterface;
using SFR.Game;

namespace SFR.Objects;

internal sealed class ObjectNukeTrigger : ObjectTriggerBase
{
    internal ObjectNukeTrigger(ObjectDataStartParams startParams) : base(startParams)
    {
        ScriptBridge = new ObjectScriptTriggerScriptBridge(this, (ObjectTriggerBaseScriptBridge)ScriptBridge);
        Use2XTexture();
    }

    public override void SetProperties()
    {
        Properties.Add(ObjectPropertyID.ScriptTerminatePlayers_TerminateType);
        Properties.Add(ObjectPropertyID.ScriptGameOverTrigger_GameOverText);
        Properties.Add(ObjectPropertyID.DestroyTargetsType);

        SetBaseTriggerProperties();
    }

    public override bool TriggerNode(BaseObject sender)
    {
        if (!GameWorld.CheckTriggerExecution(this, sender))
        {
            return false;
        }

        if (CheckScriptTriggerMethodToRunOK(ScriptBridge, sender))
        {
            TriggerTargetNodes(ScriptBridge);
        }

        if (GameOwner != GameOwnerEnum.Client && !NukeHandler.IsActive)
        {
            NukeHandler.CreateNuke(GameWorld, GetPlayerTerminateType(), (string)Properties.Get(ObjectPropertyID.ScriptGameOverTrigger_GameOverText).Value, GetObjectTerminateType());
        }

        return true;
    }

    public override void Initialize()
    {
        DoDraw = GameWorld.EditMode;
    }

    private PlayerTerminateType GetPlayerTerminateType() => (PlayerTerminateType)(int)Properties.Get(ObjectPropertyID.ScriptTerminatePlayers_TerminateType).Value;

    private ObjectTerminateType GetObjectTerminateType() => (ObjectTerminateType)(int)Properties.Get(ObjectPropertyID.DestroyTargetsType).Value;
}
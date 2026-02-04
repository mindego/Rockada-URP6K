using static ConditionChecker;

public class PulseEmitter : BaseEmitter
{
    public PulseEmitter(IAnimationServer gm, AnimationData data):base(gm, data) { UnityEngine.Debug.Log("Created new PulseEmitter "+ GetHashCode().ToString("X8"));  }

    public override bool update(float scale, bool visible = true)
    {
        if (visible && checkCondition(myData.myConds, myGame))
        {
            if (!isActivated() && checkWeight())
                activate();
        }
        else if (isActivated())
            deactivate();
        //UnityEngine.Debug.Log("Updating PulseEmitter " + GetHashCode().ToString("X8") + " status " + isActivated());
        return base.update(scale, visible);
    }
    private bool checkWeight()
    {
        return (myData.myWeight <= 0.0f || myGame.getWeight() + myData.myWeight <= 1.0f);
    }
};
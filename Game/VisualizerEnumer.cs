public class VisualizerEnumer : RoadElementEnumer
{
    SceneVisualizer vis;
    RoadElementState state;

    public bool ProcessElement(HMember m)
    {
        //Debug.Log("Processing VisEnum " + m.Object() + " [" + m.Object().GetType() + "] " + m.Object().GetFlags().ToString("X8"));
        //return true;
        IVisualPart p = (IVisualPart)(m.Object());

        switch (state)
        {
            case RoadElementState.POS_OLD:
                if (p.GetFlag(DataHasherDefines.RS_MODE_ALREADY_CREATED) != 0)
                    vis.ProcessRSection(p, true);
                break;
            case RoadElementState.POS_NEW:
                if ((p.GetFlag(DataHasherDefines.RS_MODE_ALREADY_CREATED)) == 0)
                    vis.ProcessRSection(p, false);
                break;
        }
        return true;

    }

    public void SetState(RoadElementState new_state)
    {
        state = new_state;
    }

    public RoadElementState GetState()
    {
        return state;
    }

    public VisualizerEnumer(SceneVisualizer _vis)
    {
        vis = _vis;
    }
}

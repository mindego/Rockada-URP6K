public static class ClassFactory
{
    public static IMessageProcessor CreateMessageProcessor(IMissionAi miss)
    {
        //MessageProcessor myObject = new RefMem<MessageProcessor>;
        MessageProcessor myObject = new MessageProcessor();
        if (!myObject.Initialize(miss))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static IEventProcessor CreateEventProcessor()
    {
        //EventProcessor myObject = new RefMem<EventProcessor>;
        EventProcessor myObject = new EventProcessor();
        if (!myObject.Initialize())
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static IMenuHolder CreateMenuHolder(string nm)
    {
        MenuHolder myobject = new MenuHolder();
        if (!myobject.Initialize(nm))
        {
            myobject.Release();
            myobject= null;
        }
        return myobject;
    }

    public static IMenuChanged CreateMenuChanger(IGroupAi grp, IMissionAi msn)
    {
        StdMissionAi std = (StdMissionAi) msn.Query(StdMissionAi.ID);
        if (std!=null)
        {
            MissionMenuChanged mn = new MissionMenuChanged();
            mn.Initialize(grp, std);
            return mn;
        }
        return null;
    }

    public static iEventDesigner CreateEventDesigner(IMissionAi m, IGroupAi g)
    {
        EventDesigner myobject = new EventDesigner();
        if (!myobject.Initialize(m, g))
        {
            myobject.Release();
            myobject= null;
        }
        return myobject;
    }

}

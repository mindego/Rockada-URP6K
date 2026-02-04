//using NonHashList = System.Collections.Generic.List<IHashObject>;
public class LightEss<L> : ILightEss where L : LightObject
{
    public LightEss(L light)
    {
        mObject = light;
    }
    public LIGHT GetLIGHT()
    {
        return mObject.GetLIGHT();
    }

    public void AddRef()
    {
        mObject.AddRef();
    }

    public int Release()
    {
        return mObject.Release();
    }

    public object Query(uint cls_id)
    {
        return mObject.Query(cls_id);
    }
    L mObject;
};
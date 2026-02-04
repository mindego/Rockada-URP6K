using DWORD = System.UInt32;

public class BaseItem : IBaseItem
{
    public const uint ID = 0x9EAD7039;
    DWORD OwnHandle;
    bool mIsRemote;

    protected BaseScene rScene;
    public uint GetHandle()
    {
        return OwnHandle;
    }

    public BaseScene getScene()
    {
        return rScene;
    }

    public bool IsLocal() { return !mIsRemote; }

    public bool IsRemote() { return mIsRemote; }

    public int SetRemote(char[] pData)
    {
        mIsRemote = true;
        return 0;
    }

    public bool SetLocal(int DataLength, char[] pData)
    {
        mIsRemote = false;
        return true;
    }

    public void Release()
    {
        rScene.ItemsArray.Sub(OwnHandle);
        OwnHandle = TArrayDefines.THANDLE_INVALID;
    }

    public BaseItem(BaseScene s,DWORD h,IBaseItem Owner)
    {
        rScene = s;

        if (h == Constants.THANDLE_INVALID)
        {

            //OwnHandle = (uint)this.GetHashCode();
            ////rScene.ItemsArray.Add((uint)this.GetHashCode(), this);
            //rScene.ItemsArray.Add(OwnHandle, real);
            //OwnHandle = rScene.ItemsArray.Add(this); //В исходниках Шторма - this, но это потому что объект может наследоваться от нескольких классов. C# это не позволяет, поэтому добавляеися "владелец" итема
            OwnHandle = rScene.ItemsArray.Add(Owner); 
            mIsRemote = false;
        }
        else
        {
            //OwnHandle = h;
            //if (rScene.ItemsArray.ContainsKey(h)) rScene.ItemsArray.Remove(h);
            //rScene.ItemsArray.Add(OwnHandle, real);
            rScene.ItemsArray.Set(OwnHandle = h, Owner); //см. выше
            mIsRemote = true;
        }
    }
}
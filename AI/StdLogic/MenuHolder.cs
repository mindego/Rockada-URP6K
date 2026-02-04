using DWORD = System.UInt32;

public class MenuHolder : IMenuHolder
{
    MenuItemHolder mpMenuRoot;
    IMenuChanged mpChanger;
    IRefMem mpRefMem = new RefMem();

    public MenuHolder()
    {
        mpMenuRoot = null;
        mpChanger = null;
    } 
    public bool Initialize(string nm)
    {
        mpMenuRoot = new MenuItemHolder();
        return true;
    }
    void Destroy()
    {
        mpMenuRoot = null;
        mpChanger = null;

    }

    // API
    public virtual IMenuItemHolder AddMenuItem(string name, DWORD hashed_name, string caption, string parent_item)
    {
        MenuItemHolder prnt;
        if (parent_item == null)
            prnt = mpMenuRoot;
        else
            prnt = mpMenuRoot.FindItem(Hasher.HshString(parent_item), true);
        if (prnt==null)
        {
            if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
            {
                AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "parent item \"{0}\" does`t exists", parent_item !=null ? parent_item : "Root");
            }
        }
        else
        {
            MenuItemHolder hld = prnt.FindItem(hashed_name);
            if (hld!=null)
            {
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "item \"{0}\" already exists", name!="" ? name : Parsing.sAiNothing);
                }
            }
            else
            {
                hld = new MenuItemHolder();
                hld.Initialize(hashed_name, caption, prnt);
                int n = hld.RefCount();
                hld.Release();
                if (n > 1 && mpChanger!=null)
                    mpChanger.MenuChanged(true);
                return (n > 1) ? hld : null;
            }
        }
        return null;

    }
    public virtual bool DeleteMenuItem(DWORD nm)
    {
        MenuItemHolder prnt = mpMenuRoot.FindItem(nm, true);
        if (prnt!=null)
        {
            prnt.PrepareToDestroy();
            if (mpChanger!=null)
                mpChanger.MenuChanged(false);
            return true;
        }
        else
            return false;
    }
    public virtual IMenuItemHolder FindItem(DWORD nm)
    {
        IMenuItemHolder hld=null;
        if (Constants.THANDLE_INVALID == nm)
            hld = mpMenuRoot;
        else
            hld = mpMenuRoot.FindItem(nm, true);
        if (hld!=null)
            hld.AddRef();
        return hld;

    }
    public virtual void SetMenuChangedCallback(IMenuChanged ch)
    {
        mpChanger = ch;
    }

    public void AddRef()
    {
        mpRefMem.AddRef();
    }

    public int Release()
    {
        return mpRefMem.Release();
    }

    public int RefCount()
    {
        return mpRefMem.RefCount();
    }
}
public class MenuItemHolder : IMenuItemHolder
{
    DWORD mName;
    string mpCaption;
    MenuItemHolder mpParent;
    IQuery mpQuery;
    RefMem mpRefmem = new RefMem();

    int mChildCount;
    MenuItemHolder[] mpChilds;

    // notification from childs
    void AddSubItem(MenuItemHolder hld)
    {
        MenuItemHolder[] new_childs = new MenuItemHolder[mChildCount + 1];
        for (int i = 0; i < mChildCount; ++i)
            new_childs[i] = mpChilds[i];
        if (mChildCount!=0)
            mpChilds = null;
        new_childs[mChildCount++] = hld;
        mpChilds = new_childs;
        hld.AddRef();
    }
    void DeleteSubItem(MenuItemHolder hld )
    {
        for (int i = 0; i < mChildCount; ++i)
            if (mpChilds[i] == hld)
            {
                mpChilds[i] = null;
                hld.Release();
                hld = null;
            }
        Asserts.AssertBp(mChildCount!=0);
        int n = mChildCount - 1;
        MenuItemHolder[] new_childs=null;
        if (n!=0)
        {
            new_childs = new MenuItemHolder[n];
            int k = 0;
            for (int j = 0; j < mChildCount; ++j)
                if (mpChilds[j]!=null)
                    new_childs[k++] = mpChilds[j];
            Asserts.AssertBp(k == n);
        }
        mChildCount = n;
        mpChilds = new_childs;

    }
    public MenuItemHolder FindItem(DWORD code, bool enum_subobj = false)
    {
        MenuItemHolder hld = null;
        for (int i = 0; i < mChildCount; ++i)
            if (mpChilds[i].GetName() == code)
            {
                hld = mpChilds[i];
                break;
            }
            else
            {
                if (enum_subobj)
                {
                    hld = mpChilds[i].FindItem(code, enum_subobj);
                    if (hld!=null) break;
                }
            }
        return hld;

    }

    public MenuItemHolder() { 
        mpCaption = "";
        mpParent = null;
        mChildCount = 0;
        mpChilds = null;
        mpQuery = null;
    }

    public bool Initialize(DWORD name, string caption, MenuItemHolder parentitem)
    {
        // save name
        mName = name;

        // save caption
        if (caption!="")
        {
            //int n = StrLen(caption);
            //mpCaption = new char[n + 1];
            //StrCpy(mpCaption, caption);
            mpCaption = caption;
        }

        mpParent = parentitem;
        if (mpParent!=null)
            mpParent.AddSubItem(this);

        return true;

    }
    public void PrepareToDestroy()
    {
        if (mpParent != null)
            mpParent.DeleteSubItem(this);
    }
    void Destroy()
    {
        if (mpCaption != "")
            mpCaption = ""; ;
        for (int i = 0; i < mChildCount; ++i)
            mpChilds[i].Release();
        if (mpChilds!=null)
            mpChilds =null;
    }

    // API
    public virtual int GetChildCount()
    {
        return mChildCount;

    }
    public virtual IMenuItemHolder GetChild(int i)
    {
        IMenuItemHolder hld = null;
        if (i >= 0 && i < mChildCount)
            hld = mpChilds[i];
        if (hld!=null)
            hld.AddRef();
        return hld;

    }
    public virtual DWORD GetName()
    {
        return mName;
    }
    public virtual string GetCaption()
    {
        return mpCaption;
    }
    public virtual IQuery GetIQueryData()
    {
        return mpQuery;
    }
    public virtual void SetIQueryData(IQuery qr) {
        mpQuery = qr;
    }

    public void AddRef()
    {
        mpRefmem.AddRef();
    }

    public int Release()
    {
        return mpRefmem.Release();
    }

    public int RefCount()
    {
        return mpRefmem.RefCount();
    }
}
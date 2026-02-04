using UnityEngine;

public class HUDTree
{
    HUDTree input;
    HUDTree next;
    HUDTree prev;
    IHUDObject data;
    void Cut()
    {
        if (prev != null)
        {
            if (prev.next == this)
                prev.next = next;
            else if (prev.input == this)
                prev.input = next;
            else Asserts.Assert(false);

        }
        if (next != null) next.prev = prev;
        next = null;
        prev = null;
    }
    public HUDTree()
    {
        next = null;
        input = null;
        prev = null;
        data = null;
    }
    public HUDTree(IHUDObject _t)
    {
        next = null;
        input = null;
        prev = null;
        data = _t;
        //_t.Tree = this;
        _t.SetTree(this);
    }

    public HUDTree Add(IHUDObject _t)
    {
        return Add(new HUDTree(_t));
    }

    public HUDTree Add(HUDTree _t)
    {
        HUDTree p = this;
        while (p.next!=null)
            p = p.next;
        p.next = _t;
        p.next.prev = p;
        return p.next;
    }

    public HUDTree Insert(IHUDObject _t)
    {
        return Insert(new HUDTree(_t));
    }

    public HUDTree Insert(HUDTree _t)
    {
        if (input!=null) return input.Add(_t);
        input= _t;
        input.prev = this;
        return input;
    }

    void Destroy()
    {
        data.Dispose();
        data = null;
        if (input!=null) input.Destroy();
        if (next!=null) next.Destroy();

    }
    public void Sub(bool destroy = true)
    {
        if (destroy)
        {
            if (input!=null) input.Destroy();
            data.Dispose();
        }
        if (input != null) input.Dispose();
        input= null;
        Cut();
        this.Dispose();
    }

    private void Dispose()
    {
        if (prev!=null)
        {
            if (prev.next == this) prev.next = null;
            else if (prev.input== this) prev.input= null;
            else Asserts.Assert(false);

        }

        if (input!=null) input.Dispose();
        if (next!=null) next.Dispose();
    }
    ~HUDTree()
    {
        Dispose();
    }
    //void Coll(int MouseState ,x,y,bool &AlreadyColl)
    public void Draw()
    {
        //Debug.Log("Drawing " + data);
        if (!data.IsHidden())
        {
            if (data!=null) data.BeginDraw();
            if (data!=null) data.Draw();
            if (input!=null) input.Draw();
            if (data!=null) data.EndDraw();
        }
        if (next!=null) next.Draw();
    }
    public void Update(float scale)
    {
        //Debug.Log("Updating " + data);
        if (data!=null) data.Update(scale);
        if (input!=null) input.Update(scale);
        if (next!=null) next.Update(scale);
    }

    public HUDTree In()
    {
        //return (this ?in:0);
        return input;
    }

    public HUDTree Next()
    {
        //return (this ? next : 0);
        return next;
    }

    public HUDTree Prev()
    {
        //return (this ? prev : 0);
        return prev;
    }

    public IHUDObject GetData()
    {
        //return (this ? data : 0);
        return data;
    }

};

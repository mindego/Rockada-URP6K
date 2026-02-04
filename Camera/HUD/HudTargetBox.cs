using UnityEngine;
using static HudData;

public class HudTargetBox : IHUDObject
{
    //Эмуляция множественного наследования
    IHUDObjectData hudobj = new HUDObjectImpl();

    IBill Bill;
    IFont StormFont;
    IBObject Angles;
    Vector2 TextXY;
    Vector2 TextXYUp;
    HudTargetBoxData Data;
    Texture2D Texture;
    HUDTexelData TexelData;
    public HudTargetBox(IBill _Bill, ITexturesDB _TexturesDB, HudTargetBoxData _Data, HUDTexelData _TexelData)
    {
        Bill = _Bill;
        Data = _Data;
        TexelData = _TexelData;
        StormFont = Bill.CreateFont("Small", 1, 1);
        Angles = Bill.Create(4 * 4, 4 * 2);
        Angles.Lock();
        Sq.Add(Angles, 0, Bill, Data.colors.colors[Data.colour], Data.opacity, 0f, 0f, 1f, 1f, TexelData.TargetNW);
        Sq.Add(Angles, 1, Bill, Data.colors.colors[Data.colour], Data.opacity, 0f, 0f, 1f, 1f, TexelData.TargetNE);
        Sq.Add(Angles, 2, Bill, Data.colors.colors[Data.colour], Data.opacity, 0f, 0f, 1f, 1f, TexelData.TargetSE);
        Sq.Add(Angles, 3, Bill, Data.colors.colors[Data.colour], Data.opacity, 0f, 0f, 1f, 1f, TexelData.TargetSW);
        Angles.UnLock();
        Texture = _TexturesDB.CreateTexture(TexelData.TextureName);
        Data.changed = true;
    }
    public void Update(float scale)
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        if (!Data.changed) return;
        float cx = Data.x;
        float cy = Data.y;
        float dx = Mathf.Clamp(Data.w / 2, Data.minsize * .5f, Data.maxsize * .5f);
        float dy = Mathf.Clamp(Data.h / 2, Data.minsize * .5f, Data.maxsize * .5f);
        float wb = Mathf.Min(Data.wb, 0.06f);
        float ddx = Mathf.Clamp(wb, 0, dx * 0.8f);
        float ddy = Mathf.Clamp(wb * 8.0f / 18.0f, 0, dy * 0.8f * 8.0f / 18.0f);
        Angles.Lock();
        Sq.Set(ref Angles, 0, Bill, cx - dx, cy - dy, cx - dx + ddx, cy - dy + ddy);
        Sq.Set(ref Angles, 1, Bill, cx + dx - ddx, cy - dy, cx + dx, cy - dy + ddy);
        Sq.Set(ref Angles, 2, Bill, cx + dx - ddx, cy + dy - ddy, cx + dx, cy + dy);
        Sq.Set(ref Angles, 3, Bill, cx - dx, cy + dy - ddy, cx - dx + ddx, cy + dy);
        Color myColor = Data.colors.colors[Data.colour];
        myColor *= Data.opacity;
        //Color.a*=Data.opacity;
        Angles.SetVertC(0, 4 * 4, myColor, Color.black);
        Angles.UnLock();
        TextXY = new Vector2(cx - dx, cy + dy);
        TextXYUp = new Vector2(cx - dx, cy - dy - StormFont.Height());
    }
    public void BeginDraw() { }
    public void Draw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        if ((Data.state & HUDSTATE_BAR) != 0)
            Bill.Draw(Angles);
        if ((Data.state & HUDSTATE_TEXT) != 0)
            StormFont.Puts(Data.colors.colors[Data.colour], TextXY, Data.text);
        StormFont.Puts(Data.colors.colors[Data.colour], TextXYUp, Data.textup);

    }
    public void EndDraw() { }
    public HudDeviceData GetData()
    {
        return Data;

    }

    public bool IsHidden()
    {
        return hudobj.IsHidden();
    }

    public void SetHide(bool off)
    {
        hudobj.SetHide(off);
    }

    public void SetTree(HUDTree tree)
    {
        hudobj.SetTree(tree);
    }

    public HUDTree GetTree()
    {
        return hudobj.GetTree();
    }

    ~HudTargetBox()
    {
        Dispose();
    }
    public void Dispose()
    {
        Texture = null;
        Angles.Release();
        StormFont.Release();
    }


}
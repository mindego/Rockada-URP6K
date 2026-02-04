using DWORD = System.UInt32;
using UnityEngine;

public interface IHUD
{
    public void Update(float scale);
    public void Draw(float[] ViewPort);
    public void Hide(bool off);
    //~IHUD();
    // devices
    public void ReleaseDevice(HudDevice h);
    public HudDevice CreateCompass(DWORD d);
    public HudDevice CreateVscale(DWORD d);
    public HudDevice CreateRecticle(DWORD d, int type);
    public HudDevice CreateMap(DWORD d, MapData md);
    public HudDevice CreateBar(DWORD d);
    public HudDevice CreateWeapon(DWORD d);
    public HudDevice CreateTarget(DWORD d);
    public HudDevice CreateAviaHorizon(DWORD h);
    public HudDevice CreateDamage(DWORD h);
    public HudDevice CreateFrame(DWORD h);
    //public HudDevice CreateLog(DWORD h, LogClientList l);
    public HudDevice CreateRing(DWORD h);
    public HudDevice CreateRecticles(DWORD h);
    public HudDevice CreateFPSmeter(DWORD h);
    public HudDevice CreateTable(DWORD h);
    public HudDevice CreateLabel(DWORD h);
    public HudDevice CreateLabelForEEI(string sName, DWORD iName, float _x, float _y, int color_index, float font_size, ushort aligned);
    public HudDevice CreateTableForEEI(string sName, DWORD iName, float _x, float _y, float _w, float _h, int columns);
    public HudDevice CreateViewPort(DWORD h);

    public static IHUD CreateHUD(BaseScene rScene)
    {
        return new HUD(rScene);
    }
};

class Sq
{ //Using as namespace
    public static IBObject Create(
        IBill Bill,
        Color32 Color,
        float opacity,
        float x1,
        float y1,
        float x2,
        float y2,
        float tx1,
        float ty1,
        float tx2,
        float ty2)
    {
        IBObject BObject = Bill.Create(4, 2);
        BObject.Lock();
        Add(BObject, 0, Bill, Color, opacity, x1, y1, x2, y2, tx1, ty1, tx2, ty2);
        BObject.UnLock();
        return BObject;

    }
    public static IBObject Create(
        IBill Bill,
        Color32 Color,
        float opacity,
        float x1,
        float y1,
        float x2,
        float y2, float[] txy)
    { return Create(Bill, Color, opacity, x1, y1, x2, y2, txy[0], txy[1], txy[2], txy[3]); }
    public static void Add(IBObject BObject, int n, IBill Bill, Color32 myColor, float opacity, float x1, float y1, float x2, float y2, float tx1, float ty1, float tx2, float ty2, bool Lock = false)
    {
        if (Lock) BObject.Lock();
        //Это углы 
        //TODO реализовать и это и цвет
        BObject.SetVertO(4 * n + 0, new Vector2(x1, y1), new Vector2(tx1 / 256f, ty1 / 256f));
        BObject.SetVertO(4 * n + 1, new Vector2(x1, y2), new Vector2(tx1 / 256f, ty2 / 256f));
        BObject.SetVertO(4 * n + 2, new Vector2(x2, y2), new Vector2(tx2 / 256f, ty2 / 256f));
        BObject.SetVertO(4 * n + 3, new Vector2(x2, y1), new Vector2(tx2 / 256f, ty1 / 256f));

        //Цвет
        //Color = Color * opacity;
        //BObject.SetVertC(4 * n, 4, Color, new Color32(0, 0, 0, 0));
        BObject.SetVertC(4 * n, 4, myColor, Color.black);

        BObject.SetFace(2 * n + 0, 4 * n + 0, 4 * n + 1, 4 * n + 2);
        BObject.SetFace(2 * n + 1, 4 * n + 0, 4 * n + 2, 4 * n + 3);

        BObject.SetBill(Bill);
        if (Lock) BObject.UnLock();
    }
    public static void Add(IBObject BObject, int n, IBill Bill, Color32 Color, float opacity, float x1, float y1, float x2, float y2, float[] txy, bool Lock = false)
    {
        Add(BObject, n, Bill, Color, opacity, x1, y1, x2, y2, txy[0], txy[1], txy[2], txy[3], Lock);
    }
    public static void AddR(IBObject BObject, int n, IBill Bill, Color32 Color, float opacity, float x1, float y1, float x2, float y2, float tx1, float ty1, float tx2, float ty2, bool Lock = false)
    {
        if (Lock) BObject.Lock();
        BObject.SetVertO(4 * n + 0, new Vector2(x1, y1), new Vector2(tx2 / 256f, ty1 / 256f));
        BObject.SetVertO(4 * n + 1, new Vector2(x1, y2), new Vector2(tx1 / 256f, ty1 / 256f));
        BObject.SetVertO(4 * n + 2, new Vector2(x2, y2), new Vector2(tx1 / 256f, ty2 / 256f));
        BObject.SetVertO(4 * n + 3, new Vector2(x2, y1), new Vector2(tx2 / 256f, ty2 / 256f));
        //Color = Color * opacity;
        BObject.SetVertC(4 * n, 4, Color, new Color32(0, 0, 0, 0));

        BObject.SetFace(2 * n + 0, 4 * n + 0, 4 * n + 1, 4 * n + 2);
        BObject.SetFace(2 * n + 1, 4 * n + 0, 4 * n + 2, 4 * n + 3);
        if (Lock) BObject.UnLock();
    }
    public static void Set(ref IBObject BObject, int n, IBill Bill, float x1, float y1, float x2, float y2, bool Lock = false)
    {

        if (Lock) BObject.Lock();
        BObject.SetVertO(4 * n + 0, new Vector2(x1, y1));
        BObject.SetVertO(4 * n + 1, new Vector2(x1, y2));
        BObject.SetVertO(4 * n + 2, new Vector2(x2, y2));
        BObject.SetVertO(4 * n + 3, new Vector2(x2, y1));
        if (Lock) BObject.UnLock();
    }
    public static void SetAsLine(IBObject BObject, int n, IBill Bill, float x1, float y1, float x2, float y2, float width, bool Lock = false)
    {
        float dx = x1 - x2;
        float dy = y1 - y2;
        float h = Mathf.Sqrt(dx * dx + dy * dy);

        if (h > 0)
        {
            dx = dx / h * width;
            dy = dy / h * width;
        }//else dx==dy==0

        if (Lock) BObject.Lock();
        BObject.SetVertO(4 * n + 0, new Vector2(x1 + dy, y1 - dx));
        BObject.SetVertO(4 * n + 1, new Vector2(x2 + dy, y2 - dx));
        BObject.SetVertO(4 * n + 2, new Vector2(x2 - dy, y2 + dx));
        BObject.SetVertO(4 * n + 3, new Vector2(x1 - dy, y1 + dx));
        if (Lock) BObject.UnLock();
    }
};
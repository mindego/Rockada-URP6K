using DWORD = System.UInt32;
using static HudDataColors;
using static HudData;

public class HudRecticlesData : HudDeviceData
{
    HudColors colors;
    public float speed;
    int var_speed;

    public TLIST<HudRecticleData> Items = new TLIST<HudRecticleData>();

    public HudRecticlesData(BaseScene pScene, DWORD iname = iRecticles,string sname = sRecticles) : base(pScene,iname,sname) { }
    ~HudRecticlesData()
    {
        ClearList();
    }

    public void ClearList()
    {
        Items.Free();
    }
    public HudRecticleData AddItem(HudRecticleData.TYPE _Type, float _w = 0, float _h = 0, DWORD _c = HUDCOLOR_NORMAL, DWORD uid = 0)
    {
        return Items.AddToTail(new HudRecticleData(_Type, _w, _h, _c, uid));
    }
    public void DeleteItem(HudRecticleData d )
    {
        Items.Sub(d).Dispose();
    }
};
using DWORD = System.UInt32;
using static HudData;

public class HudDamageData : HudDeviceData
{
    public string Texture;// [256];
    public bool NewTexture;
    public TLIST<DamageData> List = new TLIST<DamageData>();

    public HudDamageData(BaseScene pScene, DWORD iname = iDamage, string sname = sDamage) : base(pScene, iname, sname)
    {
        SetBitmapName("hud");
    }
    public override object OnVariable(uint code, object data)
    {
        //TODO: Реализовать обновление повреждемометра
        return base.OnVariable(code, data);
    }
    public void CreateNewPartdata(float[] xy, float[] txy,string name=null) {
        List.AddToTail(new DamageData(xy, txy,name));
    }
    public void ClearList()
    {
        List.Free();
    }
    public void SetBitmapName(string name) {
        if (name!=null)
        {
            //StrnCpy(Texture, name, sizeof(Texture));
            //Texture = name.Substring(0,256);
            Texture = name;
            NewTexture = true;
        }
    }

};

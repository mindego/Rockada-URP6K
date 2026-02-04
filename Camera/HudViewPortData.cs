using DWORD = System.UInt32;
using static HudData;

public class HudViewPortData : HudDeviceData
{

    float linewidth; int var_linewidth;
    float fontSize; int var_fontSize;
    int fontColor; int var_fontColor;
    float XmarkPos; int var_XmarkPos;
    float YmarkPos; int var_YmarkPos;
    //char mMarkText[2];
    public string mMarkText = "  "; //2

    public bool mEmpty;


    HudViewPortData(BaseScene pScene, DWORD iname = iViewPort, string sname = sViewPort) : base(pScene,iViewPort,sViewPort)
    {
        //TODO реализовать загрузку приборной камеры
    }
    public override object OnVariable(uint code ,object data)
    {
        //TODO реализовать обновление камеры
        return base.OnVariable(code, data);
    }

};
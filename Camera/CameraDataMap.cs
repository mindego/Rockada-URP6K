using UnityEngine;
/// <summary>
/// данные для радара и карты
/// </summary>
public class CameraDataMap : BaseData, CommLink
{
    // defines команд и перменных
    const string sCmMapRange = "cm_map_range";
    const uint iCmMapRange = 0xFDC18019;
    const uint CAMERA_MAP_DATA = 0xDD3774A5;

    private float MapRange;

    public float GetMapRange() { return MapRange; }
    public void SetMapRange(float r)
    {
        MapRange = Mathf.Clamp(r, 500f, 100000f);
    }
    public CameraDataMap(BaseScene s) : base(s, CAMERA_MAP_DATA)
    {
        rScene.GetCommandsApi().RegisterCommand(sCmMapRange, this, 1, "set map range");
    }
    ~CameraDataMap() { }

    public void Dispose()
    {
        rScene.GetCommandsApi().UnRegister(this);
    }

    // от CommLink
    public void OnCommand(uint code, string arg1, string arg2)
    {
        switch (code)
        {
            case iCmMapRange:
                if (arg1[0] == '-' || arg1[0] == '+') SetMapRange(MapRange + float.Parse(arg1));
                else SetMapRange(float.Parse(arg1));
# if _DEBUG
                rScene.Message("MapRange=%f", MapRange);
#endif
                return;
        }
    }
};
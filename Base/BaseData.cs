using DWORD = System.UInt32;
/// <summary>
/// базовый класс блока данных в игре
/// </summary>
public class BaseData
{
    protected DWORD Name;
    protected BaseScene rScene;
    public DWORD GetName() { return Name; }
    public BaseData(BaseScene pScene, DWORD name)
    {
        Name = name;
        rScene = pScene;
        rScene.DatasList.Add(this);
    }
    ~BaseData()
    {
        rScene.DatasList.Remove(this);
    }

}

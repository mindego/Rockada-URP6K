//using AI_STATE = System.Boolean;
using System.Collections.Generic;
using crc32 = System.UInt32;
public struct PartInfo
{
    public crc32 myName;
    public float myCondition;
};

interface IUnitService
{
    public const uint ID = 0x84B125F1;
    public void addDamage(string name, float coeff);
    public void fill(string[] names, ref List<PartInfo> tab);
    public void refresh(ref List<PartInfo> tab);
    public void stopRefresh();
    public void setFireMode(bool enable);
};

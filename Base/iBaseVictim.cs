using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// iBaseVictim: умеет получать пи№;,юлей
/// </summary>
public interface iBaseVictim : iBaseInterface
{
    public const uint WeaponCodeCollisionGround = 0x95069764; // CollisionGround
    public const uint WeaponCodeCollisionObject = 0x9A54BFE0; // CollisionObject
    public const uint WeaponCodeUltimateDeath = 0xBBA8B36D; // UltimateDeath
    new public const uint ID = 0xCF1F820B; // iBaseInterface
    public float GetCondition();
    public float GetLife();
    public float GetDamage();
    public float GetTotalLife();
    public void AddDamage(DWORD GadHandle, DWORD WeaponCode, float d);
    public void AddRadiusDamage(DWORD GadHandle, DWORD WeaponCode, Vector3 Org, float Xr, float Xd);

    public static string DescribeWeaponCode(uint code)
    {
        switch (code)
        {
            case WeaponCodeCollisionGround: return "WeaponCodeCollisionGround " + code.ToString("X8");
            case WeaponCodeCollisionObject: return "WeaponCodeCollisionObject " + code.ToString("X8");
            case WeaponCodeUltimateDeath: return "WeaponCodeUltimateDeath " + code.ToString("X8");
        }
        return "Unknown code: " + code.ToString("X8");
    }
};

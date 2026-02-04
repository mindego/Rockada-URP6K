using System.Collections;
using System.Collections.Generic;
using DWORD = System.UInt32;

public static class IHashApi
{
    public const uint HASH_VERSION = 0xC251300B;    // Hash 3.0

    public static IHash CreateHasher2(DWORD Version, TERRAIN_DATA terrain, float hash_size, float _sq_size, int poolsize, ILog _log, bool dummy)
    {
        if (Version != HASH_VERSION) return null;
        if (dummy)
            return new DummyHasher();
        else
            return new GlobalHasher(terrain, hash_size, _sq_size, poolsize, _log);
    }
}

public interface IHAccess : IQuery
{
    public int EnumPoly(RasterizeData r , uint _flags, HashEnumer e);
    public int EnumRect(geombase.Rect    _rt     , uint _flags, HashEnumer e);
    public int EnumSphere(geombase.Sphere   _sp    , uint _flags, HashEnumer e);
    public int EnumLine(Geometry.Line    _ln     , uint _flags,  HashLineEnumer _e);
};

public interface IHash : IRefMem, IHAccess
{
    new public const uint ID = (0x3CF02040);
    // update storage
    public IHashInfo GetHashInfo();
    /// <summary>
    /// Установка маски для вторичного кэша. Вторичный кэш используется, в основном, для размещения хэш-объектов света
    /// </summary>
    /// <param name="mask"></param>
    public void SetSecondCache(DWORD mask);
    public HMember RemoveMember(HMember Info);
    public HMember UpdateMember(HMember Info);
    public HMember RemoveMemberByLine(HMember Info);
    public HMember UpdateMemberByLine(HMember Info);
    public HMember UpdateMemberStatic(HMember Info);
    public HMember RemoveMemberStatic(HMember Info);
    //void EnumSphere(object value1, object value2, AreaDamageEnumer a);
}
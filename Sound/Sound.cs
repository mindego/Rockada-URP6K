using UnityEngine;
using crc32 = System.UInt32;
/*===========================================================================*\
|  Voice engine internal interface                                            |
\*===========================================================================*/


//#define SOUND_DS_VERSION 1

//#define _PROFILE 

//void _DbgOut( const char*, int, const char*, const char*);

//#if defined _DEBUG
//#define DEBUG_MSG(str)      _DbgOut( __FILE__, (int)__LINE__, 0, str )
//#define DEBUG_MSG2(hdr,str) _DbgOut( __FILE__, (int)__LINE__, str, hdr)
//#else
//#define DEBUG_MSG(str)      (0)
//#define DEBUG_MSG2(hdr,str) (0)
//#endif

//#define MSG2(hdr,str) _DbgOut( __FILE__, (int)__LINE__, str, hdr)

//const char* DSErr( HRESULT hr );
//__declspec(selectany) ILog* slog = 0;

//inline int SafeRelease(IUnknown* i) { return i ? i->Release() : 0; }
//inline ULONG RefCount(IUnknown *i) { i->AddRef(); return i->Release(); }


//#include "ListenerDs.hpp"
//#include "SoundDs.hpp"
//#include <Sound\IVoiceDBChecker.h>

public interface VoicesDB : IVoiceDBChecker
{
    public const uint SOUND_DS_VERSION = 1;
    new public const uint ID = 0xD38BCE51;
    public AudioClip LoadVoice(crc32 cCode, bool bCache);
    public AudioClip CreatePhrace(crc32[] pcCode, int Count);
    public void FreeCache();
    public IMappedDb GetDB();

    public static VoicesDB CreateVoicesDb(string db_name, Sound s, bool is_3d)
    {
        IMappedDb db = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED); //(*(int*)"DSFX"
        if (db.Open(ProductDefs.GetPI().getHddFile(db_name)) == DBDef.DB_OK)
        {
            _VoicesDB vdb = new _VoicesDB();
            vdb.Initialize(db, s, is_3d);
            return vdb;
        }
        else
        {
            db.Release();
            return null;
        }
    }

    public static VoicesDB QueryVDB(IObject db)
    {
        //return db !=null ? db.Query<VoicesDB>() : 0;
        return db != null ? (VoicesDB) db.Query(VoicesDB.ID) : null;
    }

}


// #include "SampleDs.hpp"


/*===========================================================================*\
|  Voices cached database                                                     |
\*===========================================================================*/

public class VolumeMeter
{
    static int[] values = new int[11];

     public static int Int2DB(int vol)
    {
        Asserts.Assert(vol >= 0 && vol <= 10);
        return values[vol];
    }

};

public interface IVoiceDBChecker : IObject
{
    public bool TestWave(crc32 cCode, bool Load);
};

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoadUtils
{

    public static void parseData(IMappedDb db, string msg, string block_name, string txt_name, string header, string key_name, InsertCall call)
    {
        string[] keys = { key_name };
        InsertCall[] calls = { call };
        parseMultiData(db, msg, block_name, txt_name, header, keys, calls);
    }

    //public void parseMultiData(Stream st,string msg,string block_name,string txt_name,string header, string[] keys, CARRIER_DATA.CarrierLoadCallbacks[] calls)

    public static void parseMultiData(IMappedDb db, string msg, string block_name, string txt_name, string header, string[] keys, InsertCall[] calls)
    {
        Stream st = db.GetBlock(block_name).myStream;
        parseMultiData(st, msg, block_name, txt_name, header, keys, calls);
        st.Close();
    }
    //public static void parseMultiData(PackType db, string msg, string block_name, string txt_name, string header, string[] keys, InsertCall[] calls) {
    //    Stream st = GameDataHolder.GetResource<Stream>(db, block_name);
        
    //    parseMultiData(st, msg, block_name, txt_name, header, keys, calls);
    //    st.Close();
    //}
    public static void parseMultiData(Stream st, string msg, string block_name, string txt_name, string header, string[] keys, InsertCall[] calls)
    {
        StormLog.LogMessage(msg,StormLog.logPriority.NORMAL);

        var f = new READ_TEXT_STREAM(st, ';', true, 0);
        string c = f.ReadLine(true);
        if (c != header)
        {
            StormLog.LogMessage($"Header loading error.  Expected '{header}', got '{c}'",StormLog.logPriority.NORMAL);
            return;
        }
        
        int cnt = 0;
        while (true)
        {
            c = f.GetNextItem(false, true);
            if (c == null) break;

            for (int i = 0; i < keys.Length;i++)
            {
                if (f.Recognize(keys[i]))
                {
                    calls[i](f);
                }
            }
            
            if (cnt++ > 40000)
            {
                Debug.Log(string.Format("Too many tokens {0}! Something totally wrong on parsing {1}",cnt,f.LineNumber()));
                break;
            }
        }
        st.Close();
    }

    public delegate void InsertCall(READ_TEXT_STREAM f);

    //public static SUBOBJ_DATA InsertSubobjData(SUBOBJ_DATA d, int l)
    //{
    //    ObjDatasHolder.DatasSubjObj.Add(d);
    //    return d;
    //}
}

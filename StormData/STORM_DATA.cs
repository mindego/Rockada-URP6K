using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DWORD = System.UInt32;
public class STORM_DATA
{
    // internal part

    // data section
    //static STORM_DATA_API DWORD crc;
    //static STORM_DATA_API const float GAcceleration;
    public const DWORD crc = 0xFFFFFFFF;
    public const float GAcceleration = 9.8f;

    public uint Name;
    public string FullName;

    // data access
    public uint GetName() { return Name; }

    //public STORM_DATA(uint name=0xFFFFFFFF, string fullName=null)
    public STORM_DATA() : this("undefined") { }
    public STORM_DATA(string name)
    {
        Name = Hasher.HshString(name);
        FullName = name;
    }

    public void Load(READ_TEXT_STREAM st)
    {
        if (st.GetNextItem() != "{") throw new System.Exception("Something wrong with data stream - Can't find opening '{'");
        while (true)
        {
            string c = st.GetNextItem();
            StormLog.LogMessage("Loading item: [" + c + "] size: " + c.Length + " as " + GetType());

            if (c == "}")
            {
                StormLog.LogMessage("Block loaded");
                break;

            }
            ProcessToken(st, c);
        }
    }

    public virtual void MakeLinks() { }


    public virtual void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        //StormLog.LogMessage("Process token " + this.GetType()); 
        throw new System.Exception(string.Format("Something wrong with data stream {0} {1}: {2}", FullName, this.GetType(), value));
        //StormLog.LogMessage("Something wrong with data stream: " + value);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(GetType().ToString());
        sb.AppendLine("FullName: [" + FullName + "]");
        sb.AppendLine("Name: [" + Name.ToString("X8") +"]");

        return sb.ToString();
    }


}

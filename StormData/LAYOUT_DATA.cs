using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#define ARMOR_LAYOUT    0xB78CC3B3  // ARMOR
//#define WEAPON_LAYOUT   0x66349F9F  // WEAPON
//#define SLOTS_LAYOUT    0xC0DF089F  // SLOTS


// ****************************************************************************
// data struct
public class LAYOUT_ITEM
{
    // internal part
    public LAYOUT_ITEM(string t, READ_TEXT_STREAM st)
    {
        //Storm.CRC32 crc = new Storm.CRC32();

        //Name = crc.HashString(t);
        Name = Hasher.HshString(t);
        Value = st.GetNextItem();
        TextName = t;
    }
    public LAYOUT_ITEM() { }
    public LAYOUT_ITEM(string ItemName, string itemValue)
    {
        TextName = ItemName;
        Value = itemValue;
        Name = Hasher.HshString(TextName);
    }
    // data section
    public uint Name;
    public string Value;
    public string TextName;

    public override string ToString()
    {
        return TextName + " : " + Value;
    }
}

// ****************************************************************************
// data struct
public class LAYOUT_DATA
{
    public const uint ARMOR_LAYOUT = 0xB78CC3B3;  // ARMOR
    public const uint WEAPON_LAYOUT = 0x66349F9F;  // WEAPON
    public const uint SLOTS_LAYOUT = 0xC0DF089F;  // SLOTS
    // internal part
    public void Init(READ_TEXT_STREAM st)
    {
        //Storm.CRC32 crc = new Storm.CRC32();
        //Type = crc.HashString(st.GetNextItem());
        Type = Hasher.HshString(st.GetNextItem());
        FullName = st.GetNextItem();
        Name = Hasher.HshString(FullName);
        Items = new List<LAYOUT_ITEM>();
        //StormLog.LogMessage($"Layout added {Type:X8} : {FullName}");

        string c = st.GetNextItem();
        if (c != "{") stormdata_dll.ParseError(Type.ToString("X8"), FullName);
        while (true)
        {
            c = st.GetNextItem();
            if (c == "}") break;
            Items.Add(new LAYOUT_ITEM(c, st));
        }
        //Debug.Log(string.Format("Imported LAYOUT_DATA [{0}] size {1} Type {2} Name {3}",FullName,Items.Count,Type.ToString("X8"),Name.ToString("X8")));
    }

    private string GetSlotType(uint id)
    {
        switch (id)
        {
            case ARMOR_LAYOUT:
                return "ARMOR_LAYOUT";
            case WEAPON_LAYOUT:
                return "WEAPON_LAYOUT";
            case SLOTS_LAYOUT:
                return "SLOTS_LAYOUT";
            default:
                return "YOU SHOULD NOT SEE THIS";
        }
    }
    // data section
    public uint Type;
    public uint Name;
    public string FullName;
    public List<LAYOUT_ITEM> Items;

    public override string ToString()
    {
        string res = "";
        res += FullName + "\n" +
            GetSlotType(Type);
        int i = 0;
        foreach (LAYOUT_ITEM item in Items)
        {
            res += "\n[" + (i++) + "] " + item.ToString();
        }
        return res;
    }
}

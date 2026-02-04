using UnityEngine;

public class CRC32
{
    public const uint CRC_NULL = 0xFFFFFFFF;
    uint[] table = new uint[256];
    public CRC32()
    {
        uint poly = 0xedb88320;
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 8; k != 0; k--)
                c = (c & 1) != 0 ? poly ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
    }
    public uint Code(string s)
    {
        uint code = 0xFFFFFFFF;
        //Debug.Log("Hashing " + s);
        foreach (byte c in s)
        {
          //  Debug.LogFormat("code ^ c {0} {1} {2}", (code ^ c).ToString("X8"),code.ToString("X8"),((int)c).ToString("X8"));
            code = table[(code & 0xFF) ^ c] ^ (code >> 8);
        }
        return code;
    }
    public uint Code(string s, int l)
    {
        uint code = 0xFFFFFFFF;
        foreach (byte c in s)
        {
            if (l-- == 0) break;
            code = table[(code&0xFF) ^ c] ^ (code >> 8);
        }
        return code;
    }

    public uint Code(uint code, string s)
    {
        foreach (byte c in s)
        {
            code = table[(code & 0xFF) ^ c] ^ (code >> 8);
        }
        return code;
    }

    public uint HashString(string s)
    {
        return s != null ? Code(s) : CRC_NULL;
    }
};

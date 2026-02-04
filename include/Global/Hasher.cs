using System.Collections.Generic;

public class Hasher
{
    public static Hasher instance;
    //private Storm.CRC32 crcCalculator;
    private CRC32 crcCalculator;
    private Dictionary<string, uint> Crc32Cache = new Dictionary<string, uint>();
    private Dictionary<string, uint> StormCodeCache = new Dictionary<string, uint>();


    public static Hasher GetInstance()
    {
        if (instance == null) instance = new Hasher();
        return instance;
    }

    public Hasher()
    {
        //crcCalculator = new Storm.CRC32();
        crcCalculator = new CRC32();
    }

    //public static uint HashString(string s)
    //{
    //    return s!=null ? Crc32.Code(s) : Storm.CRC32.CRC_NULL;
    //}

    //STUB
    public static uint HashString(string s)
    {
        return HshString(s);
    }

    public static uint HshString(string input)
    {
        //if (input == null | input == "") return 0;
        if (input == null | input == "") return CRC32.CRC_NULL;
        Hasher instance = Hasher.GetInstance();

        if (!instance.Crc32Cache.ContainsKey(input))
        {
            instance.Crc32Cache.Add(input, instance.crcCalculator.Code(input));
        }

        return instance.Crc32Cache[input];
    }

    //public static string GetName(uint id)
    //{
    //    Hasher instance = Hasher.GetInstance();
    //    if (instance.NamesCache.ContainsKey(id)) return instance.NamesCache[id];
    //    return null;
    //}

    public static string StringHsh(uint id)
    {
        Hasher instance = Hasher.GetInstance();

        foreach (KeyValuePair<string, uint> kvp in instance.Crc32Cache)
        {
            if (id == kvp.Value) return kvp.Key;
        }
        return null;
    }
    public static string StringCode(uint id)
    {
        Hasher instance = Hasher.GetInstance();

        foreach (KeyValuePair<string, uint> kvp in instance.StormCodeCache)
        {
            if (id == kvp.Value) return kvp.Key;
        }
        return null;
    }
    public static uint CodeString(string input)
    {
        Hasher instance = Hasher.GetInstance();
        if (!instance.StormCodeCache.ContainsKey(input))
        {
            instance.StormCodeCache.Add(input, instance.CdeString(input));
        }
        return instance.StormCodeCache[input];
    }
    public uint CdeString(string input)
    {

        //return HshString(input.ToLower());
        input = input.ToUpper();
        input = input.Replace('_', ' ');
        char[] c = new char[4];
        for (int i = 0; i < 4; i++)
        {
            c[i] = '\0';
        }
        int Pos = 0;
        foreach (char s in input)
        {
            //Debug.Log("Processing symbol [" + s + "]");
            c[Pos] += (char)(Pos ^ s);
            Pos = (Pos + 1) & 3;
        }

        int Res = 0;
        for (int i = 0; i < 4; i++)
        {
            var shiftBits = 8 * i;
            var mask = ~(0xff << shiftBits);
            Res = Res & mask | (c[i] << shiftBits);
        }
        return (uint)Res;
    }

    public static uint Code(uint v, string buffer)
    {
        //TODO странное. Надо использовать метод CRC32
        return HshString(v + buffer);
    }
}


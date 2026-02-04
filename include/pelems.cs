using UnityEngine;
using static pelems;
using static putils;
public static class pelems
{
    //public static bool IsEolChar(char x) { return x == 13 || x == 10; }
    public static bool IsEolChar(char x) { return x == '\r' || x == '\n'; }
    //public static bool IsEol(char x) { return IsEolChar(x) || x == 0; }
    public static bool IsEol(char x) { return IsEolChar(x) || x == '\0'; }
    public static string skipEols(string s)
    {
        int index = 0;
        while (IsEolChar(s[index])) ++index;

        return s.Substring(index, s.Length - index);
    }
    public static bool is_digit(char x) { return x >= '0' && x <= '9'; }      // 0..9
    public static bool is_hexchr(char x) { return x >= 'A' && x <= 'F'; }  // A..F
    public static bool is_letter(char x) { return x >= 'A' && x <= 'Z'; }  // A..Z
    public static bool is_char(char x) { return x == '_' || is_letter(char.ToUpper(x)); }
    public static bool is_space(char x) { return (x == ' ' || x == '\r' || x == '\n' || x == '\t'); }
    public static bool is_hex(char x) { return is_digit(x) || is_hexchr(char.ToUpper(x)); }

    public static int hex_value(char x) { return is_digit(x) ? int_value(x) : char.ToUpper(x) - 'A' + 10; }
    //public static int int_value(int x) { return x - '0'; }   // '5' -> 5
    public static int int_value(char x) { return int.Parse(new string(x, 1)); }

    public static bool is_AiStrEol(char x)
    {
        return is_space(x) || x == ',' || x == ')' || x == '\0';
    }

    public static int isQuote(char ch)
    {
        return (ch == '\"' || ch == '\'' || ch == '`') ? ch : 0;

    }


}


/// <summary>
/// Value  on stack
/// </summary>
public class Pelem
{
    protected string s;
    public int b, e;

    //public int s, e; //Индекс первого и последнего символа в общем тексте скрипта.
    //public string b; //строка 

    //public Pelem(string x) { e = s = next_char(b = x); }
    public Pelem(string x)
    {
        s = next_char(x);
        e = 0;
        b = 0;
    }

    public Pelem(Pelem v)
    {
        b = v.b;
        s = v.s;
        e = v.e;
    }

    public string Start() { return s; }
    public string End() { return e>=s.Length? null:s.Substring(e, s.Length - e); }
    public int Size() { return e - b; }

    public bool Valid() { return e != 0; }

    public char GetChar(int i) { return Start()[i]; }
    //operator bool()    { return e; }

    //public static char operator [] (int i)  { return s[i] ; }
}

/// <summary>
/// Desc  : One Char on stack
/// </summary>
class StrChar : Pelem
{
    public StrChar(string x) : base(x) { e++; }
    public StrChar(Pelem v) : base(v) { e++; }

    public char Char() { return s[0]; }

    //bool operator ==(char x) const { return Char() == x; }
    //bool operator !=(char x) const { return Char() != x; }
};

// Desc  : One String Word on stack : <char> [ [<digit>,<char>], ... ]
class StrWord : Pelem
{
    void Parce()
    {
        if (!is_char(s[e]))
            e = 0;
        else
        {
            do e++; while (is_char(s[e]) || is_digit(s[e]));
        }
    }


    public StrWord(string x) : base(x) { Parce(); }
    public StrWord(Pelem v) : base(v) { Parce(); }

    public string Word() { return s.Substring(0, e); }
    int WordLen() { return Word().Length; }
    //int Code() { return e != 0 ? Crc32.Code(Start(), WordLen()) : -1; }
    //public uint Code() { return e != 0 ? Hasher.HshString(s) : 0xFFFFFFFF; }
    public uint Code() { return e != 0 ? Hasher.HshString(Word()) : 0xFFFFFFFF; }

    public string CopyTo(out string dest) { return dest = Word(); }

    //bool static operator ==(int code)        { return Code() == code; }
    //bool static operator !=(int code)        { return Code() != code; }

    //bool operator ==(string x)          { return Code() == Crc32.Code(x); }
    //bool operator !=(string x)          { return Code() == Crc32.Code(x); }
};

/// <summary>
/// Desc  : One int on stack
/// </summary>
public class StrInt : Pelem
{
    int value;
    void Parce()
    {
        StrHex hx = new StrHex(this);
        if (hx.Valid())
        {
            value = hx.Value();
            //e = hx.End();
            e = hx.Size();
        }
        else
        {
            int minus, cnt = 0;
            if (s[e] == '-') { minus = 1; e++; } else minus = 0;
            while (is_digit(s[e])) { value = value * 10 + int_value(s[e++]); cnt++; }
            if (cnt == 0) e = 0; else if (minus != 0) value = -value;
        }
    }


    public StrInt(string x) : base(x)
    {
        value = 0;
        Parce();
    }
    public StrInt(Pelem v) : base(v)
    {
        value = 0;
        Parce();
    }

    public int Value() { return value; }
    //bool operator ==(int x)          { return value == x; }
    //bool operator !=(int x)          { return value != x; }
    //bool operator <=(int x)          { return value <= x; }
    //bool operator >=(int x)          { return value >= x; }
    //bool operator >(int x)          { return value >  x; }
    //bool operator <(int x)          { return value <  x; }
};

/// <summary>
/// Desc  : Hex Value on stack
/// </summary>
class StrHex : Pelem
{
    int value;
    void Parce()
    {
        if (s[0] != '0' || s[1] != 'x' || !is_hex(s[2]))
            e = 0;
        else
        {
            e += 2;
            do
            {
                value = (value << 4) + hex_value(s[e++]);
                //} while ((e - s) < 10 && is_hex(s[e]));
            } while (e < 10 && is_hex(s[e]));
        }
    }


    public StrHex(string x) : base(x)
    {
        value = 0;
        Parce();
    }
    public StrHex(Pelem v) : base(v)
    {
        value = 0;
        Parce();
    }

    public int Value() { return value; }

    //bool operator ==(int code)     { return value == code; }
    //bool operator !=(int code)     { return value != code; }
};
/// <summary>
/// Desc  : string in '"' or terminated by spaces, 0, ')', ','
/// </summary>
class AiScriptString : Pelem
{
    int quotes;
    void Parse()
    {
        if (s[e] == '(')
        {
            quotes = 0;
            string sE = CloseBracket(s.Substring(e + 1, s.Length - (e + 1)), '(', ')', 1);
            if (e != 0) ++e;
        }
        else
        {
            //Debug.Log("s[e]: " + s[e]);

            if ((quotes = isQuote(s[e])) != 0)
            {
                char x;
                for (x = s[++e]; x != 0 && x != quotes; x = s[++e]) ; //Debug.Log("\ts[e]: " + s[e]);
                e = x != 0 ? e + 1 : 0;
            }
            else
            {
                while (!is_AiStrEol(s[e]))
                {
                    //Debug.Log("IsAiStrEol? " + s[e]);
                    ++e;
                }
                //++e;
                //if (e == s) e = 0;
            }
        }
    }
    //string Param()  { return s + nQuotes(); }
    //string Param() { return s.Substring(nQuotes(), s.Length - nQuotes()); }
    string Param() { return s.Substring(0, e); }
    int nQuotes() { return quotes != 0 ? 1 : 0; }
    public char getQuote() { return quotes != 0 ? s[0] : '\0'; }

    public AiScriptString(string str) : base(str) { Parse(); }

    //int ParamLen() { return e - s - nQuotes() * 2; }
    public int ParamLen() { return Param().Length; }
    public string CopyTo(out string d)
    {
        d = Param();
        return d;
        //return __nstrcpy(d, Param(), ParamLen()); 
    }


    //int Code() { return e!=0 ? Crc32.Code(Param(), ParamLen()) : -1; }
    uint Code() { return e != 0 ? Hasher.HshString(Param()) : Constants.THANDLE_INVALID; }

};
/// <summary>
/// Desc  : One float on stack
/// </summary>
public class StrFloat : Pelem
{
    float value;
    void Parce()
    {
        int minus, cnt = 0;
        if (s[e] == '-') { minus = 1; e++; } else minus = 0;
        //Debug.LogFormat("char {0} digit? {1}", s[e],is_digit(s[e]).ToString());
        while (e < s.Length && is_digit(s[e]))
        {
            value = value * 10 + int_value(s[e++]); cnt++;
        }
        if (e < s.Length && s[e] == '.')
        {
            float pow = 1; e++;
            while (e<s.Length && is_digit(s[e]))
            {
                cnt++; pow /= 10; value += pow * int_value(s[e++]);
            }
        }
        if (cnt == 0) e = 0; else if (minus != 0) value = -value;
        //Debug.LogFormat("s{0} e{1} b{2}",s,e,b);
    }
    public StrFloat(string x) : base(x)
    {
        value = 0f;
        Parce();
    }
    public StrFloat(Pelem v) : base(v)
    {
        value = 0;
        Parce();
    }

    public float Value() { return value; }
    //bool operator ==(float x)          { return value == x; }
    //bool operator !=(float x)          { return value != x; }
    //bool operator <=(float x)          { return value <= x; }
    //bool operator >=(float x)          { return value >= x; }
    //bool operator >(float x)          { return value >  x; }
    //bool operator <(float x)          { return value <  x; }
};

/// <summary>
/// Desc  : string optionaly in '[' ...  ']'
/// </summary>

public class StrParam : Pelem
{
    void Parce()
    {
        StrChar open = new StrChar(s);

        if (open.Char() == '[')
        {
            bounds = 1; //TODO - что=то здесь не так с парсингом строки
            e += CloseBracketPos(open.Start(), '[', ']');
            if (e != 0) ++e;
        }
        else
        {
            bounds = 0;
            if (open.Char() == '\0')
            {
                e = -1;
            }
            else
            {
                StrWord2 par = new StrWord2(open.Start());
                //Debug.Log(par.Word());
                //Debug.Log(open.Start());

                e += par.e;
            }
        }
    }
    int bounds;  // 1 if '[]' else 0


    public StrParam(string x) : base(x) { Parce(); }
    public StrParam(Pelem v) : base(v) { Parce(); }

    //int ParamLen() { return e - s - bounds * 2; }
    int ParamLen() { return Param().Length; }
    //string Param() { return s + bounds; }
    string Param() { return s.Substring(bounds, s.Length - bounds - 1); }
    //string CopyTo(out string d) { return __nstrcpy(d, Param(), ParamLen());
    public string CopyTo(out string d)
    {
        return d = Param();
    }

    /// <summary>
    /// Desc: any char`s terminating by space or ';'
    /// </summary>
    public class StrWord2 : Pelem
    {
        void Parce()
        {
            //while (*e && *e != ';' && !is_space(*e)) ++e;
            while (e < s.Length && s[e] != ';' && !is_space(s[e])) ++e;
            // invalidate if len = 0
            if (WordLen() == 0) e = 0;


            //if (!is_char(s[e]))
            //    e = 0;
            //else
            //{
            //    do e++; while (is_char(s[e]) || is_digit(s[e]));
            //}
        }


        public StrWord2(string x) : base(x) { Parce(); }
        public StrWord2(Pelem v) : base(v) { Parce(); }

        public string Word() { return s.Substring(0, e); }
        int WordLen() { return Word().Length; }
        public uint Code() { return e != 0 ? Hasher.HshString(Word()) : 0xFFFFFFFF; ; }

        public string CopyTo(out string dest) { return dest = Word(); }

        //bool operator ==(int code)       const { return Code() == code; }
        //bool operator !=(int code)       const { return Code() != code; }

        //bool operator ==(cstr x)  const { return Code() == Crc32.Code(x); }
        //bool operator !=(cstr x)  const { return Code() == Crc32.Code(x); }
    }
}

/// <summary>
/// SmootherOrg1 - смуситель положения и скорости (кубический)
/// </summary>
public class SmootherOrg1
{

    // API
    public SmootherOrg1() { }
    public SmootherOrg1(Vector3 Org)
    {
        mTime0 = 0;
        mTime1 = 0;
        mCx[0] = Org.x; mCx[1] = 0; mCx[2] = 0; mCx[3] = 0;
        mCy[0] = Org.y; mCy[1] = 0; mCy[2] = 0; mCy[3] = 0;
        mCz[0] = Org.z; mCz[1] = 0; mCz[2] = 0; mCz[3] = 0;
    }
    public void SetScale(float Time0, float Time1)
    {
        mTime0 = Time0;
        mTime1 = Time1 - Time0;
    }
    public void SetOrg(Vector3 Org0, Vector3 Spd0, Vector3 Org1, Vector3 Spd1)
    {
        SetCoeffs(mCx, Org0.x, Spd0.x, Org1.x, Spd1.x);
        SetCoeffs(mCy, Org0.y, Spd0.y, Org1.y, Spd1.y);
        SetCoeffs(mCz, Org0.z, Spd0.z, Org1.z, Spd1.z);
    }
    public float GetScale(float Time)
    {
        return (Time - mTime0);
    }
    public void GetOrg(ref Vector3 Org, float Scale)
    {
        Org.x = GetValue(mCx, Scale);
        Org.y = GetValue(mCy, Scale);
        Org.z = GetValue(mCz, Scale);
    }
    public void GetSpeed(ref Vector3 Spd, float Scale)
    {
        Spd.x = GetSpeed(mCx, Scale);
        Spd.y = GetSpeed(mCy, Scale);
        Spd.z = GetSpeed(mCz, Scale);
    }
    public void GetAccel(ref Vector3 Acc, float Scale)
    {
        Acc.x = GetAccel(mCx, Scale);
        Acc.y = GetAccel(mCy, Scale);
        Acc.z = GetAccel(mCz, Scale);
    }
    private float GetTime0() { return mTime0; }
    public float GetTime1() { return mTime1 + mTime0; }

    // own
    private float GetValue(float[] C, float Scale)
    {
        if (Scale <= mTime1) return (C[0] + C[1] * Scale + C[2] * Mathf.Pow(Scale, 2) + C[3] * Mathf.Pow(Scale, 2) * Scale);
        return (GetValue(C, mTime1) + GetSpeed(C, mTime1) * (Scale - mTime1));
    }
    private float GetSpeed(float[] C, float Scale)
    {
        if (Scale > mTime1) Scale = mTime1;
        return (C[1] + 2 * C[2] * Scale + 3 * C[3] * Mathf.Pow(Scale, 2));
    }
    private float GetAccel(float[] C, float Scale)
    {

        if (Scale > mTime1) return 0;
        return (2 * C[2] + 6 * C[3] * Scale);
    }
    private void SetCoeffs(float[] C, float x0, float v0, float x1, float v1)
    {
        C[0] = x0;
        C[1] = v0;
        C[2] = (-3 * x0 - 2 * v0 * mTime1 + 3 * x1 - v1 * mTime1) / (mTime1 * mTime1);
        C[3] = (2 * x0 + v0 * mTime1 - 2 * x1 + v1 * mTime1) / (mTime1 * mTime1 * mTime1);
    }
    private float mTime0, mTime1;
    float[] mCx = new float[4];
    float[] mCy = new float[4];
    float[] mCz = new float[4];
};

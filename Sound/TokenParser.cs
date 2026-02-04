using crc32 = System.UInt32;
public class TokenParser
{
    public const string PhraseSpacial = "+";
    string mData;
    //string mPointer;
    int mPointer;
    string mSpecials; // i.e spacials = "+-;,";


    public TokenParser(string specs)
    {
        mSpecials = specs;
    }

    public TokenParser(string specs, string str)
    {
        mSpecials = specs;
        Init(str);
    }

    public void Init(string str) { mData = str+'\0'; Reset(); }
    public void Reset() { mPointer = 0; }
    public void Finish() { mPointer=mData.Length - 1; }

    //public bool IsFull() { return mPointer == mData.Length - 1; }
    public bool IsFull() { return mPointer == 0; }
    //bool IsFinished() { return mPointer == -1; }
    bool IsFinished() { return mPointer == mData.Length - 1; }
    bool IsSpec(int x) { 
        for (int i = 0; i < mSpecials.Length; i++) {
            if (mSpecials[i] == x) return true;
        }
        return false;
    }

    private bool is_space(int x) { return (x == ' ' || x == '\r' || x == '\n' || x == '\t'); }
    public Token GetNextToken()
    {
        char c = mData[mPointer];
        while (is_space(c)) c = mData[++mPointer];
        int p = mPointer;

        if (IsSpec(c))
            ++mPointer;
        else // accumulate not-spec token
            while (c!=0 && !is_space(c) && !IsSpec(c = mData[++mPointer])) ;

        return new Token(p, mPointer,mData);

    }
    public void SetPointer(Token t) { mPointer = t.mBegin; }
};

public struct Token
{
    public int mBegin;
    public int mEnd;
    string data;

    public Token(int begin, int end,string data="")
    {
        mBegin = begin;
        mEnd = end;
        this.data = data;
    }

    public char GetChar(int n)
    {
        if ((mBegin + n) > mEnd) return (char)0;
        return data[mBegin + n];
    }
    int GetLength() { return mEnd - mBegin; }
    string CopyTo(out string d) {
        //    return __nstrcpy(d, mBegin, mEnd - mBegin); 
        d = data.Substring(mBegin, GetLength());
        return d;
    }
    public bool IsEmpty() { return mEnd == mBegin; }
    //char operator() (int n) { return mBegin[n] ; }
    public char this[int n]
    {
        get
        {
            return data[mBegin+n];
        }
    }

    public crc32 GetCode()
    {
        //return Crc32.Code(mBegin, mEnd - mBegin);
        //return Hasher.HshString(data.Substring(mBegin, mEnd - mBegin));
        return Hasher.HshString(CopyTo(out string d));
    }
};
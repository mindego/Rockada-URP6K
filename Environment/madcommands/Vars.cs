using UnityEngine;
using crc32 = System.UInt32;

//////////////////////////////////////////////////////////////////////////////////////////
//// class Variabe - one console variable
//////////////////////////////////////////////////////////////////////////////////////////

public class Variable : ComBase
{
    ILog log;
    VType type;

    public Variable(string nm, crc32 code, string h, CommLink l, VType t, ILog lg) : base(nm, code, code, h, l)
    {
        type = t;
        log = lg;
        ctype = ComType.CM_VARIABLE;
    }


    public override string Exec(string input, bool inv)
    {
        StrChar c = new StrChar(input);
        if (c.Char() != ';')
            input = SetValue(c.Start(), inv);
        else
            if (!inv) PrintValue();
        return input;
    }

    private string SetValue(string input, bool nop)
    {
        //char buffer[sizeof(float) * 4];
        //union {
        object d;
        float f;
        int i;
        Vector4 v4;
        Vector2 v2;
        Vector3 v3;
        //};
        d = new string("");

        switch (type)
        {
            case VType.VAR_INT:
                {
                    StrInt pe = new StrInt(input);
                    if (pe.Valid())
                    {
                        i = pe.Value();
                        input = pe.End();
                    }
                    else
                        return null;
                }
                break;
            case VType.VAR_FLOAT:
                {
                    StrFloat pe = new StrFloat(input);
                    if (pe.Valid())
                    {
                        f = pe.Value();
                        input = pe.End();
                    }
                    else
                        return null;
                }
                break;
            case VType.VAR_SVEC4:
                {
                    v4 = new Vector4();
                    StrInt r = new StrInt(input);
                    if (r.Valid()) v4.x = r.Value(); else return null;
                    StrInt g = new StrInt(r.End());
                    if (g.Valid()) v4.y = g.Value(); else return null;
                    StrInt b = new StrInt(g.End());
                    if (b.Valid()) v4.z = b.Value(); else return null;
                    StrInt a = new StrInt(b.End());
                    if (a.Valid()) v4.w = a.Value(); else return null;
                    input = a.End();
                }
                break;
            case VType.VAR_FVEC2:
                {
                    v2 = new Vector2();
                    StrFloat f1 = new StrFloat(input);
                    if (f1.Valid()) v2.x = f1.Value(); else return null;
                    StrFloat f2 = new StrFloat(f1.End());
                    if (f2.Valid()) v2.y = f1.Value(); else return null;
                    input = f2.End();
                }
                break;
            case VType.VAR_VECTOR:
                {
                    v3 = new Vector3();
                    StrFloat f1 = new StrFloat(input);
                    if (f1.Valid()) v3.x = f1.Value(); else return null;
                    StrFloat f2 = new StrFloat(f1.End());
                    if (f2.Valid()) v3.y = f2.Value(); else return null;
                    StrFloat f3 = new StrFloat(f2.End());
                    if (f3.Valid()) v3.z = f3.Value(); else return null;
                    input = f3.End();
                }
                break;
            case VType.VAR_TEXT:
                {
                    StrParam myparam = new StrParam(input);
                    if (myparam.Valid())
                    {
                        //char* p = (char*)_alloca(param.ParamLen() + 1);
                        string p;
                        myparam.CopyTo(out p);
                        d = p;
                    }
                    else return null;
                    input = myparam.End();
                }
                break;
        }
        if (!nop) link.OnVariable(cname, d);
        return input;
    }
    void PrintValue()
    {
        //union {
        object d;
        //float f;
        //int i;
        //Vector4 v4; //Тут правильнее Color
        //Vector2 v2;
        //Vector3 v3;
        //};

        d = link.OnVariable(pname, null);

        if (d == null)
        {
            log.Message("Error: variable {0] is writeonly", name);
            return;
        }

        switch (type)
        {
            case VType.VAR_INT:
                int i = (int)d;
                log.Message("{0} = {1}", name, i.ToString());
                break;
            case VType.VAR_FLOAT:
                float f = (float)d;
                //log.Message("{0} = %.4f", name, *f);
                log.Message("{0} = {1}", name, f.ToString());
                break;
            case VType.VAR_SVEC4:
                Vector4 v4 = (Vector4)d;
                log.Message("%s = %d %d %d %d", name, v4.x, v4.y, v4.z, v4.w);
                break;
            case VType.VAR_FVEC2:
                Vector2 v2 = (Vector2)d;
                log.Message("%s = %.4f %.4f", name, v2.x, v2.y);
                break;
            case VType.VAR_VECTOR:
                Vector3 v3 = (Vector3)d;
                log.Message("%s = %.4f %.4f %.4f", name, v3.x, v3.y, v3.z);
                break;
            case VType.VAR_TEXT:
                log.Message("%s = %s", name, d.ToString());
                break;
        }
    }
};

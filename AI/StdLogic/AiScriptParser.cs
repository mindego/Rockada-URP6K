using System.Collections;
using UnityEngine;
using crc32 = System.UInt32;
using static pelems;
using static putils;


/// <summary>
/// AiScriptParser
/// </summary>
public class AiScriptParser
{
    public const int PE_EXP_CMD_SEQ_ID = 1;  // "expected a command or sequence identifier";
    public const int PE_EXP_LBR_FORPARAMS = 2;  // "expected a '(' to open param block";
    public const int PE_EXP_RBR_FORPARAMS = 3;// "expected a ')' to close param block"
    public const int PE_EXP_CMD_OR_SEQ = 4;// "expected a ';' to close command or '{' to open sequence";
    public const int PE_EXP_CLOSE_SEQ = 5;  // "expected a '}' to close sequence";
    public const int PE_NO_SEQUENCE = 6;  // "no such sequence";
    public const int PE_NO_COMMAND = 7; // "no such command"
    public const int PE_EXP_PARAMETER = 8; // "expected a parameter identifier"
    public const int PE_NO_PARAMETER = 9; // "no such parameter"
    public const int PE_EXP_OPERATOR = 10;// "expected operator ('=', '!=', '<', '<=', '>', '>=')"
    public const int PE_BAD_OPERATOR = 11; // "operator not supported by command or sequence"
    public const int PE_BAD_INTEGER = 12;// "expected integer value"
    public const int PE_BAD_FLOAT = 13; // "expected float value"
    public const int PE_BAD_STRING = 14; // "expected string value, ex: \"\\\"Alpha 12\\\"\", \"Alpha12\""
    public const int PE_BAD_VECTOR = 15;// "expected vector value, vector is \"(<float>,<float>,<float>)\""
    public const int PE_EXP_PAR_COMMA = 16; // "expected comma :','"
    public const int PE_FAIL_AFTERPARSE = 17; // "internal parser error"
    public const int PE_ILLEGAL_PAR_TYPE = 18; // "internal parser error"

    static string parseOperators(string s, out IParamList.OpType op)
    {
        //Debug.Log("parseOperators searching in [" + s + "]");
        s = next_char(s);
        op = IParamList.OpType.OP_ERR;
        switch (s[0])
        {
            case '=': op = IParamList.OpType.OP_EQU; s = s.Substring(1, s.Length - 1); break;
            case '!': if (s[1] == '=') { op = IParamList.OpType.OP_NEQU; s = s.Substring(2, s.Length - 2); } else s = null; break;
            case '<': if (s[1] == '=') { op = IParamList.OpType.OP_LWRE; s = s.Substring(2, s.Length - 2); } else { op = IParamList.OpType.OP_LWR; s = s.Substring(1, s.Length - 1); } break;
            case '>': if (s[1] == '=') { op = IParamList.OpType.OP_GRE; s = s.Substring(2, s.Length - 2); } else { op = IParamList.OpType.OP_GR; s = s.Substring(1, s.Length - 1); } break;
            default: s = null; break;
        }
        //Debug.Log("parseOperators found " + op);
        return s;
    }
    static bool prepareParsing(string text, IParamUser user, IErrorLog el)
    {
        bool ret = user.isParsingCorrect();
        if (!ret)
        {
            el.Error(text, PE_FAIL_AFTERPARSE);
        }
        return ret;
    }
    public static bool parceAiScript(string text, IVmSequence seq, IVmFactory fct, IErrorLog el, char term = '\0')
    {
        //Debug.Log("Start parceAiScript [" + text + "]");
        if (!text.EndsWith('\0')) text += '\0'; //TODO вот не нужно так делать - фигня выходит. Правильнее было бы переписать весь модуль парсинна
        int cnt = 0;
        for (string i = text; ;)
        {
            if (cnt++ > 500)
            {
                Debug.LogError("Script too big. Something wrong");
                break;
            }
            if (i.Length == 0) break;
            //StrChar ch = new StrChar(i);
            StrChar ch;
            try
            {
                ch = new StrChar(i);
            }
            catch
            {
                Debug.Log("Failed to parse [" + i + "] from [" + text + "]");
                throw;
            }

            if (ch == null) break;
            try
            {
                if (ch.Char() == term) break;
            } catch
            {
                Debug.Log("Failed to parse [" + i + "] from [" + text + "]");
                break;
            }

            StrWord id = new StrWord(ch.Start());

            if (!id.Valid())
                return el.Error(id.Start(), PE_EXP_CMD_SEQ_ID);

            StrChar lbr = new StrChar(id.End());
            if (lbr.Char() != '(')
                return el.Error(lbr.Start(), PE_EXP_LBR_FORPARAMS);

            string rbr = CloseBracket(lbr.End(), '(', ')', 1);
            if (rbr == null)
                return el.Error(lbr.Start() + 1, PE_EXP_RBR_FORPARAMS);

            //StrChar dc = new StrChar(rbr.Substring(1, rbr.Length - 1));
            StrChar dc = new StrChar(rbr.Remove(0, 1)); //Следующий символ после закрывающей скобки
            if (dc.Char() != ';' && dc.Char() != '{')
                return el.Error(dc.Start(), PE_EXP_CMD_OR_SEQ);
            if (dc.Char() == '{')
            {
                //Debug.Log("Creating from [" + dc.End() + "]");
                //string cb = CloseBracket(dc.End(), '{', '}', 1);
                int cb = CloseBracketPos(dc.End(), '{', '}', 1);
                if (-1 == cb)
                    return el.Error(dc.Start().Substring(1, dc.Start().Length - 1), PE_EXP_CLOSE_SEQ);

                //char buffer[64];
                //__nstrcpy(buffer, id.Start(), id.End() - id.Start());
                string buffer = id.Start().Substring(0, id.Size());
                // Debug.Log("Ai script buffer " + buffer);

                IVmSequence sub_seq = seq.addSequence(buffer, fct);

                if (sub_seq != null)
                {
                    //Debug.Log("lbr.End() " + lbr.End());
                    if (parseParameters(lbr.End(), sub_seq.getParamList(), el)
                        && prepareParsing(id.Start(), sub_seq, el)
                        && parceAiScript(dc.End(), sub_seq, fct, el, '}'))
                    {
                        //Debug.Log("Command parsed: " + buffer);
                    }
                    else
                        return false;
                }
                else
                {
                    Debug.Log(string.Format("PE_NO_SEQUENCE {0} buffer {1} seq {2}", id, buffer, seq));
                    return el.Error(id.Start(), PE_NO_SEQUENCE);
                }

                //i = cb + 1; // i это указатель, то есть теперь i должен указывать на следующий символ после закрывающей скобки }
                //char[] c = dc.End().ToCharArray();
                //c[cb] = '*';
                //Debug.Log(string.Format("Replaced @{0} : {1}" ,cb, new string(c)));
                //i = i.Remove(0, cb + 1);
                i = dc.End().Remove(0, cb + 1);
                //Debug.Log(string.Format("[{0}] {1} of {2}",i,i.Length,text));
            }
            else
            {
                //char buffer[64];
                //__nstrcpy(buffer, id.Start(), id.End() - id.Start());
                string buffer = id.Start().Substring(0, id.Size());
                //Debug.Log(string.Format("Creating using id {0} buffer {1} seq {2} of [{3}", id, buffer, seq, i));
                //Debug.Log("dc.End() [" + dc.End() + "] "); ;
                IVmCommand com = seq.addCommand(buffer, fct);

                if (com != null)
                {
                    if (parseParameters(lbr.End(), com.getParamList(), el)
                        && prepareParsing(id.Start(), com, el))
                    {
                    }
                    else
                        return false;
                }
                else
                {
                    Debug.Log("[FAILED!!!] [" + buffer + "]");
                    return el.Error(id.Start(), PE_NO_COMMAND);
                }

                //i = dc.End() // i это указатель, то есть теперь в i должен быть необработанный остаток строки.
                i = dc.End();
            }
        }
        return true;
    }

    static bool parseParameters(string text, IParamList com, IErrorLog el)
    {
        StrChar ch = new StrChar(text);
        int cnt = 0;
        if (ch.Char() != ')')
        {
            for (string s = ch.Start(); ;)
            {
                if (cnt++ > 4096) throw new System.Exception("Infinite loop an parse parseParameters");
                StrWord pid = new StrWord(s);
                //Debug.Log("As word: [" + pid.Word() + "]");

                if (!pid.Valid())
                    return el.Error(pid.Start(), PE_EXP_PARAMETER);

                crc32 pCode = pid.Code();
                string param_name;
                pid.CopyTo(out param_name);
                //Debug.Log("param_name [" + param_name + "] " + com + " code " + pCode.ToString("X8"));

                IParamList.PInfo info = com.getParamInfo(pCode);

                if (info.myType == IParamList.VarType.SPT_NONE)
                {
                    return el.Error(pid.Start(), PE_NO_PARAMETER);
                }

                IParamList.OpType op;
                s = parseOperators(pid.End(), out op);

                if (s == null)
                    return el.Error(next_char(pid.End()), PE_EXP_OPERATOR);

                //TODO Обдумать и исправить неопределённое состояние op в обработчике параметров
                if (!(op != IParamList.OpType.OP_ERR & info.myOpSet != 0))
                    return el.Error(next_char(pid.End()), PE_BAD_OPERATOR);
                //Debug.Log(string.Format("Loading {0} from {1}", s, info.myType));
                switch (info.myType)
                {
                    case IParamList.VarType.SPT_INT:
                        {
                            //Debug.Log(string.Format("Loading {0} from {1}", s , typeof(StrInt)));
                            StrInt n = new StrInt(s);
                            if (!n.Valid())
                                return el.Error(n.Start(), PE_BAD_INTEGER);
                            com.addParameter(pid.Code(), op, new IParamList.Param(n.Value()), param_name);
                            //Debug.Log(string.Format("Loaded {0} value {1}", s, n.Value()));
                            s = n.End();
                        }
                        break;

                    case IParamList.VarType.SPT_FLOAT:
                        {
                            StrFloat n = new StrFloat(s);
                            if (!n.Valid())
                                return el.Error(n.Start(), PE_BAD_FLOAT);
                            com.addParameter(pid.Code(), op, new IParamList.Param(n.Value()), param_name);
                            //Debug.Log(string.Format("Loaded from {0} value {1}", s, n.Value()));
                            s = n.End();
                        }
                        break;

                    case IParamList.VarType.SPT_STRING:
                        {
                            AiScriptString n = new AiScriptString(s);
                            if (!n.Valid())
                                return el.Error(n.Start(), PE_BAD_STRING);
                            //char* buf = ANewN(char, n.ParamLen() + 1);
                            //n.CopyTo(buf);
                            string buf;
                            n.CopyTo(out buf);
                            com.addParameter(pid.Code(), op, new IParamList.Param(buf), param_name);
                            s = n.End();
                        }
                        break;

                    case IParamList.VarType.SPT_VECTOR:
                        {
                            float[] v = new float[3];
                            string q = parseVector(s, ref v);
                            if (q == null)
                                return el.Error(next_char(s), PE_BAD_VECTOR);
                            com.addParameter(pid.Code(), op, new IParamList.Param(v), param_name);
                            s = q;
                        }
                        break;
                    default:
                        return el.Error(pid.Start(), PE_ILLEGAL_PAR_TYPE);
                }
                //Debug.Log("Value: " + com.getParamInfo(pid.Code()));
                ch = new StrChar(s);
                //Debug.Log("End of ParseParameters inner, value [" + s + "] char " + ch.Char());
                s = ch.End();
                if (ch.Char() == ')') break;
                //Debug.Log("Continue for some reason");
                if (ch.Char() != ',')
                    return el.Error(ch.Start(), PE_EXP_PAR_COMMA);
            }
        }
        //Debug.Log("End parseParameters " +  text);
        return true;
    }

    static string parseVector(string s, ref float[] vec)
    {
        char ch;
        char[] sep = { ',', ',', ')' };
        s = ParseChar(s, out ch);
        if (ch != '(') return null;
        for (int i = 0; i < 3; ++i)
        {
            s = ParseFloat(s, out vec[i]);
            if (s == null) return null;
            Debug.Log("sep " + i);
            s = ParseChar(s, out ch);
            if (ch != sep[i]) return null;
        }
        return s;
    }

    private static string ParseChar(string str, out char v)
    {
        str = next_char(str);
        v = str[0];
        return str.Substring(1, str.Length - 1);

    }
    private static string ParseFloat(string str, out float v)
    {
        str = next_char(str);
        v = 0f;
        bool minus;
        if (str[0] == '-')
        {
            //str = str.Substring(1, str.Length - 1);
            str = str.Remove(0, 1);
            minus = true;
        }
        else
        {
            minus = false;
        }
        float value = 0;
        int cnt = 0; // valid symbol counter
        // get integer part
        for (; is_digit(str[0]); ++cnt, str = str.Remove(0, 1))
            value = value * 10 + int_value(str[0]);
        // get partial part
        if (str[0] == '.')
        {
            float pow = 1; str = str.Remove(0, 1);
            for (; is_digit(str[0]); ++cnt, str = str.Remove(0, 1))
            {
                pow /= 10; value += pow * int_value(str[0]);
            }
        }
        if (cnt == 0)
        {
            str = null;
        }
        else v = minus ? -value : value;

        return str;
    }
    private static string discardComments(string text)
    {
        string buffer = "";
        string res = "";
        for (int i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '/':
                    if (text[i + 1] == '/')
                    {
                        while (true)
                        {
                            if (text[++i] == '\n') break;
                        }
                    }
                    break;
                case '\r':
                    i++;
                    if (buffer != "")
                    {
                        res += buffer + '\n';
                        buffer = "";
                    }
                    break;
                default:
                    buffer += text[i];
                    break;
            }
        }
        return res;
    }

    public static string getAiScriptError(int id)
    {
        switch (id)
        {
            case PE_EXP_CMD_SEQ_ID: return "expected a command or sequence identifier";
            case PE_EXP_LBR_FORPARAMS: return "expected a '(' to open param block";
            case PE_EXP_RBR_FORPARAMS: return "expected a ')' to close param block";
            case PE_EXP_CMD_OR_SEQ: return "expected a ';' to close command or '{' to open sequence";
            case PE_EXP_CLOSE_SEQ: return "expected a '}' to close sequence";
            case PE_NO_SEQUENCE: return "no such sequence";
            case PE_NO_COMMAND: return "no such command";
            case PE_EXP_PARAMETER: return "expected a parameter identifier";
            case PE_NO_PARAMETER: return "no such parameter";
            case PE_EXP_OPERATOR: return "expected operator ('=', '!=', '<', '<=', '>', '>=')";
            case PE_BAD_OPERATOR: return "operator not supported by command or sequence";
            case PE_BAD_INTEGER: return "expected integer value";
            case PE_BAD_FLOAT: return "expected float value";
            case PE_BAD_STRING: return "expected string value, ex: \"\\\"Alpha 12\\\"\", \"Alpha12\"";
            case PE_BAD_VECTOR: return "expected vector value, vector is \"(<float>,<float>,<float>)\"";
            case PE_EXP_PAR_COMMA: return "expected comma :','";
            case PE_FAIL_AFTERPARSE: return "after parsing initialization failed";
            case PE_ILLEGAL_PAR_TYPE:
            default: return "internal parser error";
        }
    }
}



interface IStormAiScripImportable<T>
{
    public string ParseT(string s, out T res);
}
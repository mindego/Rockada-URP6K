using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
public class Condition
{
    public enum OpType
    {
        OP_EQU = 0x01,  // =
        OP_NEQU = 0x02,  // !=
        OP_GR = 0x04,  // >
        OP_GRE = 0x08,  // >=
        OP_LWR = 0x10,  // <
        OP_LWRE = 0x20,  // <=
    };

    public Condition(string name, OpType op, float val, bool nd)
    {
        myFullName = name;
        myName = Hasher.HashString(name);
        myOperator = op;
        myValue = val;
        myAnd = nd;
    }
    public crc32 myName;
    public OpType myOperator;
    public float myValue;
    public bool myAnd;
    public string myFullName;


    public static string parseOperators(string s, out Condition.OpType op)
    {
        s = putils.next_char(s);
        //Debug.Log("S before: " + s);
        op = OpType.OP_EQU;
        switch (s[0])
        {
            case '=': op = Condition.OpType.OP_EQU; s = s.Remove(0, 1); break;
            case '!': if (s[1] == '=') { op = Condition.OpType.OP_NEQU; s=s.Remove(0, 2); } else s = null; break;
            case '<': if (s[1] == '=') { op = Condition.OpType.OP_LWRE; s = s.Remove(0, 2); } else { op = Condition.OpType.OP_LWR; s = s.Remove(0, 1); } break;
            case '>': if (s[1] == '=') { op = Condition.OpType.OP_GRE; s = s.Remove(0, 2); } else { op = Condition.OpType.OP_GR; s = s.Remove(0, 1); } break;
            default: s = null; break;
        }
        //Debug.Log("S after: " + s + " res " + op);
        return s;
    }
    public static bool parseCondition(ref List<Condition> cnds, string str)
    {
        string buffer;
        string s = str;
        while (s != null)
        {
            StrWord var = new StrWord(s);
            //Debug.Log("using word: " + var.Word() + " tail ->" + var.End());
            if (!var.Valid()) return false;
            Condition.OpType op;
            s = parseOperators(var.End(), out op);
            if (s == null)
            {

                Debug.Log("Failed to obtain operator from: " + var.End());
                return false;
            };

            //Debug.Log("new s value: " + s);
            //Debug.Log("Operator: " + op);
            StrFloat n = new StrFloat(s);
            //Debug.Log("Value: " + n.Value());
            //Debug.Log("Head: " + n.Start());
            //Debug.Log("Tail: " + n.End());
            if (!n.Valid()) return false;
            s = n.End();
            bool nd = false;
            if (s != null)
            {
                StrWord bp = new StrWord(s);
                bp.CopyTo(out buffer);
                //UnityEngine.Debug.Log("Loading buffer: [" + buffer + "]");
                switch (Hasher.HashString(buffer))
                {
                    case 0xF80A6492: nd = true; s = bp.End(); break;
                    case 0xE2488A78: nd = false; s = bp.End(); break;
                    default: s = null; break;
                }
                if (s == null) return false;
            }
            var.CopyTo(out buffer);
            cnds.Add(new Condition(buffer, op, n.Value(), nd));
        }
        return true;
    }

};
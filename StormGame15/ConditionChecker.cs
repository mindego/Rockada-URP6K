using System.Collections.Generic;

public static class ConditionChecker
{

    public static void apply(ref bool ret, bool value, bool op)
    {
        ret = op ? ret && value : ret || value;
    }

    //public static bool checkCondition(AnyDTab<Condition> cnd, IAnimationVariables srv)
    public static bool checkCondition(List<Condition> cnd, IAnimationVariables srv)
    {
        bool ret = true;
        for (int i = 0; i < cnd.Count; i++)
        {
            float val = srv.getFloat(cnd[i].myName);
            //srv.getLog().Message("%s=%f",cstr(cnd[i].myFullName),val);
            float cmp_val = cnd[i].myValue;
            bool and = i == 0 ? true : cnd[i - 1].myAnd;
            switch (cnd[i].myOperator)
            {
                case Condition.OpType.OP_EQU: apply(ref ret, val == cmp_val, and); break;
                case Condition.OpType.OP_NEQU: apply(ref ret, val != cmp_val, and); break;
                case Condition.OpType.OP_GR: apply(ref ret, val > cmp_val, and); break;
                case Condition.OpType.OP_LWR: apply(ref ret, val < cmp_val, and); break;
                case Condition.OpType.OP_GRE: apply(ref ret, val >= cmp_val, and); break;
                case Condition.OpType.OP_LWRE: apply(ref ret, val <= cmp_val, and); break;
            }
        }
        return ret;
    }
}
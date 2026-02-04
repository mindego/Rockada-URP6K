using UnityEngine;
using crc32 = System.UInt32;
public interface IVm : IObject
{
    public const uint THANDLE_INVALID = 0xFFFFFFFF;
    public bool parseScript(string text, string namesp = "");
    public void reset();
    public void run();
    public int getThreadCount();
    public void setFactory(IVmFactory f);
    public int enumCommands(ref IEnumer<IVmCommand> en);
};

public interface IEnumer<T>
{
    public void process(T t);
};

public interface IVmController : IParamUser
{
    public bool getResume();
    public crc32 getID();
    public void shutdown();
    public void restart();
    public void setID(crc32 id);
};

public interface IVmVariablePool : IObject
{
    public bool getVariable(crc32 name, out IParamList.Param param );
    public void setVariable(crc32 name, IParamList.Param param);
};


public interface IVmCommand : IParamUser
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>returns false if not executed , true if complete execute</returns>
    public bool run();  // returns 0 if not executed , true if complete execute
    public void onOverride();
    public void onCreate();
};

public interface IVmFactory : IObject
{
    public IVmCommand createCommand(crc32 name, IVmVariablePool pool = null, IVmController cont = null);
    public IVmController createController(crc32 name);
};

public interface IVmSequence : IParamUser
{
    public IVmCommand addCommand(string name, IVmFactory fct);
    public IVmSequence addSequence(string name, IVmFactory fct);
};

public interface IParamUser : IObject
{
    public IParamList getParamList();
    public bool isParsingCorrect();
    public string getName();
    public string describeParams(ref string myparams);
    public int getType();
};

public interface IParamList
{
    enum VarType
    {
        SPT_NONE,
        SPT_INT,
        SPT_FLOAT,
        SPT_STRING,
        SPT_VECTOR
    };

    class Param
    {
        public Param() {
            myType = VarType.SPT_NONE;
        }
        public Param(int x)
        {
            myType = VarType.SPT_INT;
            myInt = x;
        }
        public Param(float x)
        {
            myType = VarType.SPT_FLOAT;
            myFloat = x;
        }
        public Param(string x)
        {
            myType = VarType.SPT_STRING;
            myString = x;

        }
        public Param(float x, float y, float z)
        {
            myType = VarType.SPT_VECTOR;
            myVector = new Vector3(x, y, z);
        }
        public Param(float[] v)
        {
            myType=VarType.SPT_VECTOR;
            myVector = new Vector3(v[0], v[1], v[2]);
        } 

        public Param(Vector3 v)
        {
            myType = VarType.SPT_VECTOR;
            myVector = v;
        }

        VarType myType;
        //    union {
        //        int myInt;
        //    float myFloat;
        //    cstr myString;
        //    float myVector[3];
        //};
        public int myInt;
        public float myFloat;
        public string myString;
        //float myVector[3];
        public Vector3 myVector;

        internal string GetValue()
        {
            switch (myType)
            {
                case VarType.SPT_INT:
                    return myInt.ToString();
                case VarType.SPT_FLOAT:
                    return myFloat.ToString();
                case VarType.SPT_STRING:
                    return myString;
                case VarType.SPT_VECTOR:
                    return myVector.ToString();
                default:
                    return "Undefined";
            }
        }
    }
    enum OpType
    {
        OP_EQU = 0x01,  // =
        OP_NEQU = 0x02,  // !=
        OP_GR = 0x04,  // >
        OP_GRE = 0x08,  // >=
        OP_LWR = 0x10,  // <
        OP_LWRE = 0x20,  // <=
        OP_ALL = OP_EQU | OP_NEQU | OP_GR | OP_LWR | OP_GRE | OP_GRE,
        OP_ERR=0xFF,
    };

    class PInfo
    {
        public PInfo()
        {
            myType = VarType.SPT_NONE;
            myOpSet = 0;
        }
        public PInfo(VarType t, int op_set) //TODO возможно, стоит держать OpType вместо int
        {
            myType = t;
            myOpSet = op_set;
        }

        public PInfo(VarType t, OpType op_set) 
        {
            myType = t;
            myOpSet = (int) op_set;
        }
        public VarType myType;
        public int myOpSet;  // combinations of valid OpType values
    };

    public PInfo getParamInfo(crc32 param_name);
    public bool addParameter(crc32 param_name, OpType op, Param p, string real_name);


    public static string getOpSign(OpType ops)
    {
        if (((int)ops & (int)OpType.OP_EQU)!=0) return "=";
        if (((int)ops & (int)OpType.OP_NEQU)!= 0) return "!=";
        if (((int)ops & (int)OpType.OP_GR)!= 0) return ">";
        if (((int)ops & (int)OpType.OP_GRE)!= 0) return ">=";
        if (((int)ops & (int)OpType.OP_LWR)!= 0) return "<";
        if (((int)ops & (int)OpType.OP_LWRE)!= 0) return "<=";
        Asserts.Assert(false);
        return null;
    }

    public static bool evalCondition(int val1, int val2,OpType ops)
    {
        if (((int)ops & (int)OpType.OP_EQU) != 0) return val1 == val2;
        if (((int)ops & (int)OpType.OP_NEQU) != 0) return val1 != val2;
        if (((int)ops & (int)OpType.OP_GR) != 0) return val1 > val2;
        if (((int)ops & (int)OpType.OP_GRE) != 0) return val1 >= val2;
        if (((int)ops & (int)OpType.OP_LWR) != 0) return val1 < val2;
        if (((int)ops & (int)OpType.OP_LWRE) != 0) return val1 <= val2;
        Asserts.Assert(false);
        return false;
    }
};



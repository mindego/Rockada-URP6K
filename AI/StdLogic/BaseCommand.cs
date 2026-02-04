using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCommand : IVmCommand, IParamList
{
    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(GetType().ToString());
        sb.Append(myPool.ToString());
        return sb.ToString();
    }
    // IParamList
    public virtual IParamList getParamList()
    {
        return this;
    }

    public virtual bool isParsingCorrect() { return true; }

    public virtual string getName()
    {
        return myCommandName;
    }

    public virtual int getType()
    {
        return mySender != null ? mySender.getType() : 0;
    }

    // BaseCommand
    public virtual bool restart() { return true; }
    //public virtual bool exec() { return false; }
    public abstract bool exec();

    //public virtual bool setDefaults(IQuery i) { return false; }
    public abstract bool setDefaults(IQuery i);

    public bool initialize(string name, IQuery srv, IVmVariablePool pool)
    {
        myPool = pool;
        myTimer = (ITimeService)srv.Query(ITimeService.ID);
        myInitialized = false;
        myIsSequence = false;
        myCommandName = name;
        mySender = (IRadioSender)srv.Query(IRadioSender.ID);
        Debug.Log(string.Format("Command {3} initialization: sender [{0}] timer [{1}] defaults {2} ", mySender, myTimer,setDefaults(srv),this.GetType().ToString())) ;
        return mySender != null && myTimer != null && setDefaults(srv);
    }

    // IVmCommand
    public virtual bool run()
    {
        if (checkRestart() == false)
            return false;
        return exec() ? needRestart() : false;
    }
    public virtual void onOverride() { }
    public virtual void onCreate() { needRestart(); }
    protected bool myInitialized;
    protected bool myIsSequence;
    protected bool checkRestart() { if (!myInitialized) myInitialized = restart(); return myInitialized; }
    protected bool needRestart() { myInitialized = false; return true; }

    public abstract string describeParams(ref string myparams);
    public void AddRef() { }
    public int RefCount() { return 0; }
    public int Release() { return 0; }
    //public virtual IParamList.PInfo getParamInfo(uint param_name) { return new IParamList.PInfo(); } //TODO возомжно, здесь требуется иной подход.
    public abstract IParamList.PInfo getParamInfo(uint param_name);
    public virtual bool addParameter(uint param_name, IParamList.OpType op, IParamList.Param p, string real_name) { return false; }

    protected ITimeService myTimer;
    protected IVmVariablePool myPool;
    protected string myCommandName;
    protected IRadioSender mySender;
    /*
        void printMessage(cstr message, cstr st_from) {
            cstr call = getCallsign();
            cstr name = getName();
            if (call &&  name)
                dprintf("%f: %p %s %s : %s %s %s\n",myTimer->getTime(),this,name,call,message,st_from?"from":"",st_from?st_from:"");
        }

        cstr getCallsign() {
            cstr call = mySender->getInfo().myCallsign;
            if (!call) call = "<unknown>";
            describeParams(local_buffer+wsprintf(local_buffer,"%s ",call));
            return local_buffer;
        }
      */

    public static string descFloat(string myparams, string name, float val)
    {
        return myparams + (isFloatValid(val) ? string.Format("{0}={1,2:N1} ", name, val) : null); //TOOO вернуть описатор в границы "%s=%2.1f "
    }

    public static string descVector(string myparams, string name, Vector3 val)
    {
        return myparams+string.Format("{0}=({1} {2} {3} ", name, val.x, val.y, val.z);
    }
    public static string descStrings(string myparams, string name, List<string> tb)
    {
        if (tb.Count != 0)
            //params += Sprintf (params,"%s=[ ",name);
            myparams += string.Format("{0}=[ ", name);

        for (int i = 0; i < tb.Count; ++i)
            myparams += "[" + i + "] "+addValue(myparams, tb[i]) + "\n";
        //myparams += addValue(myparams, tb[i]);

        if (tb.Count != 0) myparams += string.Format("] ");
        return myparams;
    }

    public static string addValue(string myparams, string value)
    {
        return (isStringValid(value) ? string.Format("\"{0}\" ", value) : null);
    }

    public static string descString(string myparams, string name, string val)
    {
        return myparams + (isStringValid(val) ? string.Format("{0} = \"{1}\" ", name, val) : null);
    }

    public static string descInt(string myparams, string name, int val)
    {
        return myparams + (isIntValid(val) ? string.Format("{0}={1} ", name, val) : null);
    }
    public static string descInt(string myparams, string name, uint val)
    {
        return myparams + (isIntValid(val) ? string.Format("{0}={1} ", name, val) : null);
    }

    public static string descInts(string myparams, string name, List<int> tb)
    {
        for (int i = 0; i < tb.Count; ++i)
        myparams = descInt (myparams, name, tb[i]);
        return myparams;
    }

    public static string descInts(string myparams, string name, List<uint> tb)
    {
        for (int i = 0; i < tb.Count; ++i)
            myparams = descInt(myparams, name, tb[i]);
        return myparams;
    }

    public static bool isFloatValid(float val)
    {
        return !float.IsNaN(val);
    }

    public static bool isStringValid(string val)
    {
        //return val && *val;
        return val != null;
    }

    public static bool isIntValid(int val)
    {
        return true;
        //return val != INT_NULL();
    }
    public static bool isIntValid(uint val)
    {
        return true;
        //return val != INT_NULL();
    }

    protected static int INT_NULL()
    {
        return 0;
        //return null;
    }
}

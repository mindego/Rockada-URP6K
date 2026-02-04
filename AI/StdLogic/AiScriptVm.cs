using System;
using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class AiScriptVm
{
    const string msg_ParametersStart = "(";
    const string msg_ParametersEnd = ")";
    const string msg_ParametersEmpty = "";
    public static IVm createAiScriptVm(IErrorLog log)
    {
        //Vm vm = new SRefMem<Vm>();
        Vm vm = new Vm();
        vm.initialize(log);
        return vm;
    }

    public static void describeParamUser(string action, int type, IErrorLog log, IParamUser cmd)
    {
        if (log != null)
        {
            string name = cmd.getName();
            if (name != null)
            {
                ///ZString str;
                string myparams = null;
                string printed = cmd.describeParams(ref myparams);
                //bool empty = myparams.Length == printed.Length; 
                //bool empty = myparams != printed;
                bool empty = printed == null;//TODO - Сомнительно! Надо разобраться, что должно быть в результате генерации описания параметра
                log.printMessage(type, "A[{0}] N[{1}] {2} {3} {4}",
                    action,
                    name,
                    empty ? msg_ParametersEmpty : msg_ParametersStart,
                    empty ? msg_ParametersEmpty : myparams,
                    empty ? msg_ParametersEmpty : msg_ParametersEnd);
            }
        }
    }
}

public class ThreadData : IVmSequence, IParamList
{
    // IParamList added because needed main function
    public virtual bool isParsingCorrect() { return myCommands[0].isParsingCorrect(); }

    public virtual string getName()
    {
        return myCommands[0].getName();
    }

    public virtual int getType()
    {
        return myCommands[0].getType();
    }

    public virtual string describeParams(ref string myparams)
    {
        return myCommands[0].describeParams(ref myparams);
    }

    public virtual IParamList getParamList() { return myParams; }

    public virtual IParamList.PInfo getParamInfo(crc32 param_name)
    {
        return new IParamList.PInfo();
    }
    public virtual bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        return false;
    }

    public virtual IVmSequence addSequence(string name, IVmFactory fct)
    {
        ThreadData dt = new ThreadData();
        if (dt.setController(name, fct))
            addThreadData(dt, fct);
        else
            dt = null;
        return dt;
    }

    public virtual IVmCommand addCommand(string name, IVmFactory fct)
    {
        return addCommandLocal(name, fct);
    }

    public bool setController(string name, IVmFactory fct)
    {
        //Debug.Log(string.Format("Creating Controller {0} {1}", name, Hasher.HshString(name).ToString("X8")));
        myController = fct.createController(Hasher.HshString(name));
        return (myController != null && addCommandLocal(name, fct, myController) != null);
    }

    void addThreadData(ThreadData td, IVmFactory fct)
    {
        addCommand("vm_create", fct);

        //ThreadCreateCommand cmd = (ThreadCreateCommand)myCommands[myCommands.Count - 1].Query(ThreadCreateCommand.ID);
        ThreadCreateCommand cmd = (ThreadCreateCommand)myCommands[myCommands.Count - 1];
        cmd = (ThreadCreateCommand)cmd.Query(ThreadCreateCommand.ID);
        cmd.setThreadData(td);
    }

    public int getCommandsCount()
    {
        return myCommands.Count;
    }

    public int enumCommands(IEnumer<IVmCommand> en)
    {
        int n = 0;
        for (int i = 0; i < myCommands.Count; i++)
        {
            IVmCommand cmd = myCommands[i];
            ThreadCreateCommand tcc = (ThreadCreateCommand)cmd.Query(ThreadCreateCommand.ID);
            if (tcc != null)
                n += tcc.myTD.enumCommands(en);
            else
            {
                en.process(cmd);
                n++;
            }
        }
        return n;
    }

    IVmCommand addCommandLocal(string name, IVmFactory fct, IVmController cont = null)
    {
        //Debug.Log(string.Format("start addCommandLocal({0},{1},{2} controller [{3}]", name, Hasher.HshString(name).ToString("X8"), fct, cont != null ? cont : "Empty"));
        IVmCommand cmd = fct.createCommand(Hasher.HshString(name), myVars, cont);
        if (cmd != null)
        {
            if (myCommands.Count == 0)
            {
                myParams = cmd.getParamList();
            }
            myCommands.Add(cmd);
        }
        //Debug.Log(string.Format("{3} addCommandLocal({0},{1},{2}", name, Hasher.HshString(name).ToString("X8"), fct, cmd != null ? "success" : "failed"));
        return cmd;
    }

    public ThreadData()
    {
        myVars = new VariablePool();
        myParams = null;
    }

    public virtual crc32 getID() { return myController != null ? myController.getID() : CRC32.CRC_NULL; }
    public virtual void shutdown() { if (myController != null) myController.shutdown(); }

    public void AddRef()
    {
        return;
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        //STUB!
        return 0;
        //throw new System.NotImplementedException();
    }

    public List<IVmCommand> myCommands = new List<IVmCommand>();
    IParamList myParams;
    public IVmController myController;
    IVmVariablePool myVars;
}

class StormThread :IDisposable
{
    public StormThread(ThreadData d, IErrorLog log)
    {
        myData = d;
        myLog = log;
        myData.AddRef();
        rewind();
        getTop().onCreate();
        run();
    }

    public void Dispose()
    {

    }

    public bool run()
    {
        while (!isFinished())
        {
            IVmCommand cmd = getCurrent();
            if (cmd.run())
            {
                //Debug.Log("Runned " + cmd);
                AiScriptVm.describeParamUser(myCurCmd != 0 ? "exec" : "start", myCurCmd != 0 ? cmd.getType() : MessageCodes.THREAD_START_FINISH, myLog, cmd);
                myCurCmd++;
            }
            else
                break;
        }

        if (isFinished())
        {
            if (myData.myController != null && myData.myController.getResume())
            {
                AiScriptVm.describeParamUser("finish", MessageCodes.THREAD_START_FINISH, myLog, myData);
                rewind();
            }
            else
            {
                AiScriptVm.describeParamUser("release", MessageCodes.THREAD_CREATE_RELEASE, myLog, myData);
                return true;
            }
        }
        return false;
    }

    public bool isExecuted() { return myCurCmd > 0 || isFinished(); }
    public bool isDeleted() { return myCurCmd < 0; }
    public crc32 getID() { return myData.getID(); }
    public void shutdown() { myData.shutdown(); }
    public void deleteMe() { getTop().onOverride(); myCurCmd = -1; }
    public IVmCommand getTop() { return myData.myCommands[0]; }
    int enumCommands(IEnumer<IVmCommand> en) { return myData.enumCommands(en); }

    private bool isFinished() { return myCurCmd >= myData.myCommands.Count; }
    private IVmCommand getCurrent() { return myData.myCommands[myCurCmd]; }
    void rewind() { myCurCmd = 0; }

    int myCurCmd;
    ThreadData myData;
    IErrorLog myLog;
};

class VariablePool : IVmVariablePool
{
    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(GetType().ToString());
        foreach (KeyValuePair<DWORD, IParamList.Param> kvp in myDict)
        {
            //sb.AppendFormat("\t{0} {1}\n", kvp.Key.ToString("X8"), kvp.Value.GetValue());
            sb.AppendFormat("\t{0} {1}\n", Hasher.StringHsh(kvp.Key), kvp.Value.GetValue());
        }
        return sb.ToString();
    }
    Dictionary<crc32, IParamList.Param> myDict = new Dictionary<DWORD, IParamList.Param>();
    public void setValue(crc32 name, IParamList.Param param)
    {
        if (myDict.ContainsKey(name)) myDict.Remove(name);
        myDict.Add(name, param);
    }

    public bool getValue(crc32 name, out IParamList.Param param)
    {
        return myDict.TryGetValue(name, out param);
    }
    public bool getVariable(crc32 name, out IParamList.Param param)
    {
        return getValue(name, out param);
    }

    public void setVariable(crc32 name, IParamList.Param param)
    {
        setValue(name, param);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}

public class Vm : IVm
{
    //STUB!
    LocalFactory myFactory = new LocalFactory();
    IErrorLog myLog;
    List<StormThread> myThreads = new List<StormThread>();
    public void initialize(IErrorLog log)
    {
        myLog = log;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int enumCommands(ref IEnumer<IVmCommand> en)
    {
        throw new System.NotImplementedException();
    }

    public int getThreadCount()
    {
        throw new System.NotImplementedException();
    }

    public bool parseScript(string text, string namesp = "")
    {
        Debug.Log(string.Format("[{0}] Parsing script text: [{1}] Last Symbol code {2}", namesp, text, (byte)text[text.Length - 1]));
        if (!text.EndsWith('\0')) text += '\0';
        //return true;

        //ThreadData* td = createMainThread(namesp);
        //bool ret = aisp::parceAiScript(text, td, getFactory(), myLog);
        //if (ret)
        //{
        //    if (td->getCommandsCount() > 1)
        //        createThread(td);
        //}
        //td->Release();
        //return ret;
        ThreadData td = createMainThread(namesp);
        bool ret = AiScriptParser.parceAiScript(text, td, getFactory(), myLog);
        if (ret)
        {
            if (td.getCommandsCount() > 1)
                createThread(td);
        }
        td.Release();
        return ret;
    }

    IVmFactory getFactory() { return myFactory; }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public void reset()
    {
        throw new System.NotImplementedException();
    }

    public void run()
    {
        List<StormThread> tmpThreads = new List<StormThread>();
        //for (int i = 0; i < myThreads.Count;)
        for (int i = 0; i < myThreads.Count;i++)
        {
            StormThread thr = myThreads[i];
            //if (thr.isDeleted() || thr.run())
            //{
            //    //delete thr;
            //    //myThreads.Remove(i, i);
            //}
            //bool IsDeleted = thr.isDeleted();
            //bool IsRun = thr.run();
            //if (IsDeleted || IsRun)
            //bool IsDeleted = false;
            //bool IsRun = false;
            //if ((IsDeleted = thr.isDeleted()) || (IsRun = thr.run()))
            if (thr.isDeleted() || thr.run())
            {
                //Debug.Log(string.Format("IsDeleted {0} IsRun {1} Threads {2}",IsDeleted,IsRun, myThreads.Count));
                thr.Dispose();
            }
            else
            {
                tmpThreads.Add(thr);
            }
        }
        myThreads = tmpThreads;

    }

    public void setFactory(IVmFactory fct)
    {
        myFactory = new LocalFactory();
        //Debug.Log("Inheriting Local factory from " + fct);
        myFactory.initialize(this, fct);
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public IErrorLog getErrorLog()
    {
        return myLog;
    }

    public void createThread(ThreadData dt)
    {
        crc32 name = dt.getID();
        if (name != 0)
        {
            for (int i = 0; i < myThreads.Count; ++i)
            {
                StormThread thr = myThreads[i];
                if (thr.getID() == name)
                {
                    bool executed = thr.isExecuted();
                    if (executed)
                        thr.shutdown();
                    else
                        thr.deleteMe();
                    AiScriptVm.describeParamUser(executed ? "shutdown" : "override", MessageCodes.THREAD_CREATE_RELEASE, myLog, thr.getTop());
                    break;
                }
            }
        }
        myThreads.Add(new StormThread(dt, myLog));

    }

    ThreadData createMainThread(string namesp)
    {
        ThreadData td = new ThreadData();
        myFactory.setCurrentName(namesp);
        td.setController("vm_main", getFactory());
        return td;
    }
}

public class LocalFactory : IVmFactory
{
    public void setCurrentName(string name)
    {
        myCurrentName = name;
    }

    public void initialize(Vm vm, IVmFactory parent)
    {
        myVM = vm;
        myUserFactory = parent;
        myCurrentName = null;

        //if (parent == null) Debug.Log("Parent is null for " + myCurrentName + ". HELP!");
    }
    public virtual IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        IVmCommand ret = null;
        switch (name)
        {
            case 0x3A283C62:
                { // vm_create
                    ThreadCreateCommand cmd = new ThreadCreateCommand();
                    cmd.initialize(myVM);
                    ret = cmd;
                }
                break;
            case 0x7D6E9376:
                { // vm_main
                    MainCommand cmd = new MainCommand();
                    cmd.setCurrentName(myCurrentName);
                    ret = cmd;
                }
                break;
        }
        if (ret != null)
        {
            //Debug.Log(string.Format("Command created {0}", ret));
        }
        else
        {
            //Debug.Log(string.Format("Command {1} creation passed to {0}", myUserFactory, name.ToString("X8")));
        }
        return ret != null ? ret : myUserFactory.createCommand(name, pool, cont);
    }
    public virtual IVmController createController(crc32 name)
    {
        IVmController ret = null;
        if (name == 0x7D6E9376)
        { // vm_main
            MainController cmd = new MainController();
            cmd.initialize(myCurrentName);
            ret = cmd;
        }
        return ret != null ? ret : myUserFactory.createController(name);
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        throw new NotImplementedException();
    }

    private Vm myVM;
    IVmFactory myUserFactory;
    string myCurrentName;
}
public class ThreadCreateCommand : NullCommand
{
    public const uint ID = 0xC1A49D1A;

    public ThreadCreateCommand()
    {
        myVm = null;
        myTD = null;
    }

    public override bool run()
    {
        AiScriptVm.describeParamUser("create", MessageCodes.THREAD_CREATE_RELEASE, myVm.getErrorLog(), myTD);
        myVm.createThread(myTD);
        return true;
    }
    public virtual object Query(uint cl)
    {
        if (cl == ID) return this;
        return null;
    }

    public void setThreadData(ThreadData d)
    {
        myTD = d;
    }
    public void initialize(Vm t)
    {
        myVm = t;
    }
    public ThreadData myTD;
    Vm myVm;
};

public static class AiScriptDefs
{
    public const string sRunTime = "RunTime";
}
public abstract class NullController : IVmController, IParamList
{
    public virtual bool isParsingCorrect() { return true; }
    public virtual IParamList getParamList() { return this; }
    public virtual IParamList.PInfo getParamInfo(crc32 param_name) { return new IParamList.PInfo(); }
    public virtual bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name) { return false; }
    public virtual string getName() { return null; }
    public virtual void restart() { }
    public virtual int getType() { return 0; }
    public virtual string describeParams(ref string myparams) { return myparams; }

    public abstract bool getResume();
    public abstract uint getID();
    public abstract void shutdown();
    public abstract void setID(uint id);
    public void AddRef() { }
    public int RefCount() { return 0; }
    public int Release() { return 0; }
}
public class MainController : NullController
{
    public override bool getResume()
    {
        return false;
    }
    public override string getName()
    {
        return myName != null ? myName : AiScriptDefs.sRunTime;
    }
    public override crc32 getID()
    {
        return 0;
    }
    public override void setID(crc32 crc) { }
    public override void shutdown()
    {
    }
    public void initialize(string name)
    {
        myName = name;
    }
    public override IParamList getParamList() { return this; }

    private string myName;
};
public class MainCommand : NullCommand
{
    public override string getName()
    {
        return myName != null ? myName : AiScriptDefs.sRunTime;
    }
    public override bool run() { return true; }
    public override IParamList getParamList() { return this; }


    public void setCurrentName(string name)
    {
        myName = name;
    }
    string myName;

    public override string ToString()
    {
        return GetType() + " [" + myName + "]";
    }
}
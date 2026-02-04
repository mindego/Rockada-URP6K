public class LogListHolder
{

    //LogListHolder(O TheCreateFunc, string log_name, int id, int max_strs) :myCreateFunc(TheCreateFunc),myList(0)
    public LogListHolder(string log_name, int id, int max_strs) 
    {
        //myList = null;
        myLog = LogFactory.CreateLOG(log_name, ILog.LOG_VERSION);
        if (myLog != null)
        {
            //myLog.setIdentSize(id);
            //myList = new LogClientList(max_strs);
            //myLog.AddRenderer(myList);
        }
    }
    ~LogListHolder()
    {
        if (myLog != null)
        {
            //myLog.SubRenderer(myList);
            //myLog.Release();
        }
        //if (myList != null)
        //    myList.Release();
    }
    //ILog* operator ->   ()  {  return myLog; }

    public ILog getLog() { return myLog; }
    //LogClientList getList() { return myList; }

    //int getLineCount() { return myList.getLineCount(); }
    //string getLine(int i) { return myList.getLine(i); }
    //void Clear() { myList.clear(); }
    public void Clear() {  }

    //O myCreateFunc;
    private ILog myLog;
    //private  LogClientList myList;
};
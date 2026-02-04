public static class LogFactory
{
    // дефолтовый лог
    static LOG DefLog = null;
    public static ILog CreateLOG(string name, int version=ILog.LOG_VERSION)
    {
        return LOG.createLOG(name, version);
    }

    public static string GetLogName(out string dest)
    {
        //GetModuleFileName(0, dest, MAX_PATH);
        dest = "EchelonUnity.log";
        return dest;
    }
    public static LOG GetLog()
    {
        // если это первый вызов
        if (DefLog == null)
        {
            // создаем лог
            string ExeLogName;
            DefLog = (LOG)CreateLOG(GetLogName(out ExeLogName));
            DefLog.OpenLogFile();
            DefLog.setAsStdOut();
            // устанавливаем функцию автоматического удаление
            //atexit(DeleteDefLog);
        }
        // возвращаем DefLog
        return DefLog;
    }
}

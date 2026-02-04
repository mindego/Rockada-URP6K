public static class UniTools
{
    public static iUnivarTable CreateIntTable(iUnifiedVariableContainer root, string name)
    {
        if (root == null) return null;
        IntTable myobject = new IntTable();
        if (!myobject.Initialize(root, name))
        {
            myobject.Release();
            myobject = null;
        }
        return myobject;
    }

    public static iUnivarTable CreateUnivarTable(iUnifiedVariableContainer root, string name)
    {
        if (root == null) return null;
        UnivarTable myobject = new UnivarTable();
        if (!myobject.Initialize(root, name))
        {
            myobject.Release();
            myobject = null;
        }
        return myobject;
    }
}

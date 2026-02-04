using System;
using UnityEngine;

public static class UnivarDBHelper
{
    public static bool getStringArray(iUnifiedVariableContainer cont, string name, ref string[] values, int size)
    {
        //iUnifiedVariableArray arr = cont.GetVariableTpl<iUnifiedVariableArray>(name);
        UniVarArray arr = (UniVarArray) cont.GetVariableTpl<iUnifiedVariableArray>(name);

        bool ret = arr!=null;
        if (ret)
        {
            for (int i = 0; i < size; ++i)
            {
               // Debug.Log("Loading " + (i+1) + " of " + size + " arr " + arr);
                iUnifiedVariableString str = arr.openString((uint)i);
                //iUnifiedVariableString str = arr.openString("DELETEME"); //TODO временно, нужно исправить
                if (str != null)
                {
                    //char* buffer = ANewN(char, str->StrLen() + 1);
                    //str->StrCpy(buffer);
                    values[i] = str.GetValue();
                    //Debug.Log("Value: " + values[i]);
                }
                else Debug.Log(string.Format("Failed loading array element [{0}] from {1}",i,arr));
            }

        }
        return ret;
    }

    internal static void setVariable<T1, T2>(iUnifiedVariableContainer iUnifiedVariableContainer, T2 v, T2 myTitle)
    {
        throw new NotImplementedException();
    }

    internal static void setStringArray(iUnifiedVariableContainer iUnifiedVariableContainer, string v, string[] myVoices, int gMaxRadioVoices)
    {
        throw new NotImplementedException();
    }

    public static iUnifiedVariableContainer getMissionContainer(UnivarDB db, string name)
    {
        iUnifiedVariableContainer gsd = db.GetRoot().openContainer("Events");
        return gsd.openContainer(name);
    }
}


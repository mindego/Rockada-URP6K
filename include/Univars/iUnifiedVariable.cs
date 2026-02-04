using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using static IParamList;
using DWORD = System.UInt32;

/// <summary>
/// базовый класс переменных
/// </summary>
public interface iUnifiedVariable : IObject
{
    //IID(0x0EEE232E);
    new public const DWORD ID = 0x0EEE232E;
    public DWORD GetClassId();
    public bool Delete();
    public bool ExportToFile(string filename);
    public bool ImportFromFile(string filename);
    public int GetNameLength();
    public string GetName(ref string value);
    //public string GetName();

    public string getNameShort()
    {
        //string buffer="";
        //string res=GetName(ref buffer);
        //Debug.Log((res, buffer));
        //return res;

        string buffer = "";
        return GetName(ref buffer); ;
        //return GetName(string.Empty);
    }

    public string getNameLong()
    {
        string buffer = "";
        return GetName(ref buffer);
        //return GetName(string.Empty);
    }

    public DWORD GetID()
    {
        return ID;
    }
    public static DWORD GetID<T>()
    {
        DWORD res = 0;
        System.Type vartype = typeof(T);
        if (vartype == typeof(iUnifiedVariableContainer)) res = iUnifiedVariableContainer.ID;
        if (vartype == typeof(iUnifiedVariableArray)) res = iUnifiedVariableArray.ID;
        if (vartype == typeof(iUnifiedVariableString)) res = iUnifiedVariableString.ID;
        if (vartype == typeof(iUnifiedVariableInt)) res = iUnifiedVariableInt.ID;
        if (vartype == typeof(iUnifiedVariableVector)) res =    iUnifiedVariableVector.ID;
        if (vartype == typeof(iUnifiedVariableFloat)) res = iUnifiedVariableFloat.ID;
        if (vartype == typeof(iUnifiedVariableBlock)) res = iUnifiedVariableBlock.ID;

        if (res == 0) throw new System.Exception("Incorrect type: " + vartype.Name);
        return res;
    }
};


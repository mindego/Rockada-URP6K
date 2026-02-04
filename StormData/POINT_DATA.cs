using System;
using UnityEngine;
/// <summary>
/// POINT_DATA - представление точек маршрута группы для ИИ
/// </summary>
public class POINT_DATA  
{
    public Vector3 Org;                // координаты точки
    public float TimeToPoint;        // время до точки
    public string AiScript;           // пользовательская строчная переменная

    // создание/удаление
    //    POINT_DATA() :AiScript(0) { }
    public POINT_DATA()
    {
        AiScript = null;
    }
    public POINT_DATA(Vector3 org, float time, string scr)
    {
        Org = org;
        TimeToPoint = time;
        AiScript = string.Empty;
        if (scr != string.Empty)
            AiScript = scr;
    }

    //void Clear() { if (AiScript != 0) delete AiScript; }
    public void Clear()
    {
        if (AiScript != null) AiScript = null;
    }
    public void SetScript(string scr)
    {
        this.Clear();
        if (scr != string.Empty)
            AiScript = scr;
    }

    //~POINT_DATA() {
    //    Clear();
    //}
    public void Init(iUnifiedVariableContainer pCtr)
    {
        UniVarVector pVector;
        UniVarFloat pFloat;
        UniVarString pString;
        // Org
        pVector = (UniVarVector) pCtr.GetVariableTpl<iUnifiedVariableVector>("Org");
        //Debug.Log(pVector);
        Org = pVector.GetValue();
        // TimeToPoint
        //pFloat = pCtr->GetVariableTpl<iUnifiedVariableFloat>("TimeToPoint");
        pFloat = pCtr.GetVariableTpl<UniVarFloat>("TimeToPoint");
        TimeToPoint = pFloat != null ? pFloat.GetValue() : 0;
        // AiScript
        //pString = pCtr->GetVariableTpl<iUnifiedVariableString>("AiScript");
        pString = pCtr.GetVariableTpl<UniVarString>("AiScript");
        //DWORD len = pString->StrLen() + 1;
        //AiScript = new char[len];
        pString.StrCpy(out AiScript);
        //Debug.Log("AiScript strcpy [" + AiScript + "] last " + ((byte)AiScript[AiScript.Length - 1]));
        if (AiScript != String.Empty && AiScript != null) AiScript += "\0"; //Символ '\0' используется в разборке скрипта

    }
};

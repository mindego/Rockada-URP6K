using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;

/// <summary>
/// API
/// </summary>
public interface CommandsApi : IRefMem
{
    const uint UndefinedID = 0xFFFFFFFF;
    // регистрация/разрегистрация
    public crc32 RegisterCommand(string nm, CommLink cs, int nArgs, string help = "");
    public crc32 RegisterVariable(string nm, CommLink cs, VType vt, string help = "");
    public crc32 RegisterTrigger(string nm, CommLink cs, string help = "");

    public void UnRegister(CommLink cl, crc32 name = UndefinedID);
    // разобрать строку
    public void ProcessString(string s, bool Inverted = false);
    public bool ExecFile(string s);
    public string SuggestSpelling(string source, string limit);

};

/// <summary>
/// variable callback defintion
/// </summary>
public interface CommLink
{
    public virtual object OnVariable(uint id, object value) { return 0; }
    public virtual void OnCommand(uint id, string str1, string str2) { }
    public virtual void OnTrigger(uint id , bool b) { }
}


/// <summary>
/// defines
/// </summary>
public enum VType : uint
{
    VAR_INT = 0x4A98E7B3,
    VAR_FLOAT = 0x4CC8385A,
    VAR_SVEC4 = 0xA97F115F, // 4shorts
    VAR_FVEC2 = 0xE81CAC98, // 2 floats
    VAR_VECTOR = 0xF95C3559,
    VAR_TEXT = 0x3CB7C093
};

/// <summary>
/// API
/// </summary>
//public interface CommandsApi : IRefMem
//{
//    const uint UndefinedID = 0xFFFFFFFF;
//    // регистрация/разрегистрация
//    public crc32 RegisterCommand(string nm, CommLink cs, int nArgs, string help = "");
//    public crc32 RegisterVariable(string nm, CommLink cs, VType vt, string help = "");
//    public crc32 RegisterTrigger(string nm, CommLink cs, string help = "");

//    public void UnRegister(CommLink cl, crc32 name = UndefinedID);
//    // разобрать строку
//    public void ProcessString(string s, bool Inverted = false);
//    public bool ExecFile(string s);
//    public string SuggestSpelling(string source, string limit);

//};

using DWORD = System.UInt32;
using UnityEngine;
using System;

public static class CppFunctionEmulator
{
    /// <summary>
    /// Эмуляция GetTickCount
    /// </summary>
    /// <returns>время в миллисекундах с момента запуска игры</returns>
    public static DWORD GetTickCount()
    {
        return (DWORD)(Time.realtimeSinceStartup * 1000);
    }

    /// <summary>
    /// Эмуляция макроса Макрос SUCCEEDED (winerror.h)
    /// </summary>
    /// <param name="hr"></param>
    /// <returns></returns>
    public static bool SUCCEEDED(HRESULT hr)
    {
        return (hr >= 0);
    }

    public static bool FAILED(HRESULT hr)
    {
        return hr < 0;
    }
}

/// <summary>
/// Эмуляция dwFlags структуры DD_LOCKDATA (ddrawint.h)
/// </summary>
public enum dwFlags
{
    DDLOCK_DISCARDCONTENTS,
    DDLOCK_DONOTWAIT,
    DDLOCK_EVENT,
    DDLOCK_HASVOLUMETEXTUREBOXRECT,
    DDLOCK_NODIRTYUPDATE,
    DDLOCK_NOOVERWRITE,
    DDLOCK_NOSYSLOCK,
    DDLOCK_OKTOSWAP,
    DDLOCK_READONLY,
    DDLOCK_SURFACEMEMORYPTR,
    DDLOCK_WAIT,
    DDLOCK_WRITEONLY,
}

public interface DLLEmulation
{
    public const DWORD DLL_PROCESS_ATTACH = 0;
    public const DWORD DLL_PROCESS_DETACH = 1;
    public static int DllMain(HMODULE hDll, DWORD dwReason, LPVOID aborted) { throw new System.NotImplementedException(); }
}

public class HMODULE { }
public class LPVOID { }

/// < summary >
/// Эмуляция HRESULT
/// </ summary >
public enum HRESULT : uint
{
    /// <summary>
    /// Операция выполнена успешно
    /// </summary>
    S_OK = 0x00000000,
    /// <summary>
    /// Операция прервана
    /// </summary>
    E_ABORT = 0x80004004,
    /// <summary>
    /// Общая ошибка отказа в доступе
    /// </summary>
    E_ACCESSDENIED = 0x80070005,
    /// <summary>
    /// Неопределенный сбой
    /// </summary>
    E_FAIL = 0x80004005,
    /// <summary>
    /// Недопустимый дескриптор
    /// </summary>
    E_HANDLE = 0x80070006,
    /// <summary>
    /// Один или несколько аргументов являются недопустимыми
    /// </summary>
    E_INVALIDARG = 0x80070057,
    /// <summary>
    /// Такой интерфейс не поддерживается
    /// </summary>
    E_NOINTERFACE = 0x80004002,
    /// <summary>
    /// Не реализовано
    /// </summary>
    E_NOTIMPL = 0x80004001,
    /// <summary>
    /// Не удалось выделить необходимую память
    /// </summary>
    E_OUTOFMEMORY = 0x8007000E,
    /// <summary>
    /// Недопустимый указатель
    /// </summary>
    E_POINTER = 0x80004003,
    /// <summary>
    /// Непредвиденный сбой
    /// </summary>
    E_UNEXPECTED = 0x8000FFFF,

    //D3D
        D3D_OK = 0x0
}
/// <summary>
/// Эмуляция пространства имён Пространство имён std
/// </summary>
public class std
{
    public static void swap<T>(ref T val1, ref T val2)
    {
        T tmp_val = val1;
        val1 = val2;
        val2 = tmp_val;
    }

    public static bool less<T>(T a, T b) where T : IComparable<T>
    {
        if (a.CompareTo(b) == -1) return true;
        return false;
    }
    public static bool greater<T>(T a, T b) where T : IComparable<T>
    {
        if (a.CompareTo(b) == 1) return true;
        return false;
    }

}
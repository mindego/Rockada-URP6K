using System.IO;
using UnityEngine;
/// <summary>
/// Класс-обёртка над MemoryStream
/// </summary>
public class MemBlock
{
    public Stream myStream
    {
        get
        {
            if (buffer == null || buffer.Length == 0)
            {
                Debug.Log("Stream is null!");
                return null; //TODO! Корректно отрабатывать неверные потоки при импорте
            }
            return new MemoryStream(buffer);
        }
    }
    public byte[] buffer;

    public int Size()
    {
        if (buffer == null) return 0;
        return buffer.Length;
    }

    public MemBlock(byte[] buffer, int size)
    {
        if (buffer == null) Debug.LogError("Empty buffer created");
        if (buffer.Length == 0) Debug.LogError("Empty stream created");
        this.buffer = buffer;
        //myStream = new MemoryStream(buffer);
    }

    public MemBlock(Stream st)
    {
        if (st == null) Debug.LogError("Empty buffer created");
        st.Read(buffer);
    }
    public MemBlock() { }

    ~MemBlock()
    {
        return;
        if (buffer != null) buffer = null;
        if (myStream != null)
        {
            Debug.Log("Closing stream " + myStream.ToString() + " " + myStream.GetHashCode().ToString("X8"));
            myStream.Close();
            //myStream = null;
        }
    }

    public T Convert<T>(int num,int size) where T : class, IStormImportable<T>, new()
    {
        if (buffer == null || buffer.Length == 0)
        {
            Debug.Log("Buffer is null!");
            return null; //TODO! Корректно отрабатывать неверные потоки при импорте
        }
        T resource = new T();
        
        using (Stream st = myStream)
        {
            st.Seek(num * size, SeekOrigin.Begin);
            resource = resource.Import(myStream);
            st.Close();
        }
        return (T)resource;
    }
    public T Convert<T>() where T : class, IStormImportable<T>, new()
    {
        if (buffer == null || buffer.Length == 0)
        {
            Debug.Log("Buffer is null!");
            return null; //TODO! Корректно отрабатывать неверные потоки при импорте
        }

        T resourcePrototype = new T();
        T resource;
        using (Stream st = myStream)
        {
            resource = resourcePrototype.Import(myStream);
            st.Close();
        }
        //myStream.Close();
        return (T)resource;
    }
}


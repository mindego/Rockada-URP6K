using System.IO;
using UnityEngine;

public static class wintools
{
    /// <summary>GetFnName("File.dat", S) return ".dat"
    /// and if (S!=0) S will contain "File"
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static string GetFnName(string name, ref string dest)
    {
        //cstr pnt = 0;
        //for (cstr nm = name; *nm; ++nm)
        //    switch (*nm)
        //    {
        //        case '.': pnt = nm; break;
        //        case '\\': pnt = 0;
        //    }

        //if (!pnt) pnt = nm;

        //if (dest)
        //    __nstrcpy(dest, name, pnt - name);

        //return pnt;

        //string pnt = null;
        //for (string nm = name; nm != null; nm = nm.Remove(0, 1))
        //{
        //    switch (nm[0])
        //    {
        //        case '.': pnt = nm; break;
        //        case '\\': pnt = null; break;
        //    }
        //}

        //if (dest != null)
        //{
        //    dest = pnt;//TODO Вот что-то не так с выделением имени...
        //}
        //return pnt;
        
        string fname= Path.GetFileNameWithoutExtension(name);
        string dir = Path.GetDirectoryName(name);
        dest= Path.Join(dir, fname);
        return dest;
        //return Path.GetFileNameWithoutExtension(name);

    }

    /// <summary>
    /// GetFnDir("C:\\My Documents\\File.dat", S) return "File.dat" 
    /// and if (S!=0) S will contain "C:\\My Docs\\"
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static string GetFnDir(string name, ref string dest)
    {
        //cstr slh = __strrchr(name, '\\');
        //if (*slh == '\\')
        //{
        //    if (dest)
        //        for (; name <= slh;) *dest++ = *name++;
        //    else
        //        name = slh + 1;
        //}

        //if (dest!=null) dest = null;
        //return name;
        if (dest != null) dest = Path.GetDirectoryName(name);
        return Path.GetFileName(name);
    }


    public static string GetFnDir(string name)
    {
        //cstr slh = __strrchr(name, '\\');
        //if (*slh == '\\')
        //{
        //    if (dest)
        //        for (; name <= slh;) *dest++ = *name++;
        //    else
        //        name = slh + 1;
        //}

        //if (dest!=null) dest = null;
        //return name;
        return Path.GetDirectoryName(name);
    }
}


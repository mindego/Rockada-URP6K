using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Globalization;
using DWORD = System.UInt32;

public class READ_TEXT_FILE
{
    //  char filename[MAX_FILE_NAME];
    //  int string;
    //  int comment;
    //  int crc;
    //  bool count_crc;
    //  char Buffer[1024];
    //  char* Ptr;
    //  char* DataPtr;
    //  const char* DataPos;

    public string filename;
    public int stringField;
    public int comment;
    public int crc;
    public bool count_crc;
    public char[] Buffer = new char[1024];
    uint DataPos;

    public READ_TEXT_FILE(string name, int Comment, bool CountCrc, uint data)
    {
        filename = name;
        count_crc = CountCrc;
        comment = Comment;
        stringField = 0;
        DataPos = data;
        Buffer[0] = default;
        crc = -1;
    }

    public bool Ready(bool MustExist)
    {
        //if (DataPos != null) return true;
        return true;
    }

    public string ReadLine(bool CanBeEOF)
    {
        return default;
    }
}

public class READ_TEXT_STREAM
{
    private Stream stream;
    public int stringCount;
    public char comment;
    public int crc;
    public bool count_crc;
    public char[] Buffer = new char[1024];
    uint DataPos;

    private string currentToken;

    public char[] Dels = { ' ', '=', '\t', '(', ',', ')', '\r', '\n' };

    private StreamReader reader;

    public string CurrentToken { get => currentToken; }

    /// <summary>
    /// класс для чтения текстовых потоков вида
    /// перемнная = значение ; комментраий
    /// </summary>
    /// <param name="stream">Поток</param>
    /// <param name="Comment">Символ комментария</param>
    /// <param name="CountCrc">Подсчитывать CRC</param>
    /// <param name="data">Первоначальное смещение для чтения данных</param>
    public READ_TEXT_STREAM(Stream stream, char Comment, bool CountCrc, uint data)
    {
        this.stream = stream;
        comment = Comment;
        count_crc = CountCrc;
        DataPos = data;
        stringCount = 0;

        reader = new StreamReader(stream, Encoding.GetEncoding(1251));
        Buffer = reader.ReadLine().ToCharArray();
    }

    public int LineNumber()
    {
        return stringCount;
    }
    public bool Ready(bool MustExist)
    {
        //I'm always ready!
        return true;
    }
    public string ReadLine(bool CanBeEOF = true)
    {
        if (DataPos < Buffer.Length)
        {
            // StormLog.LogMessage(DataPos+ "<"+ Buffer.Length);
            //Debug.Log(new string(Buffer));
            return new string(Buffer);
        }
        //Debug.Log((DataPos, ">=", Buffer.Length));
        string rl;
        try
        {
            rl = reader.ReadLine();
            if (rl == null) return null;
            rl = rl.Trim();

        }
        catch
        {
            Debug.Log(reader);
            throw;
        }
        //Debug.Log(rl);
        rl = rl.Split(comment)[0].Trim();
        if (rl == null) return null;

        Buffer = rl.ToCharArray();

        stringCount++;

        //if (comment!=null) { char* c = StrChr(Buffer, comment); if (c) *c = 0; }
        //if (count_crc) crc ^= HashString(Buffer);
        DataPos = 0;
        //return new string(Buffer).Split(';')[0].Trim();
        return rl;
    }

    public string DebugStream()
    {
        string bufferString = new string(Buffer);
        string res = "";
        res += "Current token:" + currentToken + "\n";
        res += "Data pos: " + DataPos + "\n";
        res += "Buffer string: [" + bufferString + "]\n";
        bufferString.Insert((int)DataPos, "*");
        res += "Data position string: [" + bufferString + "]\n";

        return res;
    }
    public string GetNextItem()
    {
        return GetNextItem(false, true);
    }
    public string GetNextItem(bool ForceRead, bool CanBeEOF)
    {
        int failsafe = 5000;
        //while (true)
        //{
        string myString = String.Empty;
        while (myString == String.Empty)
        {
            if (failsafe-- == 0) throw new Exception("Too many read attempts");
            myString = ReadLine(CanBeEOF);
        }
        //Debug.Log("String value is: " + myString);
        if (myString == null) return null;
        myString = StrTokQ(Buffer, Dels);
        StormLog.LogMessage("Got token " + myString);
        currentToken = myString;
        if (myString != null) return myString;

        if (failsafe-- <= 0)
        {
            Debug.Log("File read Failsafe triggered");
            return null;
        }
        return null;
        //}
    }

    private string _parse_string(char[] p, char[] src, bool use_quotes)
    {
        if (p == null)
        {
            Debug.Log("Null string in input");
            return null;
        }
        StringBuilder sb = new StringBuilder();
        //Debug.Log($"{DataPos}, {p.Length}, [{new string(p)}]");

        bool ServiceSymbolFound;
        for (uint i = DataPos; i < p.Length; i++)
        {
            if (p[i] == '\0') return null;
            ServiceSymbolFound = false;
            foreach (char lChar in src)
            {
                if (p[i] == lChar) ServiceSymbolFound = true;
            }

            if (ServiceSymbolFound)
            {
                DataPos = i + 1;
                if (sb.Length > 0) return sb.ToString();
                continue;
            }

            if (use_quotes) //это кавычка?
            {
                //ищем завершающую кавычку
                if (p[i] == '\"' || p[i] == '\'')
                {
                    DataPos += 1;
                    char quote = p[i];
                    for (int j = (int)i + 1; j < p.Length; j++)
                    {
                        DataPos += 1;
                        if (p[j] == quote) return sb.ToString();
                        sb.Append(p[j]);
                    }
                }
            }

            DataPos = i;
            sb.Append(p[i]);
        }
        DataPos = (uint)p.Length;
        return sb.ToString();
        //while (true)
        //{
        //    if (failsafe-- <= 0)
        //    {
        //        Debug.Log("File read Failsafe triggered");
        //        return null;
        //    }
        //    for (uint i=DataPos;i<p.Length;i++)
        //    {
        //        foreach (char lChar in src)
        //        {
        //            if (p[i] == lChar)
        //            {
        //                DataPos += 2;
        //                return sb.ToString();
        //            }

        //        }
        //        DataPos = i;
        //        sb.Append(p[i]);
        //    }
        //}
    }



    /*
     static char* _parse_string(char*& p,const char* src,bool use_quotes) {
  if (p==0) return 0;
  char* r=0;
  while (*p) {
    for (const char* c=src; *c; c++) if (*c==*p) break;
    if (*c) { *p=0; p++; if (r) return r; continue; } // нашли служебный символ
    // нашли не служебный символ
    if (use_quotes) {
      // это кавычка?
      if (*p=='\"' || *p=='\'') {
        char quote=*p;
        // ищем завершающую кавычку
        char* q=p+1;
        while (*q) {
          if (*q!=quote) { q++; continue; }
          // нашли. записываем туда 0
          *q=0;
          // устанавливаем начало найденой строки и указатель на следующий кусок
          r=p+1; p=q+1;
          return r;
        }
      }
      // обрабатываем обычным способом
      use_quotes=false;
    }
    // назначаем начало строки
    if (!r) r=p;
    p++;
  }
  return r;
}

     */

    private string StrTokQ(char[] str, char[] src)
    {
        //if (ptr==0) ptr=&_ptr;
        //char*& p=*ptr;
        //if (str) p = str;
        return _parse_string(str, src, true);
    }

    public bool Recognize(string name)
    {
        bool res = (currentToken == name);
        //if (res) StormLog.LogMessage("Checking key [" + name + "] against [" + currentToken + "] " + (currentToken == name ? "match" : "mismatch"),StormLog.logPriority.WARNING);
        return res;
        //return false;
    }
    public bool LoadFloat(ref float value, string name)
    {
        if (!Recognize(name)) return false;

        string c = GetNextItem();
        //value = Convert.ToSingle(c);
        //if (!float.TryParse(c, out value)) StormLog.LogMessage($"Can't parse {name}:{c}");
        value = float.Parse(c, CultureInfo.InvariantCulture.NumberFormat);
        return true;

        //Recognize(String) { Value=atof(f.GetNextItem()); continue; }
    }

    public bool Load_String(ref string value, string name)
    {
        if (!Recognize(name)) return false;

        string c = GetNextItem();
        //value = Convert.ToSingle(c);
        //if (!float.TryParse(c, out value)) StormLog.LogMessage($"Can't parse {name}:{c}");
        value = c;
        return true;

        //Recognize(String) { Value=atof(f.GetNextItem()); continue; }
    }
    /// <summary>
    /// Преобразование строки в число с плавающим знаком
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public float AtoF(string value)
    {
        return float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
    }

    public bool LoadFloatC(ref float value, string name, Func<float, float> method)
    {
        bool res = LoadFloat(ref value, name);
        if (!res) return false;

        value = method(value);
        return true;

    }
    public bool LoadFloatC(ref float value, string name)
    {
        bool res = LoadFloat(ref value, name);

        //TODO! do smth with value;
        // value = FUNC(value);
        return res;
    }

    public bool LoadBool(ref bool value, string name)
    {
        int intval = 0;
        if (!LoadInt(ref intval, name)) return false;
        value = (intval != 0);
        return true;
    }
    public bool LoadInt(ref int value, string name)
    {
        if (!Recognize(name)) return false;

        string c = GetNextItem();
        if (c == null) return false;
        //value = Convert.ToInt32(value);
        value = int.Parse(c, CultureInfo.InvariantCulture.NumberFormat);
        return true;
    }

    public bool LoadUInt(ref uint value, string name)
    {
        if (!Recognize(name)) return false;

        string c = GetNextItem();
        if (c == null) return false;
        //value = Convert.ToInt32(value);
        value = uint.Parse(c, CultureInfo.InvariantCulture.NumberFormat);
        return true;
    }

    public bool LdHST(ref string v, string name)
    {
        if (!Recognize(name))
        {
            return false;
        }
        v = GetNextItem();
        return true;
    }


    public bool LdHST<T>(ref T v, string name) where T : iSTORM_DATA<T>, new()
    {
        //#define LdHST(v,n,t) Recognize(n) { v=(t)HshString(f.GetNextItem()); continue; }
        //        Expands to:

        //if (StriCmp(c, "Debris") == 0) { Debris = (DEBRIS_DATA*)HshString(f.GetNextItem()); continue; }
        if (!Recognize(name)) { return false; }


        //Debug.Log("Value recognized [" + name +"] , loading " + typeof(T));
        if (v == null)
        {
            v = new T();
            //Debug.Log(v);
        }
        string value = GetNextItem();
        v = v.GetByCodeLocal(Hasher.HshString(value));
        //Debug.Log("Detected " + value + ", loaded as " + v);
        //uint hashName = 0xFFFFFFFF ;
        //if (!LdHS(ref hashName, name)) return false;

        //STUB! 
        //TODO: Fix it!
        return true;
    }
    public bool LdHS(ref uint v, string name)
    {
        if (!Recognize(name))
        {
            return false;
        }
        v = Hasher.HshString(GetNextItem());
        return true;
    }

    public bool LoadColorF4(ref Color value, string name)
    {
        if (!Recognize(name)) return false;

        //float _cr, _cg, _cb;
        value.r = AtoF(GetNextItem());
        value.g = AtoF(GetNextItem());
        value.b = AtoF(GetNextItem());
        value.a = 0;
        value /= 255;
        return true;
    }

    public Vector3 recognizeVector3f()
    {
        float R = AtoF(GetNextItem());
        float G = AtoF(GetNextItem());
        float B = AtoF(GetNextItem());
        return new Vector3(R, G, B);
    }

    /// <summary>
    /// Загрузить и добавить строку к переменной v
    /// </summary>
    /// <param name="v">Значение</param>
    /// <param name="name">Имя параметра</param>
    /// <returns>Успешна или неуспешна загрузка</returns>
    public bool LdAS(ref string v, string name)
    {
        //v = string.Empty;
        if (!Recognize(name)) return false;

        v = GetNextItem();
        return true;
    }

    /// <summary>
    /// Загрузить кодированную строку
    /// </summary>
    /// <param name="v">Значение</param>
    /// <param name="name">Имя параметра</param>
    /// <returns>Успешна или неуспешна загрузка</returns>
    public bool LdCS(ref DWORD v, string name)
    {
        //#define LdCS(v,n)    Recognize(n) { v=CodeString(f.GetNextItem()); continue; }
        if (!Recognize(name)) return false;
        string RawString = GetNextItem();
        v = Hasher.CodeString(RawString);
        return true;
    }

    public bool LoadVector(ref Vector3 value, string name)
    {
        if (!Recognize(name)) return false;
        value.x = AtoF(GetNextItem());
        value.y = AtoF(GetNextItem());
        value.z = AtoF(GetNextItem());
        return true;
    }

    public bool LoadFloatPair(ref float Value1, ref float Value2, string name)
    {
        if (!Recognize(name)) return false;
        Value1 = AtoF(GetNextItem());
        Value2 = AtoF(GetNextItem());
        return true;

    }
    public bool LoadIndexFromTable(ref int Value, string myString, string[] Table)
    {
        if (!Recognize(myString)) return false;
        int i; string c = GetNextItem();
        for (i = 0; i < Table.Length; i++)
        {
            if (c != Table[i]) continue;
            Value = i;
            return true;

        }
        return false;
    }

    public Vector3 recognizeVector3f(READ_TEXT_STREAM st)
    {
        float R = AtoF(st.GetNextItem());
        float G = AtoF(st.GetNextItem());
        float B = AtoF(st.GetNextItem());
        return new Vector3(R, G, B);
    }

    public Color recognizeColor(READ_TEXT_STREAM st)
    {
        float R = AtoF(st.GetNextItem());
        float G = AtoF(st.GetNextItem());
        float B = AtoF(st.GetNextItem());
        return new Color(R / 255, G / 255, B / 255);
    }

}

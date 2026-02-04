using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static Storm.Math;

/// <summary>
/// Реализация шрифтов "Шторма". В оригинале - просто Font, имя класса StormFontImpl выбрано для избежания конфликтов с Unity. Возможно, правильнее оставить имя Font, но перенести в отдельный namespace
/// </summary>
public class StormFontImpl : IFont
{
    public const char ESCAPE_CHAR = '^';
    public const int MAX_CHARS = 256;
    public const int MAX_COLORS = 16;

    IBill bill;
    ITexturesDB mTextures;
    IBObject myIBobject;
    public FontData data;
    //IObject texture;
    Texture2D texture;
    float w_scale;
    float h_scale;
    float h_size;
    float v_size;
    float spacing;
    Color[] custom_colors = new Color[16];
    void SetDefaultCustomColors()
    {
        custom_colors[0] = new Color32(255, 0, 0, 0);
        custom_colors[1] = new Color32(255, 255, 0, 0);
        custom_colors[2] = new Color32(255, 0, 255, 0);
        custom_colors[3] = new Color32(255, 255, 255, 0);
        custom_colors[4] = new Color32(255, 0, 0, 255);
        custom_colors[5] = new Color32(255, 0, 255, 255);
        custom_colors[6] = new Color32(255, 255, 0, 255);
        custom_colors[7] = new Color32(255, 255, 255, 255);
        custom_colors[8] = new Color32(255, 128, 0, 0);
        custom_colors[9] = new Color32(255, 0, 128, 0);
        custom_colors[10] = new Color32(255, 128, 128, 0);
        custom_colors[11] = new Color32(255, 128, 255, 128);
        custom_colors[12] = new Color32(255, 0, 0, 128);
        custom_colors[13] = new Color32(255, 0, 128, 128);
        custom_colors[14] = new Color32(255, 128, 0, 128);
        custom_colors[15] = new Color32(255, 128, 128, 128);
    }

    public StormFontImpl()
    {
        texture = null;
    }
    public bool Initialize(IBill _bill, FontData fdata)
    {
        bill = _bill;
        data = fdata;
        mTextures = bill.CreateTexturesDB("Graphics\\textures.dat");

        myIBobject = bill.Create(MAX_CHARS * 4, MAX_CHARS * 2);
        //Debug.Log("Loading font bitmap: [" + data.bitmap + "]");
        texture = mTextures.CreateTexture(Hasher.HshString(data.bitmap));

        SetDefaultCustomColors();

        return true;
    }

    public string DumpFontInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Font: {0} {1}x{2} spacing {3}\n", data.bitmap, h_size, v_size,spacing);
        for (int i=0;i<data.table.Length;i++)
        {
            char usedchar = (char)(i + ' ');
            sb.AppendFormat("char [{0}] raw [{1}] v4 {2}\n", usedchar.ToString(), data.table[i], data.table[i].Get());
        }
        return sb.ToString();
    }
    ~StormFontImpl() { }
    public void SetWH(float width, float height, float spc)
    {
        const float font_scale = 1 / 640.0f;
        h_size = width * 256.0f * font_scale;
        v_size = height * 256.0f * font_scale;

        spacing = spc * width * font_scale;
    }
    public float Width(string Text, int Len = 0x7FFFFFFF)
    {
        if (Text == null) Text = "";//Странное. Но пусть будет так.
        CharCoord[] Char = data.table;
        float x = 0;
        char c;
        bool SpecChar = false;
        Encoding cp1251 = Encoding.GetEncoding("windows-1251");
        byte[] bytes = cp1251.GetBytes(Text);


        //for (int i = 0; i < Text.Length; i++)
        for (int i = 0; i < bytes.Length; i++)
        {
            c = (char)bytes[i];
            if (SpecChar)
            {
                c = char.ToUpper(c);
                SpecChar = false;
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
                {
                    continue;
                }
            }
            if (c == ESCAPE_CHAR)
            {
                SpecChar = false;
                continue;
            }
            if (c >= ' ')
            {
                try
                {
                    x += h_size * (Char[c - ' '].Get()[2] - Char[c - ' '].Get()[0]) + spacing;
                } catch
                {
                    Debug.LogError(string.Format("Index out of bound [{0}] [{1}] {2} {3} in text {4}",c,(int)c,(int)(c-' '),Char.Length,Text));
                }
            }
        }
        return x - spacing;
    }
    public float Width(char Ch, bool WithSpasing = true)
    {
        CharCoord[] Char = data.table;
        float spacingScale = WithSpasing ? 1 : 0;
        return Ch >= ' ' ? h_size * (Char[Ch - ' '].Get()[2] - Char[Ch - ' '].Get()[0]) + spacing * spacingScale : 0;
    }
    public float Height()
    {
        CharCoord[] Char = data.table;
        return v_size * (Char[0].Get()[3] - Char[0].Get()[1]);
    }
    public void Puts(Color myColor, Vector2 p, string Text)
    {
        if (Text == null) Text = "";
        string text = Text;
        
        CharCoord[] ch = data.table;
        char c;
        Color current_color = myColor;

        float x = p.x, y = p.y;

        bill.SetTexture(texture);

        // 0 1 2 3
        // x y z w

        //float y0 = y, y1 = y + v_size * (ch[0].Get().w - ch[0].Get().y);
        float y0 = y, y1 = y + v_size * (ch[0].Get()[3] - ch[0].Get()[1]);
        int i_v = 0, i_f = 0;

        myIBobject.name = Text;
        myIBobject.Lock();

        Encoding cp1251 = Encoding.GetEncoding("windows-1251");
        byte[] bytes = cp1251.GetBytes(Text);
        //Encoding utf8 = Encoding.UTF8;
        //for (int i=0;i<Text.Length;i++)
        //{
        //    c = Text[i];
        for (int i = 0; i < bytes.Length; i++)
        {
            c = (char)bytes[i];
            if (c==ESCAPE_CHAR)
            {// special character resolved
                i++;
                if (i >= Text.Length) break;
                c = Text[++i];
                if (c == '\0') break;
                c = char.ToUpper(c);
                if (c != ESCAPE_CHAR)
                {
                    if (c >= '0' && c <= '9') current_color = custom_colors[c - '0'];
                    else if (c >= 'A' && c <= 'F') current_color = custom_colors[c - 'A' + 10];
                    else current_color = myColor;
                    continue;
                }
            }

            //myIBobject.name = c.ToString();
            //myIBobject.name = Text[i].ToString();
            if ((c -= ' ')>=0) 
            {
                myIBobject.SetFace(i_f++, i_v, i_v + 1, i_v + 2);
                myIBobject.SetFace(i_f++, i_v, i_v + 2, i_v + 3);

                float x0 = x;
                x += h_size * (ch[c].Get()[2] - ch[c].Get()[0]);

                myIBobject.SetVertO(i_v + 0, new Vector2(x0, y0), new Vector2(ch[c].Get()[0], ch[c].Get()[1]));
                myIBobject.SetVertO(i_v + 1, new Vector2(x0, y1), new Vector2(ch[c].Get()[0], ch[c].Get()[3]));
                myIBobject.SetVertO(i_v + 2, new Vector2(x, y1), new Vector2(ch[c].Get()[2], ch[c].Get()[3]));
                myIBobject.SetVertO(i_v + 3, new Vector2(x, y0), new Vector2(ch[c].Get()[2], ch[c].Get()[1]));
                
                myIBobject.SetVertC(i_v + 0, current_color, Color.black);
                myIBobject.SetVertC(i_v + 1, current_color, Color.black);
                myIBobject.SetVertC(i_v + 2, current_color, Color.black);
                myIBobject.SetVertC(i_v + 3, current_color, Color.black);
                i_v += 4;
                x += spacing;
            }
            //myIBobject.UnLock();
            //myIBobject.SetActiveMesh(i_v, i_f);
            //bill.Draw(myIBobject);
        }

        myIBobject.UnLock();
        myIBobject.SetActiveMesh(i_v, i_f);
        bill.Draw(myIBobject);
    }
    // bufsize=256
    public void Printf(Color myColor, Vector2 p, string Text, params object[] args)
    {
        throw new System.NotImplementedException();
    }
    public void SetCustomColor(int num, Color color)
    {
        if (num < 0 | num >= MAX_COLORS) return;
        custom_colors[num] = color;
    }
    public Color GetCustomColor(int num)
    {
        if (num < 0 | num >= MAX_COLORS) return Color.black;
        return custom_colors[num];
    }

    public void AddRef()
    {
        return;
    }

    public int Release()
    {
        return 0;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class FontData : IStormImportable<FontData>
{
    public string bitmap
    {
        get
        {
            return new string(bitmapArray).Trim('\0');
        }
    }
    //TODO Реализовать полностью
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    char[] bitmapArray = new char[32];
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public CharCoord[] table = new CharCoord[224];

    public FontData Import(Stream st)
    {
        FontData res = StormFileUtils.ReadStruct<FontData>(st);
        res.table = new CharCoord[224];

        for (int i = 0; i < 224; i++)
        {
            res.table[i] = StormFileUtils.ReadStruct<CharCoord>(st, st.Position);
        }
        return res;
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CharCoord
{
    public short x0, y0, x1, y1;
    public CharCoord() { }
    public Vector4 Get()
    {
        return new Vector4(x0 * (OO256) - .5f * OO256, y0 * (OO256) - .5f * OO256, x1 * (OO256) + .5f * OO256, y1 * (OO256) + .5f * OO256);
        //return new Vector4(x0 * (OO256), y0 * (OO256), x1 * (OO256) , y1 * (OO256) );
    }

    public override string ToString()
    {
        return (x0, y0, x1, y1).ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.IO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PARTICLE_DATA : IStormImportable<PARTICLE_DATA>
{
    public ParticlesDefines.BirthType BirthType; //int
    public ParticlesDefines.DrawType DrawType; //int
    public short IsLocal; //bool in src
    public short LocalBorn; //bool in src
    public float SelfRadius;

    public DWORD mUnused;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] Mapping = new float[4];
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
    public char[] TextureName = new char[24];

    public Vector3 BirthRadius;
    public Vector3 BirthBase;
    public Vector3 SpeedRadius;
    public Vector3 SpeedBase;

    public int MaxParts;

    public float BirthPeriod;
    public float BirthFrequence;
    public float DecaySpeed;

    public float Friction;
    public Vector3 Gravity;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public uint[] Color = new uint[256];
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public float[] Size = new float[256];
    public float SceneLightCoeff;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("Particle system\n");
        sb.Append($"BirthType {BirthType}\n");
        sb.Append($"DrawType {DrawType}\n");
        sb.Append($"SelfRadius {SelfRadius}\n");

        sb.Append($"BirthRadius {BirthRadius}\n");
        sb.Append($"BirthBase {BirthBase}\n");
        sb.Append($"SpeedRadius {SpeedRadius}\n");
        sb.Append($"SpeedBase {SpeedBase}\n");

        sb.Append($"MaxParts {MaxParts}\n");

        sb.Append($"BirthPeriod {BirthPeriod}\n");
        sb.Append($"BirthFrequence {BirthFrequence}\n");
        sb.Append($"DecaySpeed {DecaySpeed}\n");

        sb.Append($"Friction {Friction}\n");
        sb.Append($"Gravity {Gravity}\n");
        sb.Append($"SceneLightCoeff {SceneLightCoeff}\n");

        sb.Append($"Texture name [{GetTextureName()}]\n");
        sb.Append(GetMapping() + "\n");


        return sb.ToString();
    }
    /*
   // parj methods
   char* GetBirthType(int) const;
   char* GetDrawType(int) const;
   void ProcessColor(int Start, int End);
   void ProcessSize(int Start, int End);
   int Import(char*);
   bool Export(char*) const;*/
    /// <summary>
    /// Имя текстуры в классе - массив из 0-24 символов, оканчивающийся \0
    /// Этот метод преобразует массив в sting, отбрасывая \0
    /// </summary>
    /// <returns></returns>
    public string GetTextureName()
    {
        string res = new string(TextureName);
        res = res.Replace("\0", string.Empty);

        return res;
    }
    private string GetMapping()
    {
        string res = $"Mapping: [{Mapping[0]}:{Mapping[1]}] [{Mapping[2]}:{Mapping[3]}]";
        
        return res;
    }

    public PARTICLE_DATA Import(Stream ms)
    {
        return StormFileUtils.ReadStruct<PARTICLE_DATA>(ms);
    }
}
public class ParticlesDefines
{
    public enum BirthType
    {
        PB_NORMAL = 0,
        PB_TOROIDAL = 1,
        PB_SPHERICAL = 2,
        PB_TORUS = 3

    }

    public enum DrawType
    {
        /// <summary>
        /// Texture_Add
        /// </summary>
        PD_ADDA = 0,
        /// <summary>
        /// Alpha_Texture_Blend
        /// </summary>
        PD_ATEXTURE = 2,
        /// <summary>
        /// Alpha_Texture_Inv_Draw
        /// </summary>
        PD_ATEXTUREINV = 4,
        /// <summary>
        /// Vector
        /// </summary>
        PD_VECTOR = 3
    }
}


public struct MyGradientKey
{

    public float t { get; set; }
    public Color Color { get; set; } // comes with r, g, b, alpha

    public MyGradientKey(float t, Color color)
    {
        this.t = t;
        this.Color = color;
    }

}

public class StormGradient2 : Gradient
{

}
public class StormGradient : Gradient
{
    //private new List<GradientColorKey> colorKeys;
    //private new List<GradientAlphaKey> alphaKeys;
    private new GradientAlphaKey[] alphaKeys;
    private new GradientColorKey[] colorKeys;
    private int keycount=256;

    public StormGradient()
    {
        //colorKeys = new List<GradientColorKey>();
        //alphaKeys = new List<GradientAlphaKey>();
        colorKeys = new GradientColorKey[keycount];
        alphaKeys = new GradientAlphaKey[keycount];
        for (int i=0;i< keycount; i++)
        {
            colorKeys[i] = new GradientColorKey(Color.green, i / (float)keycount);
            alphaKeys[i] = new GradientAlphaKey(1f, i / (float) keycount);
        }
    }

    public new void SetKeys(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys)
    {
        //this.colorKeys.AddRange(colorKeys);
        //this.alphaKeys.AddRange(alphaKeys);
        this.colorKeys = colorKeys;
        this.alphaKeys = alphaKeys;

    }

    public new Color Evaluate(float time)
    {
        GradientColorKey cPrev, cPast;
        GradientAlphaKey aPrev, aPast;

        return Color.green;
    }


}
public class MyGradient
{

    List<MyGradientKey> _keys;

    public MyGradient()
    {
        _keys = new List<MyGradientKey>();
    }

    public int Count => _keys.Count;

    public MyGradientKey this[int index]
    {
        get => _keys[index];
        set { _keys[index] = value; sortKeys(); }
    }

    public void AddKey(float t, Color color)
      => AddKey(new MyGradientKey(t, color));

    public void AddKey(MyGradientKey key)
    {
        _keys.Add(key);
        sortKeys();
    }

    public void InsertKey(int index, float t, Color color)
      => InsertKey(index, new MyGradientKey(t, color));

    public void InsertKey(int index, MyGradientKey key)
    {
        _keys.Insert(index, key);
        sortKeys();
    }

    public void RemoveKey(int index)
    {
        _keys.RemoveAt(index);
        sortKeys();
    }

    public void RemoveInRange(float min, float max)
    {
        for (int i = _keys.Count - 1; i >= 0; i--)
            if (_keys[i].t >= min && _keys[i].t <= max) _keys.RemoveAt(i);
        sortKeys();
    }

    public void Clear() => _keys.Clear();

    void sortKeys() => _keys.Sort((a, b) => a.t.CompareTo(b.t));

    (int l, int r) getNeighborKeys(float t)
    {
        var l = Count - 1;

        for (int i = 0; i <= l; i++)
        {
            if (_keys[i].t >= t)
            {
                if (i == 0) return (-1, i);
                return (i - 1, i);
            }
        }

        return (l, -1);
    }

    public Color Evaluate(float t)
    {
        if (Count == 0) return new Color(0f, 0f, 0f, 0f);

        var n = getNeighborKeys(t);

        if (n.l < 0) return _keys[n.r].Color;
        else if (n.r < 0) return _keys[n.l].Color;

        return Color.Lerp(
          _keys[n.l].Color,
          _keys[n.r].Color,
          Mathf.InverseLerp(_keys[n.l].t, _keys[n.r].t, t)
        );
    }

}
using System;
using UnityEngine;

public class RandomGenerator
{
    private static RandomGenerator instance;
    private System.Random randomizer;

    const int RAND__MAX = 0x10000;
    const float OO_RAND__MAX = 1f / RAND__MAX;

    public static RandomGenerator GetInstance()
    {
        if (instance == null) instance = new RandomGenerator();
        return instance;
    }

    public RandomGenerator()
    {
        randomizer = new System.Random();
    }

    public static int Rand()
    {
        int value = GetInstance().randomizer.Next();
        return (value & 0x00FFFF00) >> 8;
    }
    public static float Rand01()
    {
        return Rand() * OO_RAND__MAX;
    }

    public static int RandN( int x)
    {
        return Rand() * x / RAND__MAX;
    }
    public  static float getUniformRandom(float min, float max, float rand_01)
    {
        return min * (1 - rand_01) + max * rand_01;
    }

    public static float getUniformRandom(float min, float max)
    {
        return getUniformRandom(min,max,Rand01());
    }


    internal static float Rnd(float mn, float bnd)
    {
        return mn + Rand01() * bnd;
    }
}
using UnityEngine;

// ************************************* IterElem **************************************
public struct ITER_ELEM
{
    public const float IterPresision = 0.001f;
    public int Box, dBox;
    public float Clip, dClip;

    public ITER_ELEM(float a, float ooda, float cellsize, float oocellsize)
    {
        if (Mathf.Abs(ooda) > (1 / IterPresision))
        {
            Box = (int)(a * oocellsize - .5f);
            dBox = 0;
            dClip = 0;
            //Clip = FLT_MAX;
            Clip = float.MaxValue;
        }
        else
        {
            dBox = (ooda > 0) ? 1 : -1;
            Box = (int)(a * oocellsize - .5f + dBox * IterPresision);
            dClip = dBox * cellsize;
            Clip = ((dClip + cellsize) * .5f + Box * cellsize - a) * ooda;
            dClip *= ooda;
        }
    }

    public void Next()
    {
        Box += dBox;
        Clip += dClip;
    }
};

// ************************************* ITERATION2D ***********************************
public struct ITERATION2D
{
    public ITER_ELEM xIter, yIter;

    //x,y -start point ,dx,dy :dx=D.x,dy=D.y,D:|D|=1 -direction
    public ITERATION2D(float x, float y, float dx, float dy, float cellsize, float oocellsize)
    {
        xIter = new ITER_ELEM(x, 1 / dx, cellsize, oocellsize);
        yIter = new ITER_ELEM(y, 1 / dy, cellsize, oocellsize);
    }
    public void Next()
    {
        if (xIter.Clip < yIter.Clip)   // if dx=dist.x/norma(dist)
            xIter.Next();
        else yIter.Next();//  & dy=dist.y/norma(dist) =>result=CurDist
    }                                  //      ||
                                       //      VV
    public float GetPosition()
    {
        return (xIter.Clip < yIter.Clip) ?
                 xIter.Clip : yIter.Clip;
    }
};


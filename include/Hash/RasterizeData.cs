using UnityEngine;

public struct RasterizeData
{
    public int starty;
    public int nlines;
    public readonly int[] left;
    public readonly int[] right;

    public override string ToString()
    {
        return base.ToString() + string.Format(" starty {0} nlines {1} left {2} right {3}",starty, nlines,left,right);
    }

    public RasterizeData(int y, int n,  int[] l, int[] r)
    {
        starty = y;
        nlines = n;
        left = l;
        right = r;
    }
    public int Rasterize(RasterizeEnumer e){
        int count = 0;
        //Debug.LogFormat("Rasterize from Y {0} lines {1} left {2} right {3}",starty,nlines,left,right);
        for (int y=0; y<nlines; ++y) {
            int x = left[y], x2 = right[y];
            Debug.Log(string.Format("Rasterize @ {0} {1}:{2}",y, x, x2));
            for (;x<=x2; ++x) {
                count++;
                if (!e.ProcessElement(x, y+starty)) return count;
            }
        }
        return count;
    }
}





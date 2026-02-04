using WORD = System.UInt16;

public class T_LOD
{
    static int[][] cback_vidx ={
        new int[]{ 0,13, 5,15, 2},
        new int[]{17, 9,21,11,18},
        new int[]{ 7,22, 4,23, 8},
        new int[]{20,10,24,12,19},
        new int[]{ 1,14, 6,16, 3}
    };

    const int lod_nPoints = 25;
    const int lod_nLods = 4;

    int[] vertices = new int[lod_nLods];
    int[] vertex_x = new int[lod_nPoints];
    int[] vertex_z = new int[lod_nPoints];
    public int[][] back_vidx = new int[5][]; //было [5][5]
    //WORD Tris[2][2][16][8 * 3];
    public T_LOD()
    {
        int i, x, z, xi, zi;

        vertices[0] = 4; vertices[0] = 5; vertices[0] = 16;

        //init jagged array
        for (x = 0; x < 5; x++)
        {
            back_vidx[x] = new int[5];
        }

        for (z = 0; z < 5; z++)
        {
            for (x = 0; x < 5; x++)
            {
                back_vidx[x][z] = cback_vidx[x][z];
                vertex_x[back_vidx[x][z]] = x;
                vertex_z[back_vidx[x][z]] = z;
            }
        }

    }
    public void SetSqTris(ref WORD[] itri, int x, int z, int dim, bool symmetry, int offset = 0)
    {
        if (symmetry)
        {//symmetry
            itri[offset + 0] = (ushort)back_vidx[x][z];
            itri[offset + 2] = (ushort)back_vidx[x + dim][z + dim];
            itri[offset + 1] = (ushort)back_vidx[x][z + dim];
            itri[offset + 3] = (ushort)back_vidx[x][z];
            itri[offset + 5] = (ushort)back_vidx[x + dim][z];
            itri[offset + 4] = (ushort)back_vidx[x + dim][z + dim];
        }
        else
        {
            itri[offset + 0] = (ushort)back_vidx[x][z];
            itri[offset + 2] = (ushort)back_vidx[x + dim][z];
            itri[offset + 1] = (ushort)back_vidx[x][z + dim];
            itri[offset + 3] = (ushort)back_vidx[x + dim][z + dim];
            itri[offset + 5] = (ushort)back_vidx[x][z + dim];
            itri[offset + 4] = (ushort)back_vidx[x + dim][z];
        }
    }
};

using System.IO;
using UnityEngine;

public static class StormImportableStatic
{
    public static PatrolInfo Import(this Stream st)
    {
        Vector3[] Delta = new Vector3[PatrolInfo.PATROL_DIM];
        int memberCount = (int)(st.Length / (sizeof(float) * 3));
        Delta = StormFileUtils.ReadStructs<Vector3>(st, 0, memberCount);

        PatrolInfo patrol = new PatrolInfo();
        patrol.Delta = Delta;
        return patrol;
    }
}
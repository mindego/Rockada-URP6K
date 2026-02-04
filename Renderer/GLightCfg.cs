using System.IO;
using UnityEngine;

class GLightCfg: IStormImportable<GLightCfg>
{
    public Color diffuse;
    public Color ambient;
    public Color specular;
    public float a, h;

    public GLightCfg Import(Stream st)
    {
        Vector3 tmpVector;
        st.Seek(0, SeekOrigin.Begin);
        tmpVector = StormFileUtils.ReadStruct<Vector3>(st);
        diffuse = new Color(tmpVector.x,tmpVector.y,tmpVector.z);
        tmpVector = StormFileUtils.ReadStruct<Vector3>(st);
        ambient = new Color(tmpVector.x, tmpVector.y, tmpVector.z);
        tmpVector = StormFileUtils.ReadStruct<Vector3>(st);
        specular = new Color(tmpVector.x, tmpVector.y, tmpVector.z);
        a= StormFileUtils.ReadStruct<float>(st);
        h = StormFileUtils.ReadStruct<float>(st);

        return this;
    }
}
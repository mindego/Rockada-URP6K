using System.Collections;
using UnityEngine;

public static class ShapeGenerator
{
    public static Mesh MakeHemisphere(float radius = 100f, int nbLong = Sky.SKY_FACES, int nbLat = Sky.SKY_FACES)
    {
        //Debug.Log(string.Format("Making sphere radius {0} long {1} lat {2}",radius,nbLong,nbLat));
        Mesh mesh = new Mesh();
        bool hemisphere = true;
        bool fullUvRange = true;
        //float radius = 100f;

        // Longitude |||
        //int nbLong = SKY_FACES; //24
        // Latitude ---
        //int nbLat = SKY_FACES; //16

        #region Vertices
        Vector3[] vertices = new Vector3[(nbLong + 1) * ((hemisphere) ? nbLat / 2 : nbLat) + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        //for (int lat = 0; lat < nbLat; lat++)
        for (int lat = 0; lat < ((hemisphere) ? nbLat / 2 : nbLat); lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius;
        #endregion

        #region Normals		
        Vector3[] normals = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normals[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        //for (int lat = 0; lat < nbLat; lat++)
        for (int lat = 0; lat < ((hemisphere) ? nbLat / 2 : nbLat); lat++)
            for (int lon = 0; lon <= nbLong; lon++)
            {
                //uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
                if (!hemisphere || fullUvRange) uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
                else uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat / 2));
            }
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < ((hemisphere) ? nbLat / 2 : nbLat) - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }


        if (!hemisphere)
        {
            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        FlipMesh(ref mesh);
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void FlipMesh(ref Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        int swap;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            swap = triangles[i + 1];
            triangles[i + 1] = triangles[i + 2];
            triangles[i + 2] = swap;
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return;
    }
}

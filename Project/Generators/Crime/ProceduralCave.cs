using UnityEngine;

namespace EtT.Generators.Crime
{
    public static class ProceduralCave
    {
        public static void Build(Transform parent, CaveParams p, Material mat)
        {
            var go = new GameObject("Cave");
            go.transform.SetParent(parent, false);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            if (mat) mr.sharedMaterial = mat;

            mf.sharedMesh = GenerateTunnelMesh(p);
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = mf.sharedMesh;
        }

        private static Mesh GenerateTunnelMesh(CaveParams p)
        {
            int sides = Mathf.Max(6, p.sides);
            int segs = Mathf.Max(4, p.segments);

            var verts = new Vector3[(sides + 1) * (segs + 1)];
            var norms = new Vector3[verts.Length];
            var uvs = new Vector2[verts.Length];
            var tris = new int[sides * segs * 6];

            for (int z = 0; z <= segs; z++)
            {
                float t = (float)z / segs;
                float zPos = Mathf.Lerp(0f, p.length, t);
                float r = p.radius * (1f + 0.25f * Mathf.Sin(t * 4f));
                for (int i = 0; i <= sides; i++)
                {
                    float a = (i / (float)sides) * Mathf.PI * 2f;
                    float noise = Mathf.PerlinNoise(Mathf.Cos(a) * p.noiseFreq + t, Mathf.Sin(a) * p.noiseFreq + t) * p.noiseAmp;
                    float rad = Mathf.Max(0.2f, r + (noise - p.noiseAmp * 0.5f));
                    Vector3 v = new Vector3(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad, zPos);
                    int idx = z * (sides + 1) + i;
                    verts[idx] = v;
                    norms[idx] = (new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f)).normalized;
                    uvs[idx] = new Vector2(i / (float)sides, t);
                }
            }

            int ti = 0;
            for (int z = 0; z < segs; z++)
            {
                int row = z * (sides + 1);
                int next = (z + 1) * (sides + 1);
                for (int i = 0; i < sides; i++)
                {
                    int a = row + i;
                    int b = row + i + 1;
                    int c = next + i;
                    int d = next + i + 1;

                    tris[ti++] = a; tris[ti++] = c; tris[ti++] = b;
                    tris[ti++] = b; tris[ti++] = c; tris[ti++] = d;
                }
            }

            var mesh = new Mesh { name = "CaveTunnel" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
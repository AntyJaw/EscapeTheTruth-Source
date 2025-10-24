using UnityEngine;

namespace EtT.Generators.Crime
{
    public static class ProceduralEnvironment
    {
        public static void BuildGroundDisk(Transform parent, float radius, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Ground";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(radius, 0.02f, radius);
            go.transform.localPosition = Vector3.zero;
            var r = go.GetComponent<MeshRenderer>();
            if (r && mat) r.sharedMaterial = mat;
        }
    }
}
using UnityEngine;

namespace EtT.Generators.Crime
{
    public static class BodyGen
    {
        public static void Create(Transform parent, BodyParams p, Material mat)
        {
            var root = new GameObject("Victim_Mannequin");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = p.localPos;
            root.transform.localRotation = Quaternion.Euler(0f, p.rotationY, 0f);

            // „manekin” z prymitywów
            CreatePart(root.transform, PrimitiveType.Capsule, new Vector3(0f, 0.9f, 0f), new Vector3(0.25f, 0.9f, 0.25f), mat); // tułów
            CreatePart(root.transform, PrimitiveType.Sphere, new Vector3(0f, 2.0f, 0f), new Vector3(0.25f, 0.25f, 0.25f), mat); // głowa
            CreatePart(root.transform, PrimitiveType.Capsule, new Vector3(0.3f, 1.6f, 0f), new Vector3(0.15f, 0.5f, 0.15f), mat, new Vector3(0f, 0f, 30f)); // ręka
            CreatePart(root.transform, PrimitiveType.Capsule, new Vector3(-0.3f, 1.6f, 0f), new Vector3(0.15f, 0.5f, 0.15f), mat, new Vector3(0f, 0f, -30f)); // ręka
            CreatePart(root.transform, PrimitiveType.Capsule, new Vector3(0.15f, 0.2f, 0f), new Vector3(0.15f, 0.7f, 0.15f), mat); // noga
            CreatePart(root.transform, PrimitiveType.Capsule, new Vector3(-0.15f, 0.2f, 0f), new Vector3(0.15f, 0.7f, 0.15f), mat);
        }

        private static void CreatePart(Transform parent, PrimitiveType t, Vector3 localPos, Vector3 scale, Material m, Vector3? euler = null)
        {
            var go = GameObject.CreatePrimitive(t);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            if (euler.HasValue) go.transform.localRotation = Quaternion.Euler(euler.Value);
            var r = go.GetComponent<MeshRenderer>();
            if (r && m) r.sharedMaterial = m;
        }
    }
}
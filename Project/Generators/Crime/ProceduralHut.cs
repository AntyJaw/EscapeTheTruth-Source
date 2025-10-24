using UnityEngine;

namespace EtT.Generators.Crime
{
    public static class ProceduralHut
    {
        public static void Build(Transform parent, HutParams p, Material mat)
        {
            var hut = new GameObject("Hut");
            hut.transform.SetParent(parent, false);

            // Ściany (prostopadłościan)
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "Walls";
            walls.transform.SetParent(hut.transform, false);
            walls.transform.localScale = new Vector3(p.size.x, p.height, p.size.y);
            walls.transform.localPosition = new Vector3(0f, p.height/2f, 0f);
            ApplyMat(walls, mat);

            // Dach (klin / gable)
            var roofL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofL.name = "RoofL";
            roofL.transform.SetParent(hut.transform, false);
            roofL.transform.localScale = new Vector3(p.size.x, 0.15f, p.size.y);
            roofL.transform.localPosition = new Vector3(0f, p.height + p.roofHeight*0.5f, 0f);
            roofL.transform.localRotation = Quaternion.Euler(p.roofHeight*10f, 0f, 10f);
            ApplyMat(roofL, mat);

            var roofR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofR.name = "RoofR";
            roofR.transform.SetParent(hut.transform, false);
            roofR.transform.localScale = new Vector3(p.size.x, 0.15f, p.size.y);
            roofR.transform.localPosition = new Vector3(0f, p.height + p.roofHeight*0.5f, 0f);
            roofR.transform.localRotation = Quaternion.Euler(p.roofHeight*10f, 0f, -10f);
            ApplyMat(roofR, mat);

            // Drzwi (otwór „wizualny” – realizujemy jako inny materiał + collider drzwi)
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(hut.transform, false);
            door.transform.localScale = new Vector3(p.doorWidth, p.doorHeight, 0.1f);
            door.transform.localPosition = new Vector3(0f, p.doorHeight/2f, p.size.y/2f - 0.06f);
            ApplyMat(door, mat);

            // Okna (symbolicznie)
            for (int i = 0; i < p.windowCount; i++)
            {
                var win = GameObject.CreatePrimitive(PrimitiveType.Quad);
                win.name = $"Window_{i+1}";
                win.transform.SetParent(hut.transform, false);
                float side = (i % 2 == 0) ? -1f : +1f;
                win.transform.localPosition = new Vector3(side * (p.size.x/2f - 0.06f), p.height*0.7f, 0f);
                win.transform.localRotation = Quaternion.Euler(0f, side < 0 ? 90f : -90f, 0f);
                win.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
                var mr = win.GetComponent<MeshRenderer>();
                if (mr && mat) mr.sharedMaterial = mat;
            }
        }

        private static void ApplyMat(GameObject go, Material m)
        {
            var r = go.GetComponent<MeshRenderer>();
            if (r && m) r.sharedMaterial = m;
        }
    }
}
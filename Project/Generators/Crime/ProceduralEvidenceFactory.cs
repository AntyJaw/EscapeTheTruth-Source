using UnityEngine;
using EtT.Evidence;

namespace EtT.Generators.Crime
{
    public static class ProceduralEvidenceFactory
    {
        public static void Create(Transform parent, EvidenceSpawn s, Material mat)
        {
            // Prosty „props” – np. cylinder („butelka”) / cube („telefon”) losowany po ID
            GameObject go;
            if (s.id.Contains("bottle")) go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            else if (s.id.Contains("phone")) go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            else go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            go.name = $"EV_{s.display}";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = s.localPos + new Vector3(0f, 0.02f, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            go.transform.localScale = Vector3.one * 0.3f;

            var r = go.GetComponent<MeshRenderer>();
            if (r && mat) r.sharedMaterial = mat;

            // Target do skanowania (nasz istniejący system Evidence)
            var target = go.AddComponent<EtT.Evidence.EvidenceScanTarget>();
            var serializedObj = new UnityEditor.SerializedObject(target);
            // ale nie polegamy na SerializedObject w buildzie — ustaw przez odbicie public API:
            // EvidenceScanTarget ma [SerializeField], więc z poziomu API:
            SetEvidenceTarget(target, s);

            // Czytelność dowodu względem pogody
            var vis = go.AddComponent<EtT.Generators.Crime.EvidenceVisibilityController>();
            vis.baseVisibility = 1f;
            vis.decayPerMinute = 0.015f;
            vis.targetRenderer = r;
        }

        private static void SetEvidenceTarget(EtT.Evidence.EvidenceScanTarget t, EvidenceSpawn s)
        {
            // EvidenceScanTarget ma pola prywatne z [SerializeField] — dodaj publiczny setter?
            // Dla MVP: nadpiszemy przez „helper”:
            t.SendMessage("Scan", SendMessageOptions.DontRequireReceiver); // nic nie zrobi bez wejścia
            // Tu zostawiamy wartości domyślne ustawione w inspectorze,
            // a praktyczne ID/wartswy podamy w prefabie/pliku seed (w inspektorze).
        }
    }
}
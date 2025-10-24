#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using EtT.Data;          // PlayerBalanceConfig, ClassDefinition, PlayerClassCatalog
using EtT.PlayerClasses; // CharacterClass
using EtT.PlayerStats;   // Attributes

public static class EtTDefaultAssetsGenerator
{
    private const string RES_DIR = "Assets/_Project/Resources";
    private const string DATA_DIR = "Assets/_Project/Data/PlayerClassDefinitions";

    [MenuItem("Tools/EtT/Generate Default Configs")]
    public static void GenerateAll()
    {
        EnsureDir(RES_DIR);
        EnsureDir(DATA_DIR);

        // 1) PlayerBalanceConfig.asset -> Resources/PlayerBalanceConfig.asset
        var balancePath = Path.Combine(RES_DIR, "PlayerBalanceConfig.asset");
        CreatePlayerBalanceIfMissing(balancePath);

        // 2) SkillDecayConfig.asset -> Resources/SkillDecayConfig.asset
        var decayPath = Path.Combine(RES_DIR, "SkillDecayConfig.asset");
        CreateSkillDecayIfMissing(decayPath);

        // 3) ClassDefinitions (Agent/Laborant/Informatyk/Antropolog/Ezoteryk) -> Data/PlayerClassDefinitions
        var agent  = CreateClassIfMissing(Path.Combine(DATA_DIR, "Class_Agent.asset"),
            CharacterClass.Agent, new Attributes{ Perception=2, Technique=1, Composure=1 }, 80,100,80,50,
            new []{ "interrogation" }, hidden:false);

        var labo   = CreateClassIfMissing(Path.Combine(DATA_DIR, "Class_Laborant.asset"),
            CharacterClass.Laborant, new Attributes{ Perception=1, Technique=3, Composure=0 }, 75,95,70,45,
            new []{ "lab_toxicology" }, hidden:false);

        var inf    = CreateClassIfMissing(Path.Combine(DATA_DIR, "Class_Informatyk.asset"),
            CharacterClass.Informatyk, new Attributes{ Perception=1, Technique=2, Composure=1 }, 70,95,75,40,
            new []{ "hacker_gps" }, hidden:false);

        var anth   = CreateClassIfMissing(Path.Combine(DATA_DIR, "Class_Antropolog.asset"),
            CharacterClass.Antropolog, new Attributes{ Perception=1, Technique=0, Composure=2 }, 75,100,70,45,
            new []{ "symbols" }, hidden:false);

        var esot   = CreateClassIfMissing(Path.Combine(DATA_DIR, "Class_Ezoteryk.asset"),
            CharacterClass.Ezoteryk, new Attributes{ Perception=1, Technique=0, Composure=2 }, 70,100,70,50,
            new []{ "ritual_sight" }, hidden:true);

        // 4) PlayerClassCatalog -> Resources/PlayerClassCatalog.asset (bez Ezoteryka w katalogu domyślnym)
        var catalogPath = Path.Combine(RES_DIR, "PlayerClassCatalog.asset");
        CreateCatalogIfMissing(catalogPath, new []{ agent, labo, inf, anth });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("EtT", "Default configs generated / verified.", "OK");
    }

    private static void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parent = Path.GetDirectoryName(path).Replace("\\","/");
            var name   = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void CreatePlayerBalanceIfMissing(string assetPath)
    {
        var existing = AssetDatabase.LoadAssetAtPath<PlayerBalanceConfig>(assetPath);
        if (existing) return;

        var so = ScriptableObject.CreateInstance<PlayerBalanceConfig>();
        so.maxLevel = 50;
        so.pointsPerLevel = 2;
        so.startingAttributePoints = 5;
        so.missionRadiusReductionPerPerceptionPct = 2f;
        so.evidenceDegradationReductionPerComposurePct = 2f;
        so.analysisBoostPerTechniquePct = 3f;
        so.maxAttributeValue = 20;
        AssetDatabase.CreateAsset(so, assetPath);
        Debug.Log($"[EtT] Created PlayerBalanceConfig at {assetPath}");
    }

    private static void CreateSkillDecayIfMissing(string assetPath)
    {
        var existing = AssetDatabase.LoadAssetAtPath<SkillDecayConfigAsset>(assetPath);
        if (existing) return;

        var so = ScriptableObject.CreateInstance<SkillDecayConfigAsset>();
        so.config.daysToStartDecay = 14;
        so.config.levelPenaltyPerWeek = 0.2f;
        so.config.minEffectiveLevel = 1;
        AssetDatabase.CreateAsset(so, assetPath);
        Debug.Log($"[EtT] Created SkillDecayConfig at {assetPath}");
    }

    private static ClassDefinition CreateClassIfMissing(string assetPath, CharacterClass cc,
        Attributes startAttr, int hp, int psyche, int energy, int rep, string[] skillIds, bool hidden)
    {
        var existing = AssetDatabase.LoadAssetAtPath<ClassDefinition>(assetPath);
        if (existing) return existing;

        var so = ScriptableObject.CreateInstance<ClassDefinition>();
        so.characterClass = cc;
        so.startAttributes = startAttr;
        so.startHealth = hp;
        so.startPsychika = psyche;
        so.startEnergia = energy;
        so.startReputacja = rep;
        so.extraPoints = 5;
        so.classSkillIds = skillIds;
        so.hiddenUntilRite = hidden;
        AssetDatabase.CreateAsset(so, assetPath);
        Debug.Log($"[EtT] Created ClassDefinition {cc} at {assetPath}");
        return so;
    }

    private static void CreateCatalogIfMissing(string assetPath, ClassDefinition[] defs)
    {
        var existing = AssetDatabase.LoadAssetAtPath<PlayerClassCatalog>(assetPath);
        if (existing) return;

        var so = ScriptableObject.CreateInstance<PlayerClassCatalog>();
        so.classes = defs;
        AssetDatabase.CreateAsset(so, assetPath);
        Debug.Log($"[EtT] Created PlayerClassCatalog at {assetPath}");
    }
}
#endif
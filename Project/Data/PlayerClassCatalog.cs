using UnityEngine;

namespace EtT.Data
{
    [CreateAssetMenu(fileName = "PlayerClassCatalog", menuName = "EtT/Player/Class Catalog")]
    public class PlayerClassCatalog : ScriptableObject
    {
        public ClassDefinition[] classes;
    }
}
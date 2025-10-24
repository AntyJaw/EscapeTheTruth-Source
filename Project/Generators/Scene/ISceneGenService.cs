using EtT.Missions;

namespace EtT.Generators.SceneGen
{
    public interface ISceneGenService
    {
        /// <summary>Buduje paczkę sceny na podstawie misji/progu trudności/pogody itp.</summary>
        SceneBundle Generate(Mission mission, int seed);
    }
}
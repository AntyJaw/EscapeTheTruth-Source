namespace EtT.Generators.Wyrd
{
    public interface IWyrdService
    {
        /// <summary>
        /// Wyznacza centrum miejsca zbrodni, biorąc pod uwagę pozycję gracza, mobilność i ograniczenia świata.
        /// Zwraca pozycję geograficzną (lat, lng).
        /// </summary>
        World.Position ChooseCrimeCenter(World.Position playerPos, MobilityKind mobility, int playerLevel);

        public enum MobilityKind { Walk, Bike, Car }
    }
}
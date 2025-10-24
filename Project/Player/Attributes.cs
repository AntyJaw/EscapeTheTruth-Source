namespace EtT.PlayerStats
{
    public enum AttributeType { Perception, Technique, Composure }

    [System.Serializable]
    public struct Attributes
    {
        public int Perception;
        public int Technique;
        public int Composure;

        public int Get(AttributeType type) => type switch
        {
            AttributeType.Perception => Perception,
            AttributeType.Technique  => Technique,
            AttributeType.Composure  => Composure,
            _ => 0
        };

        public void Set(AttributeType type, int value)
        {
            switch (type)
            {
                case AttributeType.Perception: Perception = value; break;
                case AttributeType.Technique:  Technique = value; break;
                case AttributeType.Composure:  Composure = value; break;
            }
        }
    }
}
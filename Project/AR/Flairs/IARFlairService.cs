namespace EtT.AR.Flairs
{
    public interface IARFlairService
    {
        void Init();
        void OnAREnter();
        void OnARExit();
        // opcjonalnie: void Trigger(string id);
    }
}
namespace CampaignEditor.StartupHelpers
{
    public interface IAbstractFactory<T>
    {
        T Create();
    }
}
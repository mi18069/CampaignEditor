namespace CampaignEditor.Helpers
{
    public class CompletedValidationEventArgs
    {
        public string Date { get; private set; }
        public bool IsCompleted { get; private set; }

        public CompletedValidationEventArgs(string date, bool isCompleted)
        {
            Date = date;
            IsCompleted = isCompleted;
        }
    }
}

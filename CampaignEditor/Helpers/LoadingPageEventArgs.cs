namespace CampaignEditor.Helpers
{
    public class LoadingPageEventArgs
    {
        public string Message { get; private set; }
        public int progressBarValue { get; private set; }
        public LoadingPageEventArgs(string message, int progressBarValue = 0)
        {
            this.Message = message;
            this.progressBarValue = progressBarValue;
        }
    }
}

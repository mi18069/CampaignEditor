using Database.Entities;

namespace CampaignEditor.Helpers
{
    public class MediaPlansCalculatingProcess
    {
        public int TotalMediaPlans { get; private set; }
        public int ProgressPercentage { get; private set; }
        object progressLock = new object();
        private int _processedMediaPlans { get; set; }


        public MediaPlansCalculatingProcess(int totalMediaPlans)
        {
            TotalMediaPlans = totalMediaPlans;
            _processedMediaPlans = 0;
        }

        public void IncrementProcess(int count = 1)
        {
            _processedMediaPlans += count;
            ProgressPercentage = (int)((double)_processedMediaPlans / TotalMediaPlans * 100);
            // Increment the processed count for this list of media plans
            /*lock (progressLock)
            {
                ProcessedMediaPlans += count;
                ProgressPercentage = (int)((double)ProcessedMediaPlans / TotalMediaPlans * 100);
            }*/
        }
    }
}

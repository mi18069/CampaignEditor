using CampaignEditor.Controllers;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for RulerTimeline.xaml
    /// </summary>
    public partial class RulerTimeline : UserControl
    {

        public MediaPlanController _mediaPlanController;
        public MediaPlanTermController _mediaPlanTermController;
        public SpotController _spotController;

        int lineHeight = 3;

        public RulerTimeline()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            ruler.Initialize(lineHeight);
            canvas.Initialize(lineHeight);
        }

        public async Task LoadData(int cmpid, int chid, DateOnly date, int version)
        {
            List<TermTuple> termTuples = new List<TermTuple>();

            var mediaPlans = await _mediaPlanController.GetAllChannelCmpMediaPlans(chid, cmpid, version); 

            foreach (var mediaPlan in mediaPlans)
            {
                var mediaPlanTerms = await _mediaPlanTermController.GetAllNotNullMediaPlanTermsByXmpid(mediaPlan.xmpid);
                foreach(var mediaPlanTerm in mediaPlanTerms)
                {
                    if (mediaPlanTerm.date == date)
                    {
                        foreach (char spotcode in mediaPlanTerm.spotcode.Trim())
                        {
                            var spot = await _spotController.GetSpotsByCmpidAndCode(mediaPlan.cmpid, spotcode.ToString());
                            TermTuple termTuple = new TermTuple(mediaPlan, mediaPlanTerm, spot);
                            termTuples.Add(termTuple);
                        }
                    }
                    
                }
            }

            termTuples = termTuples.OrderBy(tt => tt.MediaPlan.stime).ToList();

            DrawTermsInCanvas(termTuples);
        }

        private void DrawTermsInCanvas(List<TermTuple> termTuples)
        {
            canvas.ClearCanvas();

            for (int i=0; i<termTuples.Count(); i++)
            {
                TermTuple termTuple = termTuples[i];

                if (termTuple.MediaPlan.etime != null)
                {
                    int height = TimeFormat.CalculateMinutesBetweenRepresentatives(
                        termTuple.MediaPlan.stime, termTuple.MediaPlan.etime);
                    int offset = TimeFormat.CalculateMinutesBetweenRepresentatives(
                        "02:00", termTuple.MediaPlan.stime) + 3; 

                    string name = termTuple.Spot.spotname.Trim();
                    canvas.DrawTermRectangle(height, offset, 0, name);
                }

                if (i + 1 < termTuples.Count())
                {
                    TermTuple termTupleNext = termTuples[i + 1];

                    if (termTupleNext.MediaPlan.etime != null &&
                        termTupleNext.MediaPlanTerm.xmptermid == termTuple.MediaPlanTerm.xmptermid)
                    {
                        int height = TimeFormat.CalculateMinutesBetweenRepresentatives(
                            termTupleNext.MediaPlan.stime, termTupleNext.MediaPlan.etime);
                        int offset = TimeFormat.CalculateMinutesBetweenRepresentatives(
                            "02:00", termTupleNext.MediaPlan.stime) + 3;

                        string name = termTupleNext.Spot.spotname.Trim();
                        canvas.DrawTermRectangle(height, offset, 1, name);
                        i++;
                    }
                }
                                
            }
        }
    }
}

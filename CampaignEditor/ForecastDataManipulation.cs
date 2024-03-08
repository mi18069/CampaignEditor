using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public class ForecastDataManipulation
    {
        private CampaignDTO _campaign;

        private SchemaController _schemaController;
        private ChannelController _channelController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private MediaPlanHistController _mediaPlanHistController;
        private MediaPlanRefController _mediaPlanRefController;
        private GoalsController _goalsController;
        private MediaPlanVersionController _mediaPlanVersionController;
        private DatabaseFunctionsController _databaseFunctionsController;

        private MediaPlanTermConverter _mpTermConverter;

        private MediaPlanConverter _mpConverter;
        private MediaPlanForecastData _forecastData;

        private ObservableRangeCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableRangeCollection<MediaPlanTuple>();

        private DateTime startDate;
        private DateTime endDate;

        public ForecastDataManipulation(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanRefRepository mediaPlanRefRepository,
            IGoalsRepository goalsRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            MediaPlanTermConverter mpTermConverter)
        {
            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanHistController = new MediaPlanHistController(mediaPlanHistRepository);
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _goalsController = new GoalsController(goalsRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);
            _mpTermConverter = mpTermConverter;
        }

        // For sending updates on ProgressBar when calculating AMRs
        public event EventHandler<LoadingPageEventArgs> UpdateProgressBar;

        private void OnUpdateProgressBar(string message, int value)
        {
            UpdateProgressBar?.Invoke(this, new LoadingPageEventArgs(message, value));
        }

        public void Initialize(CampaignDTO campaign,
            MediaPlanForecastData forecastData, 
            MediaPlanConverter mpConverter)
        {
            _campaign = campaign;
            _mpConverter = mpConverter;
            _forecastData = forecastData;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);
        }

        public async Task InsertData(int version)
        {
            // Inserting new MediaPlans in database
            await InsertCampaignMediaPlansFromSchemas(version);

            var mediaPlansByChannels = await GetMediaPlansByChannel(version);

            await StartAmrByListOfChannelMediaPlans(mediaPlansByChannels);


            // Getting newly updated mediaPlans
            mediaPlansByChannels = await GetMediaPlansByChannel(version);

            // Calculating additional values for mediaPlans
            // Making nChannel threads, and for each thread we'll run startAMRCalculation for each MediaPlan for that channel
            List<Task> calculatingTasks = new List<Task>();
            foreach (List<MediaPlanDTO> mediaPlanList in mediaPlansByChannels)
            {
                Task task = Task.Run(() => CalculateMPValuesForMediaPlans(mediaPlanList));
                calculatingTasks.Add(task);
            }
            await Task.WhenAll(calculatingTasks);

        }

        private async Task<List<List<MediaPlanDTO>>> GetMediaPlansByChannel(int version)
        {
            // Create tasks for each channel to fetch MediaPlanDTOs asynchronously
            List<Task<List<MediaPlanDTO>>> tasks = _forecastData.Channels.Select(async channel =>
            {
                // Fetch MediaPlanDTOs for the current channel asynchronously
                return (await _mediaPlanController.GetAllChannelCmpMediaPlans(channel.chid, _campaign.cmpid, version)).ToList();
            }).ToList();

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Extract results from completed tasks
            List<List<MediaPlanDTO>> mediaPlansByChannels = tasks.Select(task => task.Result).ToList();
            return mediaPlansByChannels;
        }

        // Adding mediaPlans in database from corresponding schemas
        private async Task InsertCampaignMediaPlansFromSchemas(int version)
        {
            List<Task> insertingTasks = new List<Task>();
            foreach (var channel in _forecastData.Channels)
            {
                Task task = Task.Run(() => InsertChannelMediaPlansFromSchemas(channel.chid, version));
                insertingTasks.Add(task);
            }

            // waiting for all tasks to finish
            await Task.WhenAll(insertingTasks);
        }

        private async Task InsertChannelMediaPlansFromSchemas(int chid, int version)
        {
            var schemas = await _schemaController.GetAllChannelSchemasWithinDateAndTime(
                chid, DateOnly.FromDateTime(startDate), 
                DateOnly.FromDateTime(endDate),
                _campaign.cmpstime, _campaign.cmpetime);

            foreach (var schema in schemas)
            {
                MediaPlanDTO mediaPlan = await SchemaToMP(schema, version, true);
                _ = await MediaPlanToMPTerm(mediaPlan);
            }

        }       

        private async Task StartAmrByListOfChannelMediaPlans(IEnumerable<IEnumerable<MediaPlanDTO>> mediaPlansByChannels)
        {
            int totalMediaPlans = mediaPlansByChannels.Sum(mpList => mpList.Count());
            MediaPlansCalculatingProcess calculatingProcess = new MediaPlansCalculatingProcess(totalMediaPlans);

            // No need for threading, this way is faster
            foreach (List<MediaPlanDTO> mediaPlanList in mediaPlansByChannels)
            {
                if (mediaPlanList.Count == 0)
                    continue;
                var channelName = _forecastData.Channels.First(ch => ch.chid == mediaPlanList[0].chid).chname.Trim();
                foreach (var mediaPlan in mediaPlanList)
                {
                    await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlan.xmpid);
                    calculatingProcess.IncrementProcess();
                    OnUpdateProgressBar($"GETTING REFERENCED DATA FOR CHANNELS...\n{channelName}\n{calculatingProcess.ProgressPercentage}%", calculatingProcess.ProgressPercentage);
                }

            }
        }

        private async Task CalculateMPValuesForMediaPlans(IEnumerable<MediaPlanDTO> mediaPlansDTO)
        {

            foreach (var mediaPlanDTO in mediaPlansDTO)
            {
                await CalculateMPValues(mediaPlanDTO);
            }
        }

        private async Task CalculateMPValues(MediaPlanDTO mediaPlanDTO)
        {
            var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
        }




        // reaching or creating mediaPlan
        private async Task<MediaPlanDTO> SchemaToMP(SchemaDTO schema, int version, bool shouldReplace = false)
        {
            MediaPlanDTO mediaPlan = null;
            // if already exist, fix conflicts
            if ((mediaPlan = await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid, version)) != null)
            {
                // if mediaPlan already exists, return that one
                if (AreEqualMediaPlanAndSchema(mediaPlan, schema) && !shouldReplace)
                    return mediaPlan;
                else
                {
                    if (!shouldReplace)
                    {
                        if (MessageBox.Show($"New program conflicts with existing:\n{mediaPlan.name}\n" +
                            $"This action will replace existing program with new one", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                            == MessageBoxResult.Cancel)
                        {
                            return mediaPlan;
                        }
                    }

                    await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlan.xmpid);
                    await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(mediaPlan.xmpid);
                    await _mediaPlanController.DeleteMediaPlanById(mediaPlan.xmpid);

                    var itemToRemove = _allMediaPlans.First(item => item.MediaPlan.schid == schema.id);
                    _allMediaPlans.Remove(itemToRemove);

                }

            }
            if (schema.blocktime == null || schema.blocktime.Length == 0)
            {
                string position = schema.position.Trim();
                if (position == "INS" || position == "BET")
                {
                    schema.blocktime = _schemaController.CalculateBlocktime(position, schema.stime, schema.etime);
                }
            }

            CreateMediaPlanDTO createMediaPlan = new CreateMediaPlanDTO(schema.id, _campaign.cmpid, schema.chid,
            schema.name.Trim(), version, schema.position, schema.stime, schema.etime, schema.blocktime,
            schema.days, schema.type, schema.special, schema.sdate, schema.edate, schema.progcoef,
            schema.created, schema.modified, 0, 100, 0, 100, 0, 100, 0, 100, 0, 0, 0, 0, 1, 1, 1, 0, true, 0);

            return await _mediaPlanController.CreateMediaPlan(createMediaPlan);


        }

        public async Task DeleteCampaignForecast(int cmpid)
        {
            var version = await _mediaPlanVersionController.GetLatestMediaPlanVersion(cmpid);
            if (version == null)
            {
                await _mediaPlanRefController.DeleteMediaPlanRefById(cmpid);
                return;
            }
            for (int i = version.version; i >= 1; i--)
            {
                await DeleteCampaignForecastVersion(cmpid, i);
            }
        }

        // Using for deleting processed data when error is encountered
        public async Task DeleteCampaignForecastVersion(int cmpid, int version)
        {
            var mediaPlans = await _mediaPlanController.GetAllMediaPlansByCmpid(cmpid, version);
            foreach (var mediaPlan in mediaPlans)
            {
                await DeleteMediaPlan(mediaPlan.xmpid);
            }

            if (version == 1)
            {
                await _mediaPlanVersionController.DeleteMediaPlanVersionById(cmpid);
                await _mediaPlanRefController.DeleteMediaPlanRefById(cmpid);
            }
            else
                await _mediaPlanVersionController.UpdateMediaPlanVersion(cmpid, version - 1);
        }

        private async Task DeleteMediaPlan(int xmpid)
        {
            await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(xmpid);
            await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(xmpid);
            await _mediaPlanController.DeleteMediaPlanById(xmpid);
        }

        public bool AreEqualMediaPlanAndSchema(MediaPlanDTO plan1, SchemaDTO schema)
        {
            return plan1.schid == schema.id &&
                   plan1.chid == schema.chid &&
                   plan1.name.Trim() == schema.name.Trim() &&
                   plan1.position == schema.position &&
                   plan1.stime == schema.stime &&
                   plan1.etime == schema.etime &&
                   //plan1.blocktime == schema.blocktime && // becaus we sometimes change blocktime when inserting mediaPlan 
                   plan1.days == schema.days &&
                   plan1.sdate == schema.sdate &&
                   plan1.edate == schema.edate;
        }

        private async Task<ObservableArray<MediaPlanTerm?>> MediaPlanToMPTerm(MediaPlanDTO mediaPlan)
        {
            List<DateTime> availableDates = GetAvailableDates(mediaPlan);
            DateTime started = startDate;

            int n = (int)(endDate - startDate).TotalDays;
            var mediaPlanDates = new ObservableArray<MediaPlanTerm?>(n + 1);

            List<DateTime> sorted = availableDates.OrderBy(d => d).ToList();

            for (int i = 0, j = 0; i <= n; i++)
            {
                if (j >= sorted.Count())
                {
                    mediaPlanDates[i] = null;
                    continue;
                }
                if (started.AddDays(i).Date == sorted[j].Date)
                {
                    CreateMediaPlanTermDTO mpTerm = new CreateMediaPlanTermDTO(mediaPlan.xmpid, DateOnly.FromDateTime(sorted[j]), null);
                    mediaPlanDates[i] = _mpTermConverter.ConvertFromDTO(await _mediaPlanTermController.CreateMediaPlanTerm(mpTerm));
                    j++;
                }
                else
                {
                    mediaPlanDates[i] = null;
                }
            }

            return mediaPlanDates;
        }
        private List<DateTime> GetAvailableDates(MediaPlanDTO mediaPlan)
        {
            List<DateTime> dates = new List<DateTime>();

            var sDate = startDate > mediaPlan.sdate.ToDateTime(TimeOnly.MinValue) ? startDate : mediaPlan.sdate.ToDateTime(TimeOnly.MinValue);
            var eDate = !mediaPlan.edate.HasValue ? endDate :
                        endDate < mediaPlan.edate.Value.ToDateTime(TimeOnly.MinValue) ? endDate : mediaPlan.edate.Value.ToDateTime(TimeOnly.MinValue);

            foreach (char c in mediaPlan.days)
            {
                switch (c)
                {
                    case '1':
                        var mondays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Monday);
                        foreach (DateTime date in mondays)
                            dates.Add(date);
                        break;
                    case '2':
                        var tuesdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Tuesday);
                        foreach (DateTime date in tuesdays)
                            dates.Add(date);
                        break;
                    case '3':
                        var wednesdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Wednesday);
                        foreach (DateTime date in wednesdays)
                            dates.Add(date);
                        break;
                    case '4':
                        var thursdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Thursday);
                        foreach (DateTime date in thursdays)
                            dates.Add(date);
                        break;
                    case '5':
                        var fridays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Friday);
                        foreach (DateTime date in fridays)
                            dates.Add(date);
                        break;
                    case '6':
                        var saturdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Saturday);
                        foreach (DateTime date in saturdays)
                            dates.Add(date);
                        break;
                    case '7':
                        var sundays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Sunday);
                        foreach (DateTime date in sundays)
                            dates.Add(date);
                        break;
                }

            }
            return dates;

        }

        private List<DateTime> GetWeekdaysBetween(DateTime startDate, DateTime endDate, DayOfWeek dayOfWeek)
        {
            var dates = new List<DateTime>();

            // calculate the number of days between the start date and the next occurrence of the day of the week
            var daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;

            // get the first date in the range
            var date = startDate.AddDays(daysToAdd);

            // add the day of the week repeatedly to get all the dates in the range
            while (date <= endDate)
            {
                dates.Add(date);
                date = date.AddDays(7);
            }

            return dates;
        }

        public async Task<IEnumerable<MediaPlanTuple>> MakeMediaPlanTuples(int version)
        {
            int daysNum = (int)(endDate - startDate).TotalDays;

            // Create tasks for each channel so every thread works with that channel mediaPlans
            List<Task<List<MediaPlanTuple>>> tasks = _forecastData.Channels.Select(async channel =>
            {
                // Make MediaPlanTuples for the current channel asynchronously
                return (await FillMPListByChannel(daysNum, channel.chid, version));
            }).ToList();

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Extract results from completed tasks
            List<MediaPlanTuple> allMediaPlans = tasks.SelectMany(task => task.Result).ToList();
            return allMediaPlans;
        }

        private async Task<List<MediaPlanTuple>> FillMPListByChannel(int daysNum, int chid, int version)
        {
            var mediaPlanTuples = new List<MediaPlanTuple>();
            var mediaPlansByChannel = await _mediaPlanController.GetAllMediaPlansByCmpidAndChannel(_campaign.cmpid, chid, version);
            foreach (MediaPlanDTO mediaPlan in mediaPlansByChannel)
            {
                List<MediaPlanTermDTO> mediaPlanTerms = (List<MediaPlanTermDTO>)await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
                var mediaPlanDates = new ObservableArray<MediaPlanTerm?>(daysNum + 1);
                for (int i = 0, j = 0; i <= daysNum; i++)
                {
                    if (j >= mediaPlanTerms.Count())
                    {
                        mediaPlanDates[i] = null;
                        continue;
                    }

                    if (DateOnly.FromDateTime(startDate.AddDays(i)) == mediaPlanTerms[j].date)
                    {
                        mediaPlanDates[i] = _mpTermConverter.ConvertFromDTO(mediaPlanTerms[j]);
                        j++;
                    }
                    else
                    {
                        mediaPlanDates[i] = null;
                    }
                }

                MediaPlanTuple mpTuple = new MediaPlanTuple(_mpConverter.ConvertFromDTO(mediaPlan, mediaPlanTerms), mediaPlanDates);
                mediaPlanTuples.Add(mpTuple);
            }
            return mediaPlanTuples;
        }

        #region Clear MP Tuples

        public async Task ClearAllMPTerms(IEnumerable<MediaPlanTuple> mediaPlanTuples)
        {
            foreach (var mediaPlanTuple in mediaPlanTuples)
            {
                await ClearMPTerms(mediaPlanTuple);
            }
        }
        public async Task ClearMPTerms(MediaPlanTuple mediaPlanTuple)
        {
            foreach (var mediaPlanTerm in mediaPlanTuple.Terms)
            {
                if (mediaPlanTerm == null ||
                    mediaPlanTerm.Spotcode == null)
                {
                    continue;
                }
                else if (mediaPlanTerm.Date > DateOnly.FromDateTime(DateTime.Today) &&
                    mediaPlanTerm.Spotcode.Trim() != "")
                {
                    mediaPlanTerm.Spotcode = null;
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                        new UpdateMediaPlanTermDTO(_mpTermConverter.ConvertToDTO(mediaPlanTerm)));
                }
            }
            await RecalculateMPValues(mediaPlanTuple);
        }


        private async Task RecalculateMPValues(MediaPlanTuple mediaPlanTuple)
        {
            var termsDTO = _mpTermConverter.ConvertToEnumerableDTO(mediaPlanTuple.Terms);
            _mpConverter.ComputeExtraProperties(mediaPlanTuple.MediaPlan, termsDTO, true);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlanTuple.MediaPlan)));
        }

        #endregion

        #region Adding New MediaPlans
        public async Task InsertNewForecastMediaPlans(IEnumerable<MediaPlanTuple> mediaPlanTuples, int newVersion)
        {
            foreach (MediaPlanTuple mediaPlanTuple in mediaPlanTuples)
            {
                MediaPlan mp = mediaPlanTuple.MediaPlan;
                MediaPlanDTO mpDTO = _mpConverter.ConvertToDTO(mp);
                CreateMediaPlanDTO createMPDTO = new CreateMediaPlanDTO(mpDTO);
                createMPDTO.version = newVersion;

                var mediaPlan = await _mediaPlanController.CreateMediaPlan(createMPDTO);

                if (mediaPlan != null)
                {
                    // Adding MediaPlanTerms in database
                    foreach (MediaPlanTerm mpTerm in mediaPlanTuple.Terms)
                    {
                        if (mpTerm != null)
                        {
                            MediaPlanTermDTO mpTermDTO = _mpTermConverter.ConvertToDTO(mpTerm);
                            CreateMediaPlanTermDTO createMPTermDTO = new CreateMediaPlanTermDTO(mpTermDTO);
                            createMPTermDTO.xmpid = mediaPlan.xmpid;
                            await _mediaPlanTermController.CreateMediaPlanTerm(createMPTermDTO);
                        }

                    }

                    // Adding MediaPlanHists in database
                    var hists = await _mediaPlanHistController.GetAllMediaPlanHistsByXmpid(mpDTO.xmpid);

                    foreach (var hist in hists)
                    {
                        CreateMediaPlanHistDTO createMpHistDTO = new CreateMediaPlanHistDTO(hist);
                        createMpHistDTO.xmpid = mediaPlan.xmpid;
                        await _mediaPlanHistController.CreateMediaPlanHist(createMpHistDTO);
                    }

                }
            }
                     
        }
        #endregion
    }
}

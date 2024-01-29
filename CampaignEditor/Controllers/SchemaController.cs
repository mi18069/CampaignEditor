using Database.DTOs.MediaPlanDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class SchemaController : ControllerBase
    {
        private readonly ISchemaRepository _repository;
        public SchemaController(ISchemaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateSchema(CreateSchemaDTO schemaDTO)
        {
            return await _repository.CreateSchema(schemaDTO);
        }

        public async Task<SchemaDTO> CreateGetSchema(CreateSchemaDTO schemaDTO)
        {
            return await _repository.CreateGetSchema(schemaDTO);
        }

        public async Task<SchemaDTO> GetSchemaById(int id)
        {
            return await _repository.GetSchemaById(id);
        }

        public async Task<SchemaDTO> GetSchemaByName(string name)
        {
            return await _repository.GetSchemaByName(name);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemas(int chid)
        {
            return await _repository.GetAllChannelSchemas(chid);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemasWithinDate(int chid, DateOnly sdate, DateOnly edate)
        {
            return await _repository.GetAllChannelSchemasWithinDate(chid, sdate, edate);
        }

        public async Task<List<SchemaDTO>> GetAllChannelSchemasWithinDateAndTime(int chid, DateOnly sdate, DateOnly edate, string stime, string etime = null)
        {
            var withinDates = await _repository.GetAllChannelSchemasWithinDate(chid, sdate, edate);
            List<SchemaDTO> withinDateAndTime = new List<SchemaDTO>();
            foreach (var schema in withinDates)
            {
                if (TimeFormat.Time5CharCompare(schema.stime.Substring(0, 5), stime.Substring(0, 5)) >= 0 
                    && (etime == null || schema.etime == null || TimeFormat.Time5CharCompare(schema.etime!.Substring(0, 5), etime.Substring(0, 5)) <= 0))
                {
                    withinDateAndTime.Add(schema);
                }
            }
            return withinDateAndTime;
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllSchemas()
        {
            return await _repository.GetAllSchemas();
        }

        public async Task<bool> UpdateSchema(UpdateSchemaDTO schemaDTO)
        {
            return await _repository.UpdateSchema(schemaDTO);
        }

        public async Task<bool> DeleteSchemaById(int id)
        {
            return await _repository.DeleteSchemaById(id);
        }

        public string CalculateBlocktime(string position, string timeFrom, string? timeTo = null)
        {
            string blocktime = "";

            if (position == "BET")
                blocktime = CalculateBetBlocktime(timeFrom);
            else
            {
                if (timeTo != null)
                    blocktime = CalculateInsBlocktime(timeFrom, timeTo);
                else
                {
                    blocktime = TimeFormat.ReturnGoodTimeFormat(timeFrom);
                }
            }

            return blocktime;
        }
        private string CalculateBetBlocktime(string timeFrom)
        {
            try
            {
                var goodTimeFormat = TimeFormat.ReturnGoodTimeFormat(timeFrom);

                // Convert to minutes
                int timeMins = TimeStringToMinutes(goodTimeFormat);

                // Calculate middle value
                int newValueMins = timeMins - 10;

                return TimeFormat.MinToRepresentative(newValueMins);
            }
            catch
            {
                return "";
            }
        }

        private string CalculateInsBlocktime(string timeFrom, string timeTo = "")
        {
            try
            {
                var goodTimeFormatFrom = TimeFormat.ReturnGoodTimeFormat(timeFrom);
                var goodTimeFormatTo = TimeFormat.ReturnGoodTimeFormat(timeTo);

                // Convert to minutes
                int startTime = TimeStringToMinutes(goodTimeFormatFrom);
                int endTime = TimeStringToMinutes(goodTimeFormatTo);

                if (startTime > endTime)
                {
                    return "";
                }
                // Calculate middle value
                int middleValue = (startTime + endTime)/2;

                return TimeFormat.MinToRepresentative(middleValue);
            }
            catch
            {
                return "";
            }
        }

        private int TimeStringToMinutes(string timeString)
        {
            string[] parts = timeString.Split(':');
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            return hours * 60 + minutes;
        }

    }
}

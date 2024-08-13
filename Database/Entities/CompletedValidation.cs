
using System;

namespace Database.Entities
{
    public class CompletedValidation
    {
        public int cmpid { get; set; }
        public string date { get; set; }
        public bool completed { get; set; }

        public CompletedValidation(int cmpid, string date, bool completed)
        {
            this.cmpid = cmpid;
            this.date = date;
            this.completed = completed;
        }
    }
}

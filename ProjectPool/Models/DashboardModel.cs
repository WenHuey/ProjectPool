using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class DashboardModel
    {
        public int UserId { get; set; }

        public int ActiveCount { get; set; }
        public int RunningCount { get; set; }
        public int ApplicationCount { get; set; }
        public int InterviewCount { get; set; }

        //Active
        public string A_Title { get; set; }
        public string A_Desc { get; set; }
        public string A_Cost { get; set; }
        public string A_Duration { get; set; }

        //Running
        public string R_Title { get; set; }
        public string R_Desc { get; set; }
        public string R_Cost { get; set; }
        public string R_Duration { get; set; }

        //Applicaton
        public string AP_Title { get; set; }
        public string AP_Tags { get; set; }
        public string AP_FullName { get; set; }
        public string AP_Duration { get; set; }

        //Interview
        public string I_Date { get; set; }
        public string I_IvTime { get; set; }
        public string I_Time { get; set; }
        public string I_FullName { get; set; }
        public bool datePassed { get; set; }

    }
}

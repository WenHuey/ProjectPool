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
        public int PendingCount { get; set; }


    }
}

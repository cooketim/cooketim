using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    public class NextHearingCourtDetails
    {
        public string courtName { get; set; }
        public string welshCourtName { get; set; }
        public string hearingDate { get; set; }
        public string hearingTime { get; set; }
        public string hearingWeekCommencing { get; set; }
        public string roomName { get; set; }
        public string welshRoomName { get; set; }
        public NowAddress courtAddress { get; set; }
        public NowAddress welshCourtAddress { get; set; }
    }

    public class NextHearingCourtDetails251
    {
        public string courtName { get; set; }
        public string hearingDate { get; set; }
        public string hearingTime { get; set; }
        public NowAddress courtAddress { get; set; }

        public NextHearingCourtDetails251(NextHearingCourtDetails nextHearingCourtDetails)
        {
            this.courtName = nextHearingCourtDetails.courtName;
            this.hearingDate = nextHearingCourtDetails.hearingDate;
            this.hearingTime = nextHearingCourtDetails.hearingTime;
            this.courtAddress = nextHearingCourtDetails.courtAddress;
        }
    }
}

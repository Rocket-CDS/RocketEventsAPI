using DNNrocketAPI.Components;
using RazorEngine.Text;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketEventsAPI.Components
{
    public class RocketEventsAPITokens<T> : RocketDirectoryAPI.Components.RocketDirectoryAPITokens<T>
    {
        public DateTime monthStartDate;
        public DateTime monthEndDate;
        public DateTime calMonthStartDate;
        public DateTime articleEventStartDate;
        public DateTime articleEventEndDate;
        public string[] listUrlParams;
        /// <summary>
        /// Assigns and prepares the data model for Razor event templates. It calls the base class's AssignDataModel, sets up date parameters for calendar views, and populates several event lists (next events, past events, monthly events, etc.) into the SimplisityRazor model.
        /// </summary>
        public new string AssignDataModel(SimplisityRazor sModel)
        {
            base.AssignDataModel(sModel);

            calMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var calYear = sModel.SessionParamsData.GetInt("calyear");
            if (calYear == 0) calYear = DateTime.Now.Year;
            var calMonth = sModel.SessionParamsData.GetInt("calmonth");
            if (calMonth == 0) calMonth = DateTime.Now.Month;
            if (calMonth > 0 && calYear > 0) calMonthStartDate = new DateTime(calYear, calMonth, 1, 0, 0, 0).Date;

            var yDate = sModel.SessionParamsData.GetInt("year");
            var mDate = sModel.SessionParamsData.GetInt("month");
            // Event List Params
            listUrlParams = new string[] { "month", mDate.ToString(), "year", yDate.ToString() };

            // Event List
            var eventListData = RocketEventsUtils.GetNextEvents(portalData.PortalId, sessionParams.CultureCode);
            sModel.SetDataObject("eventnextlist", eventListData);

            // Passed Event List
            var eventListDataP = RocketEventsUtils.GetPassedEvents(portalData.PortalId, sessionParams.CultureCode);
            sModel.SetDataObject("eventpassedlist", eventListDataP);

            // Events In Month
            var eventListData2 = RocketEventsUtils.GetMonthEvents(portalData.PortalId, sessionParams.CultureCode, yDate, mDate);
            sModel.SetDataObject("eventmonthlist", eventListData2);

            // Events By Month
            var eventListData3 = RocketEventsUtils.GetEventsByMonth(portalData.PortalId, sessionParams.CultureCode, DateTime.Now.AddMonths(-6), 12);
            sModel.SetDataObject("eventlistbymonth", eventListData3);

            // Events By Day in Month
            var eventListData4 = RocketEventsUtils.GetEventsByDay(portalData.PortalId, sessionParams.CultureCode, calYear, calMonth);
            sModel.SetDataObject("eventlistbyday", eventListData4);

            // ArticleData
            var articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
            if (articleData != null && articleData.Exists)
            {
                articleEventStartDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate");
                articleEventEndDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventenddate");
            }

            return "";
        }
        /// <summary>
        /// Generates a URL for an RSS feed of events for a specific month and year. The URL includes parameters for the command, month, year, and a SQL index for sorting by event start date.
        /// </summary>
        public IEncodedString RssEventUrl(int portalId, string cmd, int monthDate, int yearDate)
        {
            var portalData = new PortalLimpet(portalId);
            var rssurl = portalData.EngineUrlWithProtocol + "/Desktopmodules/dnnrocket/api/rocket/action?cmd=" + cmd + "&month=" + monthDate + "&year=" + yearDate + "&months=1&sqlidx=eventstartdate";
            return new RawString(rssurl);
        }


    }
}

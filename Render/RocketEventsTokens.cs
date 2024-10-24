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
        public new string AssignDataModel(SimplisityRazor sModel)
        {
            base.AssignDataModel(sModel);

            // Display Month (from URL)
            calMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var calYear = sModel.SessionParamsData.GetInt("calyear");
            if (calYear == 0) calYear = DateTime.Now.Year;
            var calMonth = sModel.SessionParamsData.GetInt("calmonth");
            if (calMonth == 0) calMonth = DateTime.Now.Month;
            if (calMonth > 0 && calYear > 0) calMonthStartDate = new DateTime(calYear, calMonth, 1, 0, 0, 0).Date;

            // if we have "Search" in the URL params use it as searchtext.
            if (sModel.SessionParamsData.Get("search") != "") sModel.SessionParamsData.Set("searchtext", sModel.SessionParamsData.Get("search"));
            var searchText = sModel.SessionParamsData.Get("searchtext");
            var yDate = sModel.SessionParamsData.GetInt("year");
            var mDate = sModel.SessionParamsData.GetInt("month");
            if (searchText == "")
            {
                if (mDate == 0)
                {
                    monthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-2).Month, 1, 0, 0, 0).Date;
                    monthEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 0, 0, 0).Date;
                }
                else
                {
                    monthStartDate = new DateTime(yDate, mDate, 1, 0, 0, 0).Date;
                    monthEndDate = new DateTime(yDate, mDate, DateTime.DaysInMonth(yDate, mDate), 0, 0, 0).Date;
                }
            }
            else
            {
                // do search on 3 years
                monthStartDate = DateTime.Now.AddYears(-2).Date;
                monthEndDate = DateTime.Now.AddYears(1).Date;
            }
            sModel.SessionParamsData.Set("searchdate1", monthStartDate.ToString("O"));
            sModel.SessionParamsData.Set("searchdate2", monthEndDate.ToString("O"));


            var articleDataList = new ArticleLimpetList(sModel.SessionParamsData, portalContent, sModel.SessionParamsData.CultureCode, true, false);
            sModel.SetDataObject("articlelist", articleDataList);

            // Event List Params
            listUrlParams = new string[] { "month", mDate.ToString(), "year", yDate.ToString() };

            // Event List
            var nextcount = moduleData.GetSettingInt("nextcount");
            var eventListData = RocketEventsUtils.GetNextEvents(portalData.PortalId, sessionParams.CultureCode, nextcount);
            sModel.SetDataObject("eventnextlist", eventListData);

            // Passed Event List
            var eventListDataP = RocketEventsUtils.GetPassedEvents(portalData.PortalId, sessionParams.CultureCode, nextcount);
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
        public IEncodedString RssEventUrl(int portalId, string cmd, int monthDate, int yearDate)
        {
            var portalData = new PortalLimpet(portalId);
            var rssurl = portalData.EngineUrlWithProtocol + "/Desktopmodules/dnnrocket/api/rocket/action?cmd=" + cmd + "&month=" + monthDate + "&year=" + yearDate + "&months=1&sqlidx=eventstartdate";
            return new RawString(rssurl);
        }


    }
}

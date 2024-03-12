using DNNrocketAPI.Components;
using RazorEngine.Text;
using Rocket.AppThemes.Components;
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
        public DateTime articleEventStartDate;
        public DateTime articleEventEndDate;
        public string[] listUrlParams;
        public new string AssigDataModel(SimplisityRazor sModel)
        {
            base.AssigDataModel(sModel);

            // Display Month (from URL)
            var yDate = sessionParams.GetInt("year");
            var mDate = sessionParams.GetInt("month");
            if (mDate == 0)
            {
                var numberOfDays = sessionParams.GetInt("days");
                if (numberOfDays == 0) numberOfDays = catalogSettings.Info.GetXmlPropertyInt("genxml/textbox/numberofdays");
                if (numberOfDays == 0) numberOfDays = 14;
                monthStartDate = DateTime.Now.Date;
                var daysToEndOfMonth = DateTime.DaysInMonth(monthStartDate.Year, monthStartDate.Month) - monthStartDate.Day;
                if (numberOfDays < daysToEndOfMonth) numberOfDays = daysToEndOfMonth; // always display to end of current month. 
                monthEndDate = DateTime.Now.AddDays(numberOfDays).Date;
            }
            else
            {
                monthStartDate = new DateTime(yDate, mDate, 1, 0, 0, 0).Date;
                monthEndDate = new DateTime(yDate, mDate, DateTime.DaysInMonth(yDate, mDate), 0, 0, 0).Date;
            }

            // Event List Params
            listUrlParams = new string[] { "month", mDate.ToString(), "year", yDate.ToString() };

            // Event List
            var eventListData = RocketEventsAPI.Components.RocketEventsUtils.GetEvents(portalData.PortalId, sessionParams.CultureCode, monthStartDate, monthEndDate, true, 100);
            sModel.SetDataObject("eventlistdata", eventListData);
            sessionParams.RowCount = eventListData.RowCount;

            // Event Article Date
            articleEventStartDate = DateTime.Now.Date;
            articleEventEndDate = DateTime.Now.Date;
            if (articleData != null)
            {
                articleEventStartDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate");
                articleEventEndDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventenddate");
                if (sessionParams.Get("eventdate") != "")
                {
                    var eventDays = (articleEventEndDate - articleEventStartDate).TotalDays;
                    articleEventStartDate = Convert.ToDateTime(sessionParams.Get("eventdate"));
                    articleEventEndDate = articleEventStartDate.AddDays(eventDays);
                }
            }

            // use return of "string", so we don;t get error with converting void to object.
            return "";
        }
        public IEncodedString RssEventUrl(int portalId, string cmd, int monthDate, int yearDate, int numberOfDays = 60)
        {
            if (numberOfDays == 0) numberOfDays = 7;
            var portalData = new PortalLimpet(portalId);
            var rssurl = portalData.EngineUrlWithProtocol + "/Desktopmodules/dnnrocket/api/rocket/action?cmd=" + cmd + "&month=" + monthDate + "&year=" + yearDate + "&days=" + numberOfDays;
            return new RawString(rssurl);
        }
    }
}

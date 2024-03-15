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
        public DateTime articleEventStartDate;
        public DateTime articleEventEndDate;
        public string[] listUrlParams;
        public new string AssigDataModel(SimplisityRazor sModel)
        {
            base.AssigDataModel(sModel);

            // Display Month (from URL)
            var yDate = sModel.SessionParamsData.GetInt("year");
            var mDate = sModel.SessionParamsData.GetInt("month");
            if (mDate == 0)
            {
                monthStartDate = DateTime.Now.AddMonths(-2).Date;
                monthEndDate = DateTime.Now.Date;
            }
            else
            {
                monthStartDate = new DateTime(yDate, mDate, 1, 0, 0, 0).Date;
                monthEndDate = new DateTime(yDate, mDate, DateTime.DaysInMonth(yDate, mDate), 0, 0, 0).Date;
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

            // Events In Month
            var eventListData2 = RocketEventsUtils.GetMonthEvents(portalData.PortalId, sessionParams.CultureCode, yDate, mDate);
            sModel.SetDataObject("eventmonthlist", eventListData2);

            // Events By Month
            var eventListData3 = RocketEventsUtils.GetEventsByMonth(portalData.PortalId, sessionParams.CultureCode, DateTime.Now.AddYears(-6), 12);
            sModel.SetDataObject("eventlistbymonth", eventListData3);

            // Events By Day in Month
            var eventListData4 = RocketEventsUtils.GetEventsByDay(portalData.PortalId, sessionParams.CultureCode, yDate, mDate);
            sModel.SetDataObject("eventlistbyday", eventListData4);
            

            return "";
        }
        public IEncodedString RssEventUrl(int portalId, string cmd, int monthDate, int yearDate, int numberOfDays = 60)
        {
            if (numberOfDays == 0) numberOfDays = 7;
            var portalData = new PortalLimpet(portalId);
            var rssurl = portalData.EngineUrlWithProtocol + "/Desktopmodules/dnnrocket/api/rocket/action?cmd=" + cmd + "&month=" + monthDate + "&year=" + yearDate + "&days=" + numberOfDays;
            return new RawString(rssurl);
        }
        /// <summary>
        /// Builds the List URL.
        /// </summary>
        /// <param name="listpageid">The listpageid.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns></returns>
        public IEncodedString ListUrl(int listpageid, string[] urlparams = null)
        {
            if (urlparams == null) urlparams = new string[] { };
            var listurl = DNNrocketUtils.NavigateURL(listpageid, urlparams);
            return new RawString(listurl);
        }
        /// <summary>
        /// Builds the Detail URL.
        /// </summary>
        /// <param name="detailpageid">The detailpageid.</param>
        /// <param name="title">The title.</param>
        /// <param name="eId">The row eId.</param>
        /// <returns></returns>
        public IEncodedString DetailUrl(int detailpageid, ArticleLimpet articleData, string[] urlparams = null)
        {
            if (urlparams == null) urlparams = new string[] { };
            var detailurl = "";
            var seotitle = DNNrocketUtils.UrlFriendly(articleData.Name);

            var articleParamKey = "";
            var paramidList = DNNrocketUtils.GetQueryKeys(articleData.PortalId);
            foreach (var paramDict in paramidList)
            {
                if (articleData.SystemKey == paramDict.Value.systemkey && paramDict.Value.datatype == "article")
                {
                    articleParamKey = paramDict.Value.queryparam;
                }
            }

            string[] urlparams2 = { articleParamKey, articleData.ArticleId.ToString(), seotitle };
            urlparams = urlparams.Concat(urlparams2).ToArray();
            detailurl = DNNrocketUtils.NavigateURL(detailpageid, articleData.CultureCode, urlparams);

            return new RawString(detailurl);
        }


    }
}

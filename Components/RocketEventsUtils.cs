using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEventsAPI.Components
{
    public static class RocketEventsUtils
    {
        /// <summary>
        /// Getthe recurring events, this does NOT include the first database event.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        private static List<ArticleLimpet> GetRecurringEvents(int portalId, string cultureCode, DateTime startDate, DateTime endDate)
        {
            var eventList = new List<ArticleLimpet>();

            var objCtrl = new DNNrocketController();
            var sqlCmd = "SELECT [ItemId] FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] ";
            sqlCmd += " where portalid = " + portalId + " and typecode = 'rocketeventsapiART' ";
            sqlCmd += " and [XMLData].value('(genxml/checkbox/recurringevent)[1]','bit') = 1 ";
            sqlCmd += " for xml raw";

            var xmlList = objCtrl.ExecSqlXmlList(sqlCmd);
            if (xmlList.Count > 0)
            {
                foreach (SimplisityRecord x in xmlList)
                {
                    var i = x.GetXmlPropertyInt("row/@ItemId");
                    var ev1 = new ArticleLimpet(portalId, Convert.ToInt32(i), cultureCode, "rocketeventsapi");
                    var untilDate = ev1.Info.GetXmlPropertyDate("genxml/textbox/untildate").Date;
                    var eventDate = ev1.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate");
                    var eventendDate = ev1.Info.GetXmlPropertyDate("genxml/textbox/eventenddate");
                    var eventType = ev1.Info.GetXmlProperty("genxml/select/eventtype");
                    var eventDays = (eventendDate - eventDate).TotalDays;
                    if (!"DWMY".Contains(eventType)) eventType = "Y";

                    if (eventDate < DateTime.Now.AddYears(-1))
                    {
                        // move the event date so we have a rolling event.  To prevent problems with the size of the array.
                        ev1.Info.SetXmlProperty("genxml/textbox/eventstartdate", eventDate.AddYears(1).ToString("O"), TypeCode.DateTime);
                        ev1.Info.SetXmlProperty("genxml/textbox/eventenddate", eventendDate.AddYears(1).ToString("O"), TypeCode.DateTime);
                        ev1.Update();
                    }

                    var eventLoopDate = eventDate;
                    if (eventLoopDate < DateTime.Now.AddYears(-1)) eventLoopDate = DateTime.Now.AddYears(-1); // add limit of 1 year history

                    var lp = 0; 
                    while (eventLoopDate <= untilDate)
                    {
                        if (eventType == "D") eventLoopDate = eventLoopDate.AddDays(1);
                        if (eventType == "W") eventLoopDate = eventLoopDate.AddDays(7);
                        if (eventType == "M") eventLoopDate = eventLoopDate.AddMonths(1);
                        if (eventType == "Y") eventLoopDate = eventLoopDate.AddYears(1);
                        if (eventLoopDate >= startDate && eventLoopDate <= endDate)
                        {
                            var evCloneInfo = (SimplisityInfo)ev1.Info.CloneInfo();
                            var ev = new ArticleLimpet(evCloneInfo);
                            ev.Info.SetXmlProperty("genxml/textbox/eventstartdate", eventLoopDate.ToString("O"), TypeCode.DateTime);
                            ev.Info.SetXmlProperty("genxml/textbox/eventenddate", eventLoopDate.AddDays(eventDays).ToString("O"), TypeCode.DateTime);
                            eventList.Add(ev);
                            lp += 1;
                            if (lp == 1000) break; // jump out after 1000 created.
                        }
                    }
                }
            }

            return eventList;
        }
        /// <summary>
        /// Get Events
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="startDate">Start Date Range</param>
        /// <param name="endDate">End Date Range</param>
        /// <param name="includeRecurring">Include recurring events (All)</param>
        /// <param name="limit">Limt Event return list</param>
        /// <param name="historyMonths">If the event is in the past, only return the last historyMonths.</param>
        /// <returns></returns>
        public static EventListData GetEvents(int portalId, string cultureCode, DateTime startDate, DateTime endDate, bool includeRecurring = true, int limit = 100, bool reverseOrderBy = false)
        {
            var systemKey = "rocketeventsapi";
            var cacheKey = "GetEvents" + systemKey + portalId + cultureCode + startDate.Date.ToString("O") + endDate.Date.ToString("O") + includeRecurring + limit + reverseOrderBy;
            var rtn = (EventListData)CacheUtils.GetCache(cacheKey, systemKey + portalId);
            if (rtn != null) return rtn;

            List<ArticleLimpet> eventList = new List<ArticleLimpet>();
            var objCtrl = new DNNrocketController();
            
            var sqlCmd = "SELECT TOP (" + limit + ") [ItemId] FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] ";
            sqlCmd += " where portalid = " + portalId + " and typecode = 'rocketeventsapiART' ";
            sqlCmd += " and [XMLData].value('(genxml/textbox/eventstartdate)[1]','datetime') >= CAST('" + startDate.Date.ToString("O") + "' as date) ";
            sqlCmd += " and [XMLData].value('(genxml/textbox/eventstartdate)[1]','datetime') <= CAST('" + endDate.Date.ToString("O") + "' as date) ";
            sqlCmd += " for xml raw";

            var xmlList = objCtrl.ExecSqlXmlList(sqlCmd);
            if (xmlList.Count > 0)
            {
                foreach (SimplisityRecord x in xmlList)
                {
                    var i = x.GetXmlPropertyInt("row/@ItemId");
                    eventList.Add(new ArticleLimpet(portalId, Convert.ToInt32(i), cultureCode, systemKey));
                }
            }

            if (includeRecurring)
            {
                var recurringList = GetRecurringEvents(portalId, cultureCode, startDate, endDate);
                eventList.AddRange(recurringList);
            }

            var records = from ev in eventList.OrderBy(o => o.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate")) select ev;
            if (reverseOrderBy)
                rtn = new EventListData(records.Reverse().ToList());
            else
                rtn = new EventListData(records.ToList());

            CacheUtils.SetCache(cacheKey, rtn, systemKey + portalId);
            return rtn;
        }
        public static Dictionary<DateTime, List<ArticleLimpet>> GetArticlesByMonth(int portalId, string cultureCode, DateTime startMonthDate, int numberOfMonths, bool useCache = true)
        {
            var systemKey = "rocketeventsapi";
            var cacheKey = "GetArticlesByMonth" + systemKey + portalId + cultureCode + startMonthDate.Date.ToString("O") + numberOfMonths;
            var rtn = (Dictionary<DateTime, List<ArticleLimpet>>)CacheUtils.GetCache(cacheKey, systemKey + portalId);
            if (rtn != null && useCache) return rtn;

            rtn = new Dictionary<DateTime, List<ArticleLimpet>>();
            DateTime rDateStart = new DateTime(startMonthDate.Year, startMonthDate.Month, 1, 0, 0, 0).Date;
            DateTime rDateEnd = new DateTime(startMonthDate.AddMonths(numberOfMonths).Year, startMonthDate.AddMonths(numberOfMonths).Month, DateTime.DaysInMonth(startMonthDate.AddMonths(numberOfMonths).Year, startMonthDate.AddMonths(numberOfMonths).Month), 0, 0, 0).Date;
            var eData = GetEvents(portalId, cultureCode, rDateStart, rDateEnd,true,1000);
            var lp = 0;
            while (lp < numberOfMonths)
            {
                var loopDate = startMonthDate.AddMonths(lp);
                DateTime mDate = new DateTime(loopDate.Year, loopDate.Month, 1, 0, 0, 0).Date;
                DateTime mDateEnd = new DateTime(loopDate.Year, loopDate.Month, DateTime.DaysInMonth(loopDate.Year, loopDate.Month) , 0, 0, 0).Date;
                rtn.Add(mDate, eData.GetEventsInMonth(loopDate.Year, loopDate.Month));
                lp += 1;
            }
            CacheUtils.SetCache(cacheKey, rtn, systemKey + portalId);
            return rtn;
        }

        public static bool IsEventON(ArticleLimpet articleData, DateTime checkDate)
        {
            var eventStartDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate").Date;
            var eventEndDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventenddate").Date;
            if (checkDate.Date >= eventStartDate && checkDate.Date <= eventEndDate) return true;
            return false;
        }

    }
}

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
using System.Xml.Linq;

namespace RocketEventsAPI.Components
{
    public static class RocketEventsUtils
    {
        public static void RemoveRecurringEvents(ArticleLimpet articleData)
        {
            var objCtrl = new DNNrocketController();
            var sqlCmd = "SELECT [ItemId] FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] ";
            sqlCmd += " where portalid = " + articleData.Info.PortalId + " and typecode = 'rocketeventsapiART' ";
            sqlCmd += " and ParentItemId = " + articleData.ArticleId + " ";
            sqlCmd += " for xml raw";

            var xmlList = objCtrl.ExecSqlXmlList(sqlCmd);
            foreach (SimplisityRecord x in xmlList)
            {
                var i = x.GetXmlPropertyInt("row/@ItemId");
                var ev1 = new ArticleLimpet(articleData.ArticleId, Convert.ToInt32(i), articleData.CultureCode, "rocketeventsapi");
                ev1.Delete();
            }
        }
        public static List<ArticleLimpet> GetBaseRecurringEvents(int portalId, string cultureCode)
        {
            var eventList = new List<ArticleLimpet>();

            var objCtrl = new DNNrocketController();
            var sqlCmd = "SELECT [ItemId] FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] ";
            sqlCmd += " where portalid = " + portalId + " and typecode = 'rocketeventsapiART' ";
            sqlCmd += " and XrefItemId = 1 "; // XrefItemId used as flag for recurring base event
            sqlCmd += " for xml raw";

            var xmlList = objCtrl.ExecSqlXmlList(sqlCmd);
            foreach (SimplisityRecord x in xmlList)
            {
                var i = x.GetXmlPropertyInt("row/@ItemId");
                var ev1 = new ArticleLimpet(portalId, Convert.ToInt32(i), cultureCode, "rocketeventsapi");
                if (ev1.Exists) eventList.Add(ev1);
            }
            return eventList;
        }
        public static List<ArticleLimpet> GetNextEvents(int portalId, string cultureCode, int nextCount = 3)
        {
            var eventList = new List<ArticleLimpet>();
            if (nextCount == 0) return eventList;
            var objCtrl = new DNNrocketController();
            var sqlFilter = " and eventstartdate.GUIDKey > '" + DateTime.Now.Date.ToString("O") + "' ";
            var orderBy = " order by eventstartdate.GUIDKey ";
            var l = objCtrl.GetList(portalId, -1, "rocketeventsapiART", sqlFilter, cultureCode, orderBy, nextCount,0,0,0, "RocketDirectoryAPI");
            foreach (SimplisityInfo sInfo in l)
            {
                var ev1 = new ArticleLimpet(sInfo);
                if (ev1.Exists) eventList.Add(ev1);
            }
            eventList.Reverse();
            return eventList;
        }
        public static List<ArticleLimpet> GetPassedEvents(int portalId, string cultureCode, int nextCount = 3)
        {
            var eventList = new List<ArticleLimpet>();
            if (nextCount == 0) return eventList;
            var objCtrl = new DNNrocketController();
            var sqlFilter = " and eventstartdate.GUIDKey <= '" + DateTime.Now.Date.ToString("O") + "' ";
            var orderBy = " order by eventstartdate.GUIDKey ";
            var l = objCtrl.GetList(portalId, -1, "rocketeventsapiART", sqlFilter, cultureCode, orderBy, nextCount, 0, 0, 0, "RocketDirectoryAPI");
            foreach (SimplisityInfo sInfo in l)
            {
                var ev1 = new ArticleLimpet(sInfo);
                if (ev1.Exists) eventList.Add(ev1);
            }
            eventList.Reverse();
            return eventList;
        }
        public static List<ArticleLimpet> GetMonthEvents(int portalId, string cultureCode, int yearInt, int monthInt)
        {

            var eventList = new List<ArticleLimpet>();

            if (yearInt > 0 && monthInt > 0)
            {
                var objCtrl = new DNNrocketController();

                DateTime rDateStart = new DateTime(yearInt, monthInt, 1, 0, 0, 0).Date.AddSeconds(-1);
                DateTime rDateEnd = new DateTime(yearInt, monthInt, DateTime.DaysInMonth(yearInt, monthInt), 0, 0, 0).Date;
                var sqlFilter = " and eventstartdate.GUIDKey > '" + rDateStart.ToString("O") + "' ";
                sqlFilter += " and eventstartdate.GUIDKey <= '" + rDateEnd.ToString("O") + "' ";
                var orderBy = " order by eventstartdate.GUIDKey ";
                var l = objCtrl.GetList(portalId, -1, "rocketeventsapiART", sqlFilter, cultureCode, orderBy, 0, 0, 0, 0, "RocketDirectoryAPI");
                foreach (SimplisityInfo sInfo in l)
                {
                    var ev1 = new ArticleLimpet(sInfo);
                    if (ev1.Exists) eventList.Add(ev1);
                }

            }
            return eventList;
        }
        public static Dictionary<DateTime, List<ArticleLimpet>> GetEventsByMonth(int portalId, string cultureCode, DateTime startMonthDate, int numberOfMonths)
        {
            var eventListRtn = new Dictionary<DateTime, List<ArticleLimpet>>();
            var lp = 0;
            while (lp < numberOfMonths)
            {
                var loopDate = startMonthDate.AddMonths(lp);
                DateTime mDate = new DateTime(loopDate.Year, loopDate.Month, 1, 0, 0, 0).Date;
                eventListRtn.Add(mDate, GetMonthEvents(portalId, cultureCode, loopDate.Year, loopDate.Month));
                lp += 1;
            }
            return eventListRtn;
        }
        public static Dictionary<int, List<ArticleLimpet>> GetEventsByDay(int portalId, string cultureCode, int yearInt, int monthInt)
        {
            var eventListRtn = new Dictionary<int, List<ArticleLimpet>>();
            if (yearInt > 0 && monthInt > 0)
            {
                var monthList = GetMonthEvents(portalId, cultureCode, yearInt, monthInt);
                var lp = 1;
                while (lp <= DateTime.DaysInMonth(yearInt, monthInt))
                {
                    DateTime dayDate = new DateTime(yearInt, monthInt, lp, 0, 0, 0).Date;
                    var dayList = new List<ArticleLimpet>();
                    foreach (var ev in monthList)
                    {
                        if (IsEventON(ev, dayDate)) dayList.Add(ev);
                    }
                    if (dayList.Count > 0) eventListRtn.Add(lp, dayList);
                    lp += 1;
                }
            }
            return eventListRtn;
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

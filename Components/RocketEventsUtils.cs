using DNNrocketAPI;
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
        
        public static List<ArticleLimpet> GetRecurringEvents(int portalId, string cultureCode)
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
                    var untilDate = ev1.Info.GetXmlPropertyDate("genxml/textbox/untildate");
                    var eventDate = ev1.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate");

                    // Only create history records for 3 months.
                    DateTime eventLoopDate = new DateTime(DateTime.Now.Year, eventDate.Month, eventDate.Day, 0, 0, 0).AddMonths(-3);
                    if (eventDate > eventLoopDate) eventLoopDate = eventDate;

                    var lp = 0; // jump out after 100 created.
                    while (eventLoopDate <= untilDate)
                    {
                        var evCloneInfo = (SimplisityInfo)ev1.Info.CloneInfo();
                        var ev = new ArticleLimpet(evCloneInfo);

                        if (ev.Info.GetXmlProperty("genxml/select/eventtype") == "D") eventLoopDate = eventLoopDate.AddDays(1);
                        if (ev.Info.GetXmlProperty("genxml/select/eventtype") == "W") eventLoopDate = eventLoopDate.AddDays(7);
                        if (ev.Info.GetXmlProperty("genxml/select/eventtype") == "M") eventLoopDate = eventLoopDate.AddMonths(1);
                        if (ev.Info.GetXmlProperty("genxml/select/eventtype") == "Y") eventLoopDate = eventLoopDate.AddYears(1);
                        ev.Info.SetXmlProperty("genxml/textbox/eventstartdate", eventLoopDate.ToString("O"), TypeCode.DateTime);
                        eventList.Add(ev);
                        lp += 1;
                        if (lp == 100) break;
                    }
                }
            }

            return eventList;
        }
        public static List<ArticleLimpet> GetEvents(int portalId, string cultureCode, DateTime startDate, DateTime endDate, int page = 0, int pagesize = 6, bool includeRecurring = true, int limit = 100)
        {
            List<ArticleLimpet> eventList = new List<ArticleLimpet>();
            var objCtrl = new DNNrocketController();
            
            var sqlCmd = "SELECT TOP (" + limit + ") [ItemId] FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] ";
            sqlCmd += " where portalid = " + portalId + " and typecode = 'rocketeventsapiART' ";
            sqlCmd += " and [XMLData].value('(genxml/textbox/eventstartdate)[1]','datetime') <= CAST('" + startDate.Date.ToString("O") + "' as date) ";
            sqlCmd += " and [XMLData].value('(genxml/textbox/eventstartdate)[1]','datetime') >= CAST('" + endDate.Date.ToString("O") + "' as date) ";
            sqlCmd += " for xml raw";

            var xmlList = objCtrl.ExecSqlXmlList(sqlCmd);
            if (xmlList.Count > 0)
            {
                foreach (SimplisityRecord x in xmlList)
                {
                    var i = x.GetXmlPropertyInt("row/@ItemId");
                    eventList.Add(new ArticleLimpet(portalId, Convert.ToInt32(i), cultureCode, "rocketeventsapi"));
                }
            }

            var recurringList = GetRecurringEvents(portalId, cultureCode);
            eventList.AddRange(recurringList);

            var records = from ev in eventList.OrderBy(o => o.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate") ) select ev;
            if (page > 0)
            {
                records = records.Skip((page - 1) * pagesize).Take(pagesize);
            }
            return records.ToList();
        }

    }
}

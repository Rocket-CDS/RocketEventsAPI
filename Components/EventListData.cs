using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketEventsAPI.Components
{
    public class EventListData
    {
        public EventListData(List<ArticleLimpet> eventList) {
            EventList = eventList;
        }
        public List<ArticleLimpet> GetEvents(int page, int pagesize)
        {
            var eventList = new List<ArticleLimpet>();
            if (page > 0)
            {
                eventList = EventList.Skip((page - 1) * pagesize).Take(pagesize).ToList();
            }
            return eventList;
        }
        public List<ArticleLimpet> GetEventsInMonth(int inYear, int inMonth)
        {
            var eventList = new List<ArticleLimpet>();
            if (inYear > 0 && inMonth > 0)
            {
                foreach (var ev in EventList)
                {
                    var eventStartDate = ev.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate").Date;
                    var eventEndDate = ev.Info.GetXmlPropertyDate("genxml/textbox/eventenddate").Date;
                    DateTime checkDate = new DateTime(inYear, inMonth, 1, 0, 0, 0).Date;
                    DateTime startDate = new DateTime(eventStartDate.Year, eventStartDate.Month, 1, 0, 0, 0).Date;
                    DateTime endDate = new DateTime(eventEndDate.Year, eventEndDate.Month, DateTime.DaysInMonth(eventEndDate.Year, eventEndDate.Month), 0, 0, 0).Date;
                    if (checkDate >= startDate && checkDate <= endDate) eventList.Add(ev);
                }
            }
            return eventList;
        }
        public Dictionary<int, List<ArticleLimpet>> GetEventsDayListInMonth(int inYear, int inMonth)
        {
            var articleDay = new Dictionary<int, List<ArticleLimpet>>();
            var articleList = GetEventsInMonth(inYear, inMonth);
            foreach (var a in articleList)
            {
                for (int d = 1; d <= DateTime.DaysInMonth(inYear, inMonth); d++)
                {
                    DateTime checkDate = new DateTime(inYear, inMonth, d, 0, 0, 0).Date;
                    if (RocketEventsUtils.IsEventON(a, checkDate))
                    {
                        if (articleDay.ContainsKey(d))
                        {
                            var l = articleDay[d];
                            l.Add(a);
                            articleDay.Remove(d);
                            articleDay.Add(d, l);
                        }
                        else
                        {
                            var l = new List<ArticleLimpet>();
                            l.Add(a);
                            articleDay.Add(d, l);
                        }
                    }
                }

            }
            return articleDay;
        }
        public List<ArticleLimpet> EventList { get; set; }
        public int RowCount { get { return EventList.Count;  } }

    }
}

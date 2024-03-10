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
        public List<ArticleLimpet> EventList { get; set; }
        public int RowCount { get { return EventList.Count;  } }

    }
}

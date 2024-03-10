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
        public new string AssigDataModel(SimplisityRazor sModel)
        {
            base.AssigDataModel(sModel);

            var eventListData = RocketEventsAPI.Components.RocketEventsUtils.GetEvents(portalData.PortalId, sessionParams.CultureCode, DateTime.Now.AddMonths(2), DateTime.Now.AddMonths(-2), sessionParams.Page, sessionParams.PageSize, true, 100);
            sModel.SetDataObject("eventlistdata", eventListData);
            sessionParams.RowCount = eventListData.RowCount;

            // use return of "string", so we don;t get error with converting void to object.
            return "";
        }
    }
}

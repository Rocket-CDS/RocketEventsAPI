using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEventsAPI.Components
{
    public class Events : IEventAction
    {
        public Dictionary<string, object> AfterEvent(string paramCmd, SystemLimpet systemData, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var rtn = new Dictionary<string, object>();
            if (paramCmd == "articleadmin_savedata")
            {
                var articleId = paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
                if (articleId > 0)
                {
                    var portalid = paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                    if (portalid == 0) portalid = PortalUtils.GetCurrentPortalId();
                    var cultureCode = DNNrocketUtils.GetCurrentCulture();
                    var articleData = new ArticleLimpet(portalid, articleId, cultureCode, systemData.SystemKey);

                    // Delete any existing recurring children in DB
                    RocketEventsUtils.RemoveRecurringEvents(articleData);

                    if (articleData.Exists && articleData.Info.GetXmlPropertyBool("genxml/checkbox/recurringevent"))
                    {
                        var untilDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/untildate").Date;
                        var eventDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventstartdate");
                        var eventendDate = articleData.Info.GetXmlPropertyDate("genxml/textbox/eventenddate");
                        var eventType = articleData.Info.GetXmlProperty("genxml/select/eventtype");
                        if (eventType == "") eventType = "M";
                        var recurringevery = articleData.Info.GetXmlPropertyInt("genxml/textbox/recurringevery");
                        var eventDays = (eventendDate - eventDate).TotalDays;

                        var eventLoopDate = eventDate;
                        if (eventType == "W") eventLoopDate = eventLoopDate.AddDays(recurringevery * 7);
                        if (eventType == "M") eventLoopDate = eventLoopDate.AddMonths(recurringevery);

                        var lp = 0;
                        while (eventLoopDate <= untilDate)
                        {
                            var evCloneInfo = (SimplisityInfo)articleData.Info.CloneInfo();
                            var ev = new ArticleLimpet(evCloneInfo);
                            ev.Info.ItemID = -1;
                            ev.Info.ModuleId = -1;
                            ev.Info.XrefItemId = 0;                            
                            ev.ParentItemId = articleData.ArticleId;
                            ev.Info.SetXmlProperty("genxml/textbox/eventstartdate", eventLoopDate.ToString("O"), TypeCode.DateTime);
                            ev.Info.SetXmlProperty("genxml/textbox/eventenddate", eventLoopDate.AddDays(eventDays).ToString("O"), TypeCode.DateTime);
                            ev.Update();

                            if (eventType == "W") eventLoopDate = eventLoopDate.AddDays(recurringevery * 7);
                            if (eventType == "M") eventLoopDate = eventLoopDate.AddMonths(recurringevery);

                            lp += 1;
                            if (lp == 200) break; // jump out after 200 created.
                        }
                        articleData.Info.SetXmlProperty("genxml/textbox/untildate", eventLoopDate.ToString("O"), TypeCode.DateTime);
                        articleData.XrefItemId = 1; // flag for base recurring
                        articleData.Update();
                    } 
                    else
                    {
                        articleData.ModuleId = -1; // flag for base recurring
                        articleData.Update();
                    }
                }
            }
            return rtn;
        }

        public Dictionary<string, object> BeforeEvent(string paramCmd, SystemLimpet systemData, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            return new Dictionary<string, object>();
        }
    }
}

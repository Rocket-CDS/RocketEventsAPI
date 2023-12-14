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
                    if (articleData.Exists)
                    {
                        var catalogSettings = new CatalogSettingsLimpet(portalid, cultureCode, systemData.SystemKey);
                        if (catalogSettings.Info.GetXmlPropertyBool("genxml/checkbox/hidefuturedates"))
                        {
                            // Hide any future articles.  (The scheudler will display them on the correct date)
                            if (articleData.Info.GetXmlPropertyDate("genxml/textbox/publisheddate").Date > DateTime.Now.Date)
                            {
                                if (articleData.Info.GetXmlPropertyBool("genxml/checkbox/autopublish"))
                                {
                                    articleData.Info.SetXmlProperty("genxml/checkbox/hidden", "true");
                                    articleData.Update();

                                    var sessionParams = new SessionParams(paramInfo);
                                    var strOut = "";
                                    var dataObject = new DataObjectLimpet(portalid, sessionParams.ModuleRef, sessionParams, systemData.SystemKey);

                                    dataObject.SetDataObject("articledata", articleData);

                                    var razorTempl = dataObject.AppTheme.GetTemplate("admindetail.cshtml");
                                    var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, dataObject.DataObjects, dataObject.Settings, sessionParams, true);
                                    if (pr.ErrorMsg != "")
                                        strOut = pr.ErrorMsg;
                                    else
                                        strOut = pr.RenderedText;
                                    rtn.Add("outputhtml", strOut);
                                }
                            }
                        }
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

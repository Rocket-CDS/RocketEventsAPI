using DNNrocketAPI.Components;
using RocketEventsAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using RocketDirectoryAPI;
using RocketDirectoryAPI.Components;
using DNNrocketAPI.Interfaces;

namespace RocketEventsAPI.API
{
    public partial class StartConnect : IProcessCommand
    {
        public Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            paramInfo.SetXmlProperty("genxml/settings/sqladminfilter", " and [R1].ParentItemId <= 0 "); // special filter for admin (must be in "genxml/settings/")

            paramCmd = paramCmd.Replace("rocketeventsapi_", "rocketdirectoryapi_");
            systemInfo.SetXmlProperty("genxml/systemkey", "rocketeventsapi");
            var catalogStartConnect = new RocketDirectoryAPI.API.StartConnect();
            return catalogStartConnect.ProcessCommand(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);
        }
    }

}

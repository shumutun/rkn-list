using Microsoft.AspNetCore.Mvc;
using RknList.Processing.RnkListObserve;
using System;
using System.Text;

namespace RknList.WebApi
{
    public class RknListController : Controller
    {
        private readonly RnkListObserver _rnkListObserver;

        public RknListController(RnkListObserver rnkListObserver)
        {
            _rnkListObserver = rnkListObserver;
        }

        public IActionResult Get()
        {
            var curList = _rnkListObserver.GetCurrentIPAddressList();
            var resBuilder = new StringBuilder();
            resBuilder.AppendLine("/ip firewall address-list");
            if (curList.Count > 0)
            {
                resBuilder.AppendLine("remove [/ip firewall address-list find list=rkn]");
                foreach (var item in curList)
                    resBuilder.AppendLine($"add list=rkn address={item}");
            }
            resBuilder.AppendLine($"/log info {DateTime.UtcNow.ToShortDateString()} (UTC)");
            return Content(resBuilder.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}

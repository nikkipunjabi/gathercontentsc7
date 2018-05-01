namespace GatherContent.Connector.WebControllers.Areas.Gc.Controllers
{
    using System;
    using System.Net;

    using IoC;

    using Managers.Interfaces;

    using Newtonsoft.Json;

    using Sitecore.Diagnostics;

    public class DropTreeController : BaseController
    {
        protected IDropTreeManager DropTreeManager;

        public DropTreeController()
        {
            DropTreeManager = ServiceFactory.DropTreeManager;
        }

        public string GetTopLevelNode(string id)
        {
            try
            {
                var result = DropTreeManager.GetTopLevelNode(id);
                var model = JsonConvert.SerializeObject(result);
                return model;
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return exception.Message + " Please check your credentials";
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return exception.Message;
            }

        }

        public string GetChildren(string id)
        {
            try
            {
                var result = DropTreeManager.GetChildrenNodes(id);
                var model = JsonConvert.SerializeObject(result);
                return model;
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return exception.Message + " Please check your credentials";
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return exception.Message;
            }
        }
    }
}

using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.WebControllers.IoC;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Sitecore.Diagnostics;

namespace GatherContent.Connector.Website.Services
{
    /// <summary>
    /// Summary description for DropTree
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DropTree : System.Web.Services.WebService
    {

        protected IDropTreeManager DropTreeManager;
       

        public DropTree()
        {
            DropTreeManager = ServiceFactory.DropTreeManager;
        }

        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void GetTopLevelNode(string id)
        {
            object resultObject = null;
            try
            {
                resultObject  = DropTreeManager.GetTopLevelNode(id);
                //var model = JsonConvert.SerializeObject(result);
                //resultObject = model;
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
                //return exception.Message + " Please check your credentials";
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = "GatherContent message: " + exception.Message };
                //return exception.Message;
            }
            finally
            {
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(resultObject));
            }

        }


        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void GetChildren(string id)
        {
            object resultObject = null;
            try
            {
                resultObject = DropTreeManager.GetChildrenNodes(id);
                //var model = JsonConvert.SerializeObject(result);
                //resultObject = model;
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
               //return exception.Message + " Please check your credentials";
           }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = "GatherContent message: " + exception.Message };

                //return exception.Message;
            }
            finally
            {
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(resultObject));
            }
        }
    }
}

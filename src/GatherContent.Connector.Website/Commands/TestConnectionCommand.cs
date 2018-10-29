using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using GatherContent.Connector.Managers.Managers;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace GatherContent.Connector.Website.Commands
{
    public class TestConnectionCommand : Command
    {
        public override CommandState QueryState(CommandContext context)
        {
            var item = context.Items[0];

            return item.TemplateID == new ID("F8B571EE-59A4-4EA5-9B32-9C57720F87ED") ? base.QueryState(context) : CommandState.Hidden;
        }

        public override void Execute(CommandContext context)
        {
            try
            {
                Assert.ArgumentNotNull(context, "context");
                if (context.Items.Length != 1)
                {
                    return;
                }
                var item = context.Items[0];
                var parameters = new NameValueCollection
                                     {
                                         { "id", item.ID.ToString() }, 
                                         { "language", item.Language.ToString() }, 
                                         { "version", item.Version.ToString() }, 
                                         { "load", StringUtil.GetString(new[] { context.Parameters["load"] }) }, 
                                     };

                Context.ClientPage.Start(this, "Run", parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, typeof(MappingsCommand));
                SheerResponse.Alert("Sync failed. See log for details.");
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (!SheerResponse.CheckModified())
            {
                return;
            }
            
            //var testConnectionManager = new TestConnectionManager();
            //bool testConnection = testConnectionManager.TestConnection();

            //don't want to add a bunch of references and IoC-related code here, lets test whole controller :-/
            string testUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/services/mappings.asmx/Try";
            HttpWebRequest webrequest = WebRequest.Create(testUrl) as HttpWebRequest;
            HttpWebResponse response = webrequest.GetResponse() as HttpWebResponse;
            bool success = response.StatusCode == HttpStatusCode.OK;

            var uri = "/sitecore modules/shell/gathercontent/testconnection/testconnection.html";

            var path = string.Format("{0}?success={1}", uri, success);

            Context.ClientPage.ClientResponse.Broadcast(Context.ClientPage.ClientResponse.ShowModalDialog(path, "250", "100", "Test Connection", false), "Shell");

        }
    }
}
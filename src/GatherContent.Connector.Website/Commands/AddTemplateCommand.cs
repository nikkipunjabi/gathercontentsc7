using System;
using System.Collections.Specialized;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace GatherContent.Connector.Website.Commands
{
    public class AddTemplateCommand : Command
    {
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
                Log.Error(ex.Message, ex, typeof(AddTemplateCommand));
                SheerResponse.Alert("Sync failed. See log for details.");
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var id = args.Parameters["id"].Replace("{", "").Replace("}", "");
            var language = Language.Parse(args.Parameters["language"]);
            var version = args.Parameters["version"];
            var uri = "/sitecore modules/shell/gathercontent/AddTemplate/AddTemplate.html";
            var path = string.Format("{0}&id={1}&l={2}&v={3}", uri, id, language, version);

#if SC72
            Context.ClientPage.ClientResponse.Broadcast(Context.ClientPage.ClientResponse.ShowModalDialog(path, "800", "495","Add template", true), "Shell");
#else
            var options = new ModalDialogOptions(path)
            {
                Width = "800",
                Height = "495",
                MinWidth = "600",
                MinHeight = "495",
                Maximizable = false,
                Header = "Add template"
            };

            Context.ClientPage.ClientResponse.Broadcast(Context.ClientPage.ClientResponse.ShowModalDialog(options), "Shell");
#endif
        }
    }
}
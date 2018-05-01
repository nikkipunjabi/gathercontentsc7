using System;
using System.Collections.Specialized;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace GatherContent.Connector.Website.Commands
{
    public class UpdateCommand : Command
    {
        public override CommandState QueryState(CommandContext context)
        {
            var item = context.Items[0];

            return item.Paths.FullPath.ToLower().StartsWith("/sitecore/content") ? base.QueryState(context) : CommandState.Hidden;
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
                                         { "database", item.Database.Name }, 
                                         { "load", StringUtil.GetString(new[] { context.Parameters["load"] }) }, 
                                     };

                Context.ClientPage.Start(this, "Run", parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, typeof(ImportCommand));
                SheerResponse.Alert("Sync failed. See log for details.");
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var id = args.Parameters["id"].Replace("{", "").Replace("}", "");
            var db = args.Parameters["database"];
            var language = Language.Parse(args.Parameters["language"]);
            var version = args.Parameters["version"];
            var uri = "/sitecore modules/shell/gathercontent/update/update.html";
            var path = string.Format("{0}?id={1}&l={2}&v={3}&db={4}", uri, id, language, version, db);

#if SC72
            Context.ClientPage.ClientResponse.Broadcast(Context.ClientPage.ClientResponse.ShowModalDialog(path, "1600", "700", "Pull Updated Content from GatherContent", false), "Shell");
#else
            var options = new ModalDialogOptions(path)
            {
                Width = "1600",
                Height = "700",
                MinWidth = "881",
                MinHeight = "400",
                Maximizable = false,
                Header = "Pull Updated Content from GatherContent"
            };

            Context.ClientPage.ClientResponse.Broadcast(Context.ClientPage.ClientResponse.ShowModalDialog(options), "Shell");
#endif
        }
    }
}

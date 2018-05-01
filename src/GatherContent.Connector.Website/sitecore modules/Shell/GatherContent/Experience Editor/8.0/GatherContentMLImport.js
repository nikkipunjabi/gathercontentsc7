define(["sitecore"], function(sitecore) {
    sitecore.Commands.GCMLImportCommand =
    {
        canExecute: function(context) {
            return true;
        },
        execute: function(context) {
            var itemId = context.currentContext.itemId.replace("%7b", "").replace("%7d", "");
            var dialogPath = "/sitecore modules/shell/gathercontent/import/MultiLocationImport.html?id=" + itemId + "&l=" + context.currentContext.language + "&v=" + context.currentContext.version + "&db=" + context.currentContext.database;
            var dialogFeatures = "dialogHeight: 700px;dialogWidth: 1200px;";
            sitecore.ExperienceEditor.Dialogs.showModalDialog(dialogPath, '', dialogFeatures, null, function(result) {
                if (!result) {
                    return;
                }
            });
        }
    };
});
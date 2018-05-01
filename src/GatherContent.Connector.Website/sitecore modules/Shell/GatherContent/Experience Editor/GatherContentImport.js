define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function(sitecore, experienceEditor) {
    sitecore.Commands.GCImportCommand =
    {
        canExecute: function(context) {
            return true;
        },
        execute: function(context) {
            var itemId = context.currentContext.itemId.replace("%7b", "").replace("%7d", "");
            var dialogPath = "/sitecore modules/shell/gathercontent/import/import.html?id=" + itemId + "&l=" + context.currentContext.language + "&v=" + context.currentContext.version + "&db=" + context.currentContext.database;
            var dialogFeatures = "dialogHeight: 700px;dialogWidth: 1200px;";
            experienceEditor.Dialogs.showModalDialog(dialogPath, '', dialogFeatures, null, function(result) {
                if (!result) {
                    return;
                }
            });
        }
    };
});
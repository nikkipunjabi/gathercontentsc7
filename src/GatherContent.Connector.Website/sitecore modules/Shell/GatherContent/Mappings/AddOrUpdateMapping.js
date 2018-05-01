var gcTemplateId = getUrlVars()["id"];
if (gcTemplateId == undefined) {
    gcTemplateId = "";
}
var scMappingId = getUrlVars()["scMappingId"];
if (scMappingId == undefined) {
    scMappingId = "";
}
var url = '/services/mappings.asmx/GetMapping?gcTemplateId=' + gcTemplateId + "&scMappingId=" + scMappingId;


function Init() {
    var resultData;

    jQuery.ajax({
        url: url,
        dataType: 'json',
        async: false,
        success: function (data) {
            resultData = data;
            resultData.IsError = false;
            resultData.IsShowing = false;
            jQuery(".preloader").hide();
        }
    });

    return resultData;
}





function ViewModel(data) {
    var self = this;

    this.NotValid = ko.observable(false);
    this.NotValidGcProject = ko.observable(false);
    this.NotValidGcTemplate = ko.observable(false);
    this.NotValidScTemplate = ko.observable(false);

    this.validate = function () {
        var isValid = true;
        if (self.GcMappingTitle() == null || self.GcMappingTitle() == "") {
            self.NotValid(true);
            isValid = false;
        }
        if (self.SelectedGcProject().Id == "0") {
            self.NotValidGcProject(true);
            isValid = false;
        }
        if (self.SelectedTemplateId() == "0") {
            self.NotValidScTemplate(true);
            isValid = false;
        }
        if (self.SelectedGcTemplate().Id == "0") {
            self.NotValidGcTemplate(true);
            isValid = false;
        }

        return isValid;
    };

    //Methods
    this.saveMapping = function () {

        if (self.validate()) {

            var model = new function () {
                this.TemplateTabs = self.Tabs();
                this.IsEdit = self.IsEdit();
                this.SelectedTemplateId = self.SelectedTemplateId();
                this.TemplateId = self.SelectedGcTemplate().Id;
                this.GcMappingTitle = self.GcMappingTitle();
                this.ScMappingId = self.ScMappingId();
                this.DefaultLocation = self.DefaultLocation();
            }

            jQuery.ajax({
                url: '/services/mappings.asmx/Post',
                type: 'post',
                data: '{model : ' + JSON.stringify(model) + '}',
                contentType: 'application/json',
                success: function (data) {
                    if (data.status != "error") {
                        window.opener.location.reload(true);
                        window.top.dialogClose();
                    } else {
                        self.ErrorText("Error:" + " " + data.message);
                        self.IsError(true);
                    }
                },

            });

        } else {
            //self.ValidationMessage("Mapping Title is mandatory  field");
            //self.NotValid(true);
        }
    };

    this.closeDropTree = function (model, e) {
        if (e.target.tagName === "INPUT") {
            return;
        }

        var id = this.OpenerId();
        jQuery("#" + id).hide();
        this.IsShowing(false);
    };

    this.openDropTree = function () {
        var id = this.OpenerId();
        var locationId = this.DefaultLocation();

        var t = this;

        if (!this.IsShowing()) {
            //TODO use Knockout
            jQuery("#" + id).show();
            this.IsShowing(true);
            var mapping = this;
            jQuery("#" + id).dynatree({
                autoFocus: false,
                imagePath: "~/icon/",
                initAjax: {
                    url: '/services/DropTree.asmx/GetTopLevelNode?id=' + locationId,
                    data: { mode: "funnyMode" }
                },
                onActivate: function (node) {
                    var path = "";
                    var keys = node.getKeyPath().split("/");
                    keys.shift();

                    for (var i = 0; i < keys.length; i++) {
                        if (i != keys.length - 1) {
                            path += node.tree.getNodeByKey(keys[i]).data.title + " / ";
                        } else {
                            path += node.tree.getNodeByKey(keys[i]).data.title;
                        }
                    }

                    jQuery("#" + id).hide();
                    t.IsShowing(false);

                    jQuery('[data-openerid="' + id + '"]').val(path);
                    mapping.DefaultLocation(node.data.key);
                    mapping.DefaultLocationText(node.data.title);
                },
                onLazyRead: function (node) {
                    node.appendAjax({
                        url: "/services/DropTree.asmx/GetChildren?id=" + node.data.key,
                        data: {
                            key: node.data.key,
                            mode: "funnyMode"
                        }
                    });
                }
            });
        }
        else {
            //TODO use Knockout
            jQuery("#" + id).hide();
            this.IsShowing(false);
        }
    }

    this.scTemplateChanged = function () {
        this.SitecoreFields(self.SelectedScTemplate().SitecoreFields);
        this.SelectedTemplateId(self.SelectedScTemplate().SitrecoreTemplateId);
    }

    this.gcProjectChanged = function () {

        if (self.SelectedGcProject != undefined && self.SelectedGcProject() != null) {
            jQuery.ajax({
                url: '/services/mappings.asmx/GetTemplByProjectId?gcProjectId=' + self.SelectedGcProject().Id,
                dataType: 'json',
                async: false,
                success: function (data) {
                    if (data.status != "error") {
                        self.GcTemplates(data);
                        if (self.IsEdit()) {
                            self.SelectedGcTemplate(self.find("Id", data, self.GcTemplateId()));
                        } else {
                            self.SelectedGcTemplate(data[0]);
                        }

                        self.gcTemplateChanged();
                    } else {
                        self.ErrorText("Error:" + " " + data.message);
                        self.IsError(true);
                    }
                }
            });
        }
    }

    this.gcTemplateChanged = function () {
        if (self.SelectedGcTemplate != undefined && self.SelectedGcTemplate() != null) {
            jQuery.ajax({
                url: '/services/mappings.asmx/GetFieldsByTemplateId?gcTemplateId=' + self.SelectedGcTemplate().Id,
                dataType: 'json',
                async: false,
                success: function (data) {
                    if (data.status != "error") {
                        for (var t = 0; t < data.length; t++) {
                            for (var i = 0; i < data[t].Fields.length; i++) {
                                var currentElement = data[t].Fields[i];
                                var selectedItem = self.find("GcFieldId", self.SelectedFields(), data[t].Fields[i].FieldId);
                                if (selectedItem != null) {
                                    currentElement.SelectedScField = selectedItem.SitecoreTemplateId;
                                }
                            }
                        }
                        self.Tabs(data);
                        tabSlideFirst();
                    } else {
                        self.ErrorText("Error:" + " " + data.message);
                        self.IsError(true);
                    }
                }
            });
        }
    }

    this.GetCmsFieldsAdditional = function (item) {
        var items = self.SelectedFields();
        for (var i = 0; i < items.length; i++) {
            if (items[i].GcFieldId == item.FieldId) {
                return items[i].CmsTemplateNamesAdditional;
            }
        }
        return [];
    };

    this.GetCmsFieldMappingItemUrl = function (item) {
        var items = self.SelectedFields();
        for (var i = 0; i < items.length; i++) {
            if (items[i].GcFieldId == item.FieldId) {
                return items[i].CmsFieldMappingItemUrl;
            }
        }
        return null;
    };

    this.GetCurrentFields = function (item) {
        var fieldType = item.FieldType;
        var allowedFields = self.Rules()[fieldType];
        if (typeof allowedFields === 'undefined') {
            return self.SelectedScTemplate().SitecoreFields[0];
        } else {
            if (allowedFields != null) {
                var allowedFieldsArr = allowedFields.split(",");
                var currentCollection = self.SelectedScTemplate().SitecoreFields;
                var resultCollection = [];
                resultCollection.push(self.SelectedScTemplate().SitecoreFields[0]);
                for (var i = 0; i < currentCollection.length; i++) {
                    var currentElement = currentCollection[i];
                    var selectedItem = self.find("GcFieldId", self.SelectedFields(), item.FieldId);
                    if (selectedItem != null) {
                        if (currentElement.SitecoreFieldId == selectedItem.SitecoreTemplateId) {
                            currentElement.Selected = true;
                        }
                    }
                    for (var f = 0; f < allowedFieldsArr.length; f++) {
                        var field = allowedFieldsArr[f];
                        if (currentElement.SitecoreFieldType == field.trim()) {
                            resultCollection.push(currentElement);
                        }
                    }
                }
                return resultCollection;
            }
        }
        return self.SelectedScTemplate().SitecoreFields[0];
    };

    this.returnFieldName = function (item) {
        if (item.FieldName === null) {
            return "[Guideline]";
        } else {
            return item.FieldName;
        }
    };

    this.find = function (prop, array, property) {
        return ko.utils.arrayFirst(array, function (item) {
            return item[prop] === property;
        });
    };




    //Fields
    this.Rules = ko.observable(data.Rules);
    this.IsEdit = ko.observable(data.IsEdit);
    this.ScMappingId = ko.observable(data.ScMappingId);
    this.GcMappingTitle = ko.observable(data.AddMappingModel.GcMappingTitle);
    this.OpenerId = ko.observable(data.AddMappingModel.OpenerId);
    this.DefaultLocation = ko.observable(data.AddMappingModel.DefaultLocation);
    this.DefaultLocationTitle = ko.observable(data.AddMappingModel.DefaultLocationTitle);
    this.IsShowing = ko.observable(data.IsShowing);
    this.GcProjectId = ko.observable(data.AddMappingModel.GcProjectId);
    this.GcTemplateId = ko.observable(data.AddMappingModel.GcTemplateId);
    this.SelectedTemplateId = ko.observable();
    this.SitecoreTemplates = ko.observableArray(data.SitecoreTemplates);
    this.SelectedFields = ko.observableArray(data.SelectedFields);
    this.GcProjects = ko.observableArray(data.GcProjects);
    this.GcTemplates = ko.observableArray();
    this.SitecoreFields = ko.observableArray();
    this.Tabs = ko.observableArray();

    this.ErrorText = ko.observable();
    this.IsError = ko.observable(data.IsError);
    this.ValidationMessage = ko.observable();

    if (data.IsEdit) {
        this.SelectedGcProject = ko.observable(self.find("Id", data.GcProjects, data.GcProjectId));
        this.SelectedTemplateId(data.AddMappingModel.SelectedTemplateId);
    } else {
        this.SelectedGcProject = ko.observable(data.GcProjects[0]);
        this.SelectedTemplateId(this.SitecoreTemplates()[0].SitrecoreTemplateId);
    }
    this.SelectedGcTemplate = ko.observable();
    this.SelectedScTemplate = ko.observable(self.find("SitrecoreTemplateId", data.SitecoreTemplates, data.AddMappingModel.SelectedTemplateId));

    self.gcProjectChanged();


};



function tabSlideFirst() {
    jQuery(".content_mapping").slideUp(0);
    jQuery(".title_mapping").removeClass("open");
    jQuery(jQuery(".title_mapping")[0]).addClass("open");
    jQuery(jQuery(".content_mapping")[0]).slideDown(0);
}
function tabInitSlide() {

    jQuery("body").on("click", ".title_mapping", function () {
        if (jQuery(this).hasClass("open")) {
            jQuery(this).next(".content_mapping").slideUp(200);
            jQuery(this).removeClass("open");
        } else {
            jQuery(".title_mapping.open").next(".content_mapping").slideUp(200);
            jQuery(".title_mapping.open").removeClass("open");
            jQuery(this).addClass("open");
            jQuery(this).next(".content_mapping").slideDown(200);

        }
    });
}

jQuery(window).resize(function () {
    jQuery(".tabs_mapping").css("max-height", jQuery(".gathercontent-dialog").height() - 240);
});

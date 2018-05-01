var ImportManager = function () {

    var MODE = {
        ChooseItmesForImort: 1,
        CheckItemsBeforeImport: 2,
        Import: 3,
        ImportResult: 4,
        Close: 5,
        Error: 6
    };

    var allItems = [], allItemsSelected = [];
    var self = this;

    self.errorText = ko.observable(),
    self.successImportedItemsCount = ko.observable(),
    self.notImportedItemsCount = ko.observable(),
    self.currentMode = ko.observable(MODE.ChooseItmesForImort);
    self.sortInfo = ko.observable();
    self.language = ko.observable(getUrlVars()["l"]),
    self.languages = ko.observableArray([]),

    self.defaultLocationTitle = ko.observable(decodeURI(getUrlVars()["t"])),
    self.defaultLocation = ko.observable("{" + getUrlVars()["id"] + "}"),
    self.showDropTree = ko.observable(false);

    self.projects = ko.observableArray([]),
    self.items = ko.observableArray(),
    self.statuses = ko.observableArray([]),
    self.templates = ko.observableArray([]),

    self.statusPostState = ko.observable(false),

    self.project = ko.observable(),
    self.statusFilter = ko.observable(),
    self.templateFilter = ko.observable(),

    self.query = ko.observable(''),
    self.expandGatherContentLinks = ko.observable(false),

    self.selectedItems = ko.observableArray([]);
    self.resultItems = ko.observableArray([]);

    self.filterOptions = {
        filterText: ko.observable(""),
        useExternalFilter: false
    };

    self.sortInfo = ko.observable();
    self.sortOnServer = ko.observable(false);


    self.filterConfirmOptions = {
        filterText: ko.observable(""),
        useExternalFilter: true
    };

    self.filterResultOptions = {
        filterText: ko.observable(""),
        useExternalFilter: true
    };


    self.setPagingData = function (data) {
        var items = data;
        allItems = items.slice(0);
        allItemsSelected = items;
        if (self.sortInfo()) {
            //window.kg.sortService.Sort(data, self.sortInfo()); - does not work with plain arrays. sorting extracted from that func.
            var col = self.sortInfo().column, direction = self.sortInfo().direction, sortFn, item;
            if (window.kg.sortService.colSortFnCache[col.field]) {
                sortFn = window.kg.sortService.colSortFnCache[col.field];
            } else if (col.sortingAlgorithm != undefined) {
                sortFn = col.sortingAlgorithm;
                window.kg.sortService.colSortFnCache[col.field] = col.sortingAlgorithm;
            } else {
                item = data[0];
                if (!item) {
                    return;
                }
                sortFn = kg.sortService.guessSortFn(item[col.field]);
                if (sortFn) {
                    window.kg.sortService.colSortFnCache[col.field] = sortFn;
                } else {
                    sortFn = window.kg.sortService.sortAlpha;
                }
            }
            data.sort(function (itemA, itemB) {
                var propA = window.kg.utils.evalProperty(itemA, col.field);
                var propB = window.kg.utils.evalProperty(itemB, col.field);
                if (!propB && !propA) {
                    return 0;
                } else if (!propA) {
                    return 1;
                } else if (!propB) {
                    return -1;
                }
                if (direction === "asc") {
                    return sortFn(propA, propB);
                } else {
                    return 0 - sortFn(propA, propB);
                }
            });
        }

        self.items(data);

        //self.pagingOptions.totalServerItems(data.length);
    };

    this.getPagedData = function () {
        var id = getUrlVars()["id"];
        var db = getUrlVars()["db"];
        var project = self.project() || 0;
        jQuery.ajax({
            url: '/services/Import.asmx/Get?id={' + id + '}&projectId=' + project + '&db=' + db,
            dataType: 'json',
            async: true,
            success: function (response) {
                jQuery(".preloader").hide();
                self.initVariables(response);
                self.setPagingData(response.Items);
                document.getElementsByTagName('input')[1].focus();
                jQuery(window).trigger('resize');
                //window.getSelection().removeAllRanges();

            },
            error: function (response) {
                self.errorCallbackHandle(response);
            }
        });
    };


    self.errorCallbackHandle = function (response) {
        jQuery(".preloader").hide();
        self.errorText(response.responseText);
        self.buttonClick(MODE.Error);
    }

    self.postErrorHandle = function (response) {
        jQuery(".preloader").hide();
        self.errorText(response);
        self.buttonClick(MODE.Error);
    }

    self.initVariables = function (response) {

        self.projects(response.Filters.Projects);
        self.project(response.Filters.Project);

        self.statuses(response.Filters.Statuses);

        self.templates(response.Filters.Templates);

        self.languages(response.Languages);
    }

    self.projectChanged = function (obj, event) {
        if (event.originalEvent) {
            jQuery(".preloader").show();
            self.getPagedData();
        }
    },


    //filters
    self.filter = function () {
        self.items.removeAll();

        var currentCollection = allItems.slice(0);
        currentCollection = self.search(currentCollection);
        currentCollection = self.filterByStatus(currentCollection);
        currentCollection = self.filterByTemplate(currentCollection);

        self.items(currentCollection);
    }

    self.search = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.query();
        if (value !== '') {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.Title.toLowerCase().indexOf(value.toLowerCase()) >= 0) {
                    resultCollection.push(currentElement);
                }
            }
        }

        return resultCollection;
    },

    self.filterByStatus = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.statusFilter();
        if (value) {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.Status.Id === value) {
                    resultCollection.push(currentElement);
                }
            }
        }
        return resultCollection;
    },

    self.filterByTemplate = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.templateFilter();
        if (value) {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.Template.Id === value) {
                    resultCollection.push(currentElement);
                }
            }
        }

        return resultCollection;
    }


    self.query.subscribe(self.filter);

    this.closeDropTree = function (model, e) {
        if (e.target.tagName === "INPUT") {
            return;
        }

        var id = "location-droptree";
        jQuery("#" + id).hide();
        this.IsShowing(false);
    };

    self.openDropTree = function () {
        var id = "location-droptree";
        var locationId = self.defaultLocation();

        if (!self.showDropTree()) {
            self.showDropTree(true);

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

                    self.showDropTree(false);
                    self.defaultLocation(node.data.key);
                    self.defaultLocationTitle(path);
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
            self.showDropTree(false);
        }
    }


    self.import = function () {
        var id = self.defaultLocation();

        var selectedItems = self.selectedItems();
        var items = [];

        selectedItems.forEach(function (item, i) {
            if (item.AvailableMappings.SelectedMappingId == null)
                item.AvailableMappings.SelectedMappingId = item.AvailableMappings.Mappings[0].Id;

            items.push({ Id: item.Id, SelectedMappingId: item.AvailableMappings.SelectedMappingId });
        });

        var lang = self.language();
        var status = self.statusFilter();
        var project = self.project();
        if (!self.statusPostState())
            status = "";
        jQuery.ajax
        ({
            type: "POST",
            url: '/services/Import.asmx/ImportItems?id=' + id + '&projectId=' + project + '&statusId=' + status + '&language=' + lang + '&expandLinks=' + self.expandGatherContentLinks(),
            dataType: 'text',
            //contentType: "text; charset=utf-8",
            data: JSON.stringify(items),
            success: function (response) {
                var jsonItems = jQuery.parseJSON(response);
                if (jsonItems.status && jsonItems.status == 'error') {
                    self.postErrorHandle(jsonItems.message);
                }
                var notImportedItemsCount = self.getNotImportedItemsCount(jsonItems);
                self.notImportedItemsCount(notImportedItemsCount);
                self.successImportedItemsCount(jsonItems.length - notImportedItemsCount);
                self.resultItems(jsonItems);
                self.buttonClick(MODE.ImportResult);
                jQuery(window).trigger('resize');
            },
            error: function (response) {
                self.errorCallbackHandle(response);
            }
        });
    }

    self.getNotImportedItemsCount = function (items) {
        var count = 0;
        items.forEach(function (item) {
            if (!item.IsImportSuccessful)
                count++;
        });

        return count;
    }

    self.close = function () {
        window.top.dialogClose();
    }


    self.buttonClick = function (newMode) {
        if (newMode === MODE.CheckItemsBeforeImport) {
            if (self.selectedItems().length == 0) {
                self.errorText('Please select at least one item');
            } else {
                self.currentMode(newMode);
                self.errorText('');
            }
        } else if (newMode === MODE.Import) {
            self.currentMode(newMode);
            self.import();
        } else if (newMode === MODE.Close) {
            self.currentMode(newMode);
            self.close();
        } else if (newMode === MODE.ChooseItmesForImort) {
            self.statusFilter = ko.observable();
            self.currentMode(newMode);
        } else {
            self.currentMode(newMode);
        }
    }

    self.getMode = function (section) {
        if (self.currentMode() === section) {
            return true;
        }
        return false;
    }

    self.setupWatcher = function (items) {
        for (var i = 0; i < items.length; i++) {
            items[i].Checked = ko.observable(false);
        }
        return items;
    }

    self.getImportResultTemplateColor = function (item) {
        if (!item.IsImportSuccessful)
            return 'red';
    }

    self.getImportResultMessageColor = function (item) {
        if (!item.IsImportSuccessful)
            return 'red';
        return 'green';
    }

    self.openCmsLink = function (item) {
        var link = item.CmsLink;
        window.open(link);
    }

    self.openGcLink = function (item) {
        var link = item.GcLink;
        window.open(link);
    }

    self.filterOptions.filterText.subscribe(function (data) {
        self.setPagingData(allItems);
    });


    self.getPagedData();

    var options =
    {
        showColumnMenu: false,
        showFilter: false,
        data: self.items,
        selectedItems: self.selectedItems,
        enablePaging: false,
        filterOptions: self.filterOptions,
        sortInfo: self.sortInfo,
        columnDefs: [
            {
                field: 'Status.Name',
                width: '**',
                displayName: 'Status',
                cellTemplate: '<div class="kgCellText"><div class="status-color" data-bind="style: { backgroundColor : $parent.entity.Status.Color }"></div><span data-bind="text: $parent.entity.Status.Name"></span></div>'
            },
            {
                field: 'Title',
                width: '**',
                displayName: 'Item name'
            },
            {
                field: 'LastUpdatedInGC',
                width: '**',
                displayName: 'Last updated in GatherContent',
                sortFn: dateSort
            },
            { field: 'Breadcrumb', width: '**', displayName: 'Path' },
            { field: 'Template.Name', width: 200, displayName: 'Template name' }
        ]
    };


    this.gridOptions = options;


    var confirmOptions =
      {
          canSelectRows: false,
          showColumnMenu: false,
          showFilter: false,
          data: self.selectedItems,
          enablePaging: false,
          filterOptions: self.filterConfirmOptions,
          columnDefs: [
              {
                  field: 'Status.Name',
                  displayName: 'Status', cellTemplate: '<div class="kgCellText"><div class="status-color" data-bind="style: { backgroundColor : $parent.entity.Status.Color }"></div><span data-bind="text: $parent.entity.Status.Name"></span></div>'
              },
              { field: 'Title', displayName: 'Item name' },
              { field: 'Template.Name', displayName: 'Template name' },
              {
                  displayName: 'Specify mappings', cellTemplate: '<div data-bind="if: $parent.entity.AvailableMappings.Mappings.length > 0">' +
                      '<div data-bind="if: $parent.entity.AvailableMappings.Mappings.length == 1">' +
                      '<span class="cell-padding" data-bind="text: $parent.entity.AvailableMappings.Mappings[0].Title"></span>' +
                      '<select class=\"mappings-cell\" \
                               data-bind="visible:false, options: $parent.entity.AvailableMappings.Mappings, \
							   selectedOptions: $parent.entity.AvailableMappings.Mappings[0].Id, \
                               optionsValue: \'Id\', \
                               optionsText: \'Title\',\
							   value: $parent.entity.AvailableMappings.SelectedMappingId"> \
                         </select>' +
                      '</div>' +
                      '<div data-bind="if: $parent.entity.AvailableMappings.Mappings.length > 1">' +
                          '<select class=\"mappings-cell\" \
                               data-bind="options: $parent.entity.AvailableMappings.Mappings, \
                               optionsValue: \'Id\', \
                               optionsText: \'Title\',\
                               value: $parent.entity.AvailableMappings.SelectedMappingId"> \
                         </select>' +
                      '</div>' +
                      '</div>'
              }
          ]
      };

    this.gridConfirmOptions = confirmOptions;


    var resultOptions =
    {
        displaySelectionCheckbox: false,
        canSelectRows: false,
        showColumnMenu: false,
        showFilter: false,
        data: self.resultItems,
        enablePaging: false,
        filterOptions: self.filterResultOptions,
        columnDefs: [
            {
                field: 'Status.Name',
                displayName: 'Status', cellTemplate: '<div class="kgCellText">' +
                    '<div class="status-color" data-bind="style: { backgroundColor : $parent.entity.Status.Color }">' +
                    '</div>' +
                    '<span data-bind="text: $parent.entity.Status.Name">' +
                    '</span>' +
                    '</div>'
            },
            { field: 'Title', displayName: 'Item name' },
            {
                field: 'Message',
                displayName: 'Import status',
                cellTemplate: '<div class="kgCellText" data-bind="style: { color: $parent.$userViewModel.getImportResultMessageColor($parent.entity) }, text: $parent.entity.Message"></div>'
            },
            {
                field: 'GcTemplateName',
                displayName: 'Template name',
                cellTemplate: '<div class="kgCellText" data-bind="style: { color: $parent.$userViewModel.getImportResultTemplateColor($parent.entity) }, text: $parent.entity.GcTemplateName"></div>'
            },
            {
                displayName: 'Open in Sitecore',
                cellClass: 'cell-padding',
                sortable: false,
                cellTemplate: '<a data-bind="if: $parent.entity.CmsLink!=null, click: function(){$parent.$userViewModel.openCmsLink($parent.entity)}">Open</a>'
            },
            {
                displayName: 'Open in GatherContent',
                cellClass: 'cell-padding',
                sortable: false,
                cellTemplate: '<a data-bind="click: function(){$parent.$userViewModel.openGcLink($parent.entity)}">Open</a>'
            }
        ]
    };

    this.gridResultOptions = resultOptions;

    var changeInit = {};
    jQuery("body").on("change", ".mappings-cell", function (el) {

        if (jQuery(".import-confirm-grid2").length) {

            if (jQuery(".col2:contains(" + jQuery(el.target).parents(".col3").siblings(".col2").text() + ")").length > 1) {
                if (!changeInit[jQuery(el.target).parents(".col3").siblings(".col2").text()]) {
                    var init = confirm('Set the mapping for all ' + jQuery(el.target).parents(".col3").siblings(".col2").text() + ' items?');
                    if (init) {

                        jQuery(".col2:contains(" + jQuery(el.target).parents(".col3")
                                .siblings(".col2").text() + ")").siblings(".col3")
                            .find("option[value='" + jQuery(el.target).find("option:selected").val() + "']").prop("selected", true);

                        var selectedItems = self.selectedItems();
                        selectedItems.forEach(function (item, i) {
                            item.AvailableMappings.SelectedMappingId = jQuery(el.target).find("option:selected").val();
                        });

                    }
                    else {
                        changeInit[jQuery(el.target).parents(".col3").siblings(".col2").text()] = true;
                    }
                }
            }
        }

    });
}


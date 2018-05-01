var ImportManager = function () {
    var MODE = {
        ChooseItmesForImort: 1,
        CheckItemsBeforeImport: 2,
        Confirm: 3,
        Import: 4,
        ImportResult: 5,
        Close: 6,
        Error: 7
    };

    var allItems = [], allItemsSelected = [];
    var self = this;
    self.errorText = ko.observable(),
    self.successImportedItemsCount = ko.observable(),
    self.notImportedItemsCount = ko.observable(),
    self.currentMode = ko.observable(MODE.ChooseItmesForImort);
    self.sortInfo = ko.observable();
    self.language = ko.observable(decodeURI(getUrlVars()["l"])),
    self.languages = ko.observableArray([]),
    self.projects = ko.observableArray([]),
    self.items = ko.observableArray([]),
    self.confirmItems = ko.observableArray([]),
    self.groupedItems = ko.observableArray([]),
    self.statuses = ko.observableArray([]),
    self.templates = ko.observableArray([]),
    self.statusPostState = ko.observable(false),
    self.project = ko.observable(),
    self.statusFilter = ko.observable(),
    self.templateFilter = ko.observable(),
    
    self.query = ko.observable(''),
    self.expandGatherContentLinks = ko.observable(false),

    self.selectedGroupItems = ko.observableArray([]),
    self.selectedItems = ko.observableArray([]);
    self.resultItems = ko.observableArray([]);

    self.filterOptions = {
        filterText: ko.observable(""),
        useExternalFilter: false
    };
    
    self.groupedGridFilterOptions = {
        filterText: ko.observable(""),
        useExternalFilter: true
    };

    self.confirmedFilterOptions = {
        filterText: ko.observable(""),
        useExternalFilter: false
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
        var project = self.project();
        project = project ? project : 0;
        jQuery.ajax({
            url: '/services/Import.asmx/GetMultiLocation?id={' + id + '}&projectId=' + project + '&db=' + db,
            dataType: 'json',
            async: true,
            success: function (response) {
                jQuery(".preloader").hide();
                self.initVariables(response);
                self.setPagingData(response.Items);
                //document.getElementsByTagName('input')[1].focus();


                jQuery(document).trigger('resize');
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

    self.setupDefaultValuesToFilters = function () {
        self.query('');
        self.statusFilter();
        self.templateFilter();
    }

    //filters
    self.filter = function () {
        self.items.removeAll();

        var currentCollection = allItems.slice(0);
        currentCollection = self.search(currentCollection);
        currentCollection = self.filterByStatus(currentCollection);
        currentCollection = self.filterByTemplate(currentCollection);

        self.items(currentCollection);
        resizeTableHead();
        jQuery(".tooltip").remove();
        initTooltip();
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

    //button click events
    self.ChooseDefaultLocation = function () {

        var selectedItems = self.selectedItems();
        var result = [];
        ko.utils.arrayForEach(selectedItems, function (item) {
            ko.utils.arrayForEach(item.AvailableMappings.Mappings, function (mapping) {

                var found = false;
                for (var i = 0; i < result.length; i++) {
                    if (result[i].MappingName == mapping.Title) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    result.push({
                        Id: mapping.Id,
                        TemplateName: item.Template.Name,
                        MappingName: mapping.Title,
                        ScTemplate: mapping.ScTemplate,
                        DefaultLocation: mapping.DefaultLocation,
                        DefaultLocationTitle: mapping.DefaultLocationTitle,
                        OpenerId: mapping.OpenerId
                    });
                }

            });
        });

        self.groupedItems(result);
        jQuery(document).trigger('resize');


    };
    self.switchToCheckItemsBeforeImport = function () {
        var result = [];
        var items = self.selectedItems();
        self.confirmItems.removeAll();
        ko.utils.arrayForEach(items, function (item) {
            var templateItems = self.findByTemplateName(item.Template.Name);
            ko.utils.arrayForEach(templateItems, function (templateItem) {
                ko.utils.arrayForEach(item.AvailableMappings.Mappings, function (mapping) {
                    if (mapping.Id == templateItem.Id) {
                        result.push({
                            ItemId: item.Id,
                            ItemTitle: item.Title,
                            TemplateName: item.Template.Name,
                            MappingId: mapping.Id,
                            MappingName: mapping.Title,
                            ScTemplate: mapping.ScTemplate,
                            DefaultLocation: templateItem.DefaultLocation,
                            DefaultLocationTitle: templateItem.DefaultLocationTitle,
                        });
                    }
                });
            });
        });

        self.confirmItems(result);
        jQuery(document).trigger('resize');
    }

    self.findByTemplateName = function (data) {
        var result = [];
        ko.utils.arrayForEach(self.selectedGroupItems(), function (item) {
            if (item.TemplateName === data) {
                result.push(item);
            }
        });
        return result;
    };


    self.import = function () {
        var importItems = [];
        ko.utils.arrayForEach(self.confirmItems(), function (item) {
            importItems.push({
                Id: item.ItemId,
                SelectedLocation: item.DefaultLocation,
                IsImport: true, // TODO redundant?
                SelectedMappingId: item.MappingId
            });
        });

        var lang = self.language();
        var status = self.statusFilter();
        var project = self.project();
        if (!self.statusPostState())
            status = "";
        jQuery.ajax
        ({
            type: "POST",
            url: '/services/Import.asmx/ImportItemsWithLoc?projectId=' + project + '&statusId=' + status + '&language=' + lang + '&expandLinks=' + self.expandGatherContentLinks(),
            dataType: 'text',
            //contentType: "text; charset=utf-8",
            data: JSON.stringify(importItems),
            success: function (response) {
                if (response.status == 'error') {
                    self.postErrorHandle(response.message);
                }

                var jsonItems = jQuery.parseJSON(response);

                var notImportedItemsCount = self.getNotImportedItemsCount(jsonItems);
                self.notImportedItemsCount(notImportedItemsCount);
                self.successImportedItemsCount(jsonItems.length - notImportedItemsCount);
                self.resultItems(jsonItems);
                self.buttonClick(MODE.ImportResult);

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


    this.closeDropTree = function (model, e) {
        if (e.target.tagName === "INPUT") {
            return;
        }

        var items = self.groupedItems();

        for (var i = 0; i < items.length; i++) {
            items[i].IsShowing = false;
            jQuery("#" + items[i].OpenerId).hide();
        }
    };

    self.openDropTree = function (item) {
        var id = item.OpenerId;
        var locationId = item.DefaultLocation;
        var items = self.groupedItems();

        for (var i = 0; i < items.length; i++) {
            if (items[i].OpenerId != id) {
                items[i].IsShowing = false;
                //TODO use Knockout
                jQuery("#" + items[i].OpenerId).hide();
            }
        }


        if (!item.IsShowing) {
            //TODO use Knockout
            jQuery("#" + id).show();
            item.IsShowing = true;
            jQuery("#" + id).dynatree({
                autoFocus: false,
                imagePath: "~/icon/",
                initAjax: {
                    url: '/services/DropTree.asmx/GetTopLevelNode?id=' + locationId,
                    data: { mode: "funnyMode" }
                },
                onActivate: function (node) {
                    jQuery('[data-openerid="' + id + '"]').val(node.data.title);
                    jQuery("#" + id).hide();

                    item.IsShowing = false;
                    item.DefaultLocation = node.data.key;
                    item.DefaultLocationTitle = node.data.title;

                    var parent = node.parent;
                    var path = "/" + node.data.title;
                    while (parent.data.title != null) {
                        path = "/" + parent.data.title + path;
                        parent = parent.parent;
                    }
                    item.DefaultLocationTitle = path;
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
            //item.IsShowing = false;
        }
    }


    self.buttonClick = function (newMode) {
        if (newMode === MODE.CheckItemsBeforeImport) {
            if (self.selectedItems().length == 0) {
                self.errorText('Please select at least one item');
            } else {
                self.currentMode(newMode);
                self.ChooseDefaultLocation();
                self.errorText('');
                jQuery(".kgAggregate").trigger("click")
            }
        }
        else if (newMode === MODE.Confirm) {
            var emptyLocationError = false;
            if (self.selectedGroupItems().length == 0) {
                self.errorText('Please select at least one item');
            } else {
                ko.utils.arrayForEach(self.selectedGroupItems(), function (item) {
                    if (item.DefaultLocation == "") {
                        emptyLocationError = true;
                    }
                });
                if (!emptyLocationError) {
                    self.currentMode(newMode);
                    self.switchToCheckItemsBeforeImport();
                    jQuery(document).trigger('resize');
                    self.errorText('');
                } else {
                    self.errorText('Please select default location');
                }
            }
        }
        else if (newMode === MODE.Import) {
            self.currentMode(newMode);
            self.import();
            jQuery(document).trigger('resize');
        } else if (newMode === MODE.Close) {
            self.currentMode(newMode);
            self.close();
            jQuery(document).trigger('resize');
        } else if (newMode === MODE.ChooseItmesForImort) {
            self.statusFilter = ko.observable();
            self.currentMode(newMode);
            jQuery(document).trigger('resize');

        } else {
            self.currentMode(newMode);
            jQuery(document).trigger('resize');
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
                displaySelectionCheckbox: true,
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
                ],
                afterSelectionChange: function () { return true; }

            };


    this.gridOptions = options;
    
    var groupedOptions =
        {
            showColumnMenu: false,
            showFilter: false,
            canSelectRows: true,
            selectWithCheckboxOnly: true,
            data: self.groupedItems,
            selectedItems: self.selectedGroupItems,
            enablePaging: false,
            filterOptions: self.groupedGridFilterOptions,
            groups: ["TemplateName"],

            columnDefs: [
               // { field: '...', displayName: '...', cellTemplate: tplCheckbox },
                { field: 'TemplateName', width: '**', displayName: 'Template Name' },
                { field: 'MappingName', width: '**', displayName: 'Mapping Name' },
                { field: 'ScTemplate', width: '**', displayName: 'Sitecore Template' },
                {
                    field: 'DefaultLocationTitle', width: "450", displayName: 'Default Location',
                    cellTemplate: '<div class="tree_wrap"><input data-bind="value: $parent.entity.DefaultLocationTitle, attr: {\'data-openerid\': $parent.entity.OpenerId }, click: function(){$parent.$userViewModel.openDropTree($parent.entity)}" type="text" />' +
                           '<input data-bind="value: $parent.entity.DefaultLocation, visible: false" type="text" />' +
                           '<div class="tree_init" data-bind="attr: { id: $parent.entity.OpenerId }" style="position: absolute; left: 0; top: 30px; width: 450px; height: 400px; z-index: 1000;">' +
                              '<div style="width:450px;height:400px;">' +
                                  '<div data-bind="css: { \'class\': $parent.entity.OpenerId }"> </div>' +
                              '</div>' +
                           '</div></div>'
                }
            ], afterSelectionChange: function () { return true; }
        };
    this.groupedGridOptions = groupedOptions;


    var confirmOptions =
    {
        displaySelectionCheckbox: false,
        canSelectRows: false,
        showColumnMenu: false,
        showFilter: false,
        data: self.confirmItems,
        enablePaging: false,
        filterOptions: self.confirmedFilterOptions,
        columnDefs: [
            { field: 'ItemTitle', displayName: 'Title' },
            { field: 'MappingName', displayName: 'Mapping Name' },
            { field: 'ScTemplate', displayName: 'Sitecore Template' },
            { field: 'DefaultLocationTitle', displayName: 'Location' }
        ]
    };

    this.confirmGridOptions = confirmOptions;


    var resultOptions =
   {
       displaySelectionCheckbox: false,
       canSelectRows: false,
       showColumnMenu: false,
       showFilter: false,
       data: self.resultItems,
       enablePaging: false,     
       filterOptions: self.filterResultOptions,
       sortInfo: self.sortInfo,
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

}


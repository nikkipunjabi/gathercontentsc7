var UpdateManager = function () {
    var MODE = {
        ChooseItmesForImort: 1,
        CheckItemsBeforeImport: 2,
        Import: 3,
        ImportResult: 4,
        Close: 5,
        Error: 6
    };

    this.allItems = [];
    var allItemsSelected = [];
    var self = this;
    this.allItemsSelected = [];

    self.errorText = ko.observable(),
    self.successImportedItemsCount = ko.observable(),
    self.notUpdaredItemsCount = ko.observable(),

    self.currentMode = ko.observable(MODE.ChooseItmesForImort);
    self.sortInfo = ko.observable();
    self.language = ko.observable(getUrlVars()["l"]),
    self.languages = ko.observableArray([]),

    self.items = ko.observableArray([]),
    self.statuses = ko.observableArray([]),
    self.templates = ko.observableArray([]),
    self.projects = ko.observableArray([]),

    self.statusPostState = ko.observable(false),

    self.projectFilter = ko.observable(),
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
        self.allItems = items.slice(0);
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
        //var pagedData = data.slice((page - 1) * pageSize, page * pageSize);
        self.items(data);
        //self.pagingOptions.totalServerItems(data.length);
    };

    this.getPagedData = function () {
        var id = getUrlVars()["id"];
        var db = getUrlVars()["db"];
        jQuery.ajax({
            url: '/services/Update.asmx/Get?id={' + id + '}' + '&db=' + db,
            dataType: 'json',
            async: true,
            success: function (response) {
                jQuery(".preloader").hide();
                self.initVariables(response);
                self.setPagingData(response.Items);
                document.getElementsByTagName('input')[1].focus();
                jQuery(window).trigger('resize');
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
        //var items = self.setupWatcher(response.Items);
        self.items(response.Items);
        self.allItems = response.Items.slice(0);

        self.statuses(response.Filters.Statuses);
        self.projects(response.Filters.Projects);
        self.templates(response.Filters.Templates);

        self.languages(response.Languages);
    }

    self.setupDefaultValuesToFilters = function () {
        self.query('');
        self.statusFilter();
        self.templateFilter();
    }

    //filters
    self.filter = function () {
        self.items.removeAll();

        var currentCollection = self.allItems.slice(0);
        currentCollection = self.search(currentCollection);
        currentCollection = self.filterByStatus(currentCollection);
        currentCollection = self.filterByTemplate(currentCollection);
        currentCollection = self.filterByProject(currentCollection);

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
                if (currentElement.GcItem.Name.toLowerCase().indexOf(value.toLowerCase()) >= 0) {
                    resultCollection.push(currentElement);
                }
            }
        }

        return resultCollection;
    },

    self.filterByProject = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.projectFilter();
        if (value) {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.GcProject.Name === value) {
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
                if (currentElement.GcTemplate.Id === value) {
                    resultCollection.push(currentElement);
                }
            }
        }

        return resultCollection;
    }

    //table rendering
    self.AllChecked = ko.computed({
        read: function () {
            var items = self.items();
            if (items.length === 0)
                return false;

            var firstUnchecked = ko.utils.arrayFirst(items, function (item) {
                if (item.Checked)
                    return item.Checked() === false;
            });

            return firstUnchecked == null;
        },
        write: function (value) {
            ko.utils.arrayForEach(self.items(), function (item) {
                item.Checked(value);
            });
        }
    });

    self.getCheckedCount = ko.computed(function () {
        var counter = 0;
        ko.utils.arrayForEach(self.items(), function (item) {
            if (item.Checked && item.Checked() === true)
                counter++;
        });

        return counter;
    });

    self.afterStatusesSelectRender = function (option, status) {
        if (status.color) {
            option.style.color = status.color;
        }
    };

    self.getSelectedStatusColor = function () {
        var result = "";
        ko.utils.arrayForEach(self.statuses(), function (status) {
            if (status.id.toLowerCase() === self.statusFilter()) {
                result = status.color;
                return;
            }
        });

        return result;
    }

    self.checkRow = function () {
        this.Checked(!this.Checked());
    }


    self.openCmsLink = function (item) {
        var link = item.CmsLink;
        window.open(link);
    }

    self.openGcLink = function (item) {
        var link = item.GcLink;
        window.open(link);
    }

    self.query.subscribe(self.filter);

    //button click events
    self.switchToCheckItemsBeforeImport = function () {
        var result = [];

        ko.utils.arrayForEach(self.items(), function (item) {
            if (item.Checked && item.Checked() === true)
                result.push(item);
        });

        self.allItemsSelected = self.selectedItems();
        self.selectedItems(result);
    }

    self.import = function () {
        var lang = self.language();
        var id = getUrlVars()["id"];
        var selectedItems = self.selectedItems();
        var items = [];
        selectedItems.forEach(function (item, i) {
            items.push({
                GCId: item.GcItem.Id,
                CMSId: item.Id
            });
        });
        var status = self.statusFilter();
        if (!self.statusPostState())
            status = "";
        jQuery.ajax
        ({
            type: "POST",
            url: '/services/Update.asmx/UpdateItems?id={' + id + '}&statusId=' + status + '&language=' + lang + '&expandLinks=' + self.expandGatherContentLinks(),
            dataType: 'text',
            //contentType: "text; charset=utf-8",
            data: JSON.stringify(items),
            success: function (response) {
                if (response.status == 'error') {
                    self.postErrorHandle(response.message);
                }
                var jsonItems = jQuery.parseJSON(response);
                var notUpdatedCount = self.getNotUpdatedItemsCount(jsonItems);
                self.successImportedItemsCount(jsonItems.length - notUpdatedCount);
                self.notUpdaredItemsCount(notUpdatedCount);
                self.resultItems(jsonItems);
                self.buttonClick(MODE.ImportResult);
                jQuery(window).trigger('resize');
            },
            error: function (response) {
                self.errorCallbackHandle(response);
            }
        });
    }

    self.getNotUpdatedItemsCount = function (items) {
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

    self.backButtonClick = function () {
        self.selectedItems(self.allItemsSelected);
        self.items(self.allItems);
    }

    self.buttonClick = function (newMode) {
        if (newMode === MODE.CheckItemsBeforeImport) {
            if (self.selectedItems().length == 0) {
                self.errorText('Please select at least one item');
            } else {
                self.currentMode(newMode);
                resizeTableHead();
                //self.switchToCheckItemsBeforeImport();
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
            //self.backButtonClick();
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

    //self.setupWatcher = function (items) {
    //    for (var i = 0; i < items.length; i++) {
    //        items[i].Checked = ko.observable(false);
    //    }

    //    return items;
    //}

    self.getImportResultTemplateColor = function (item) {
        if (!item.IsImportSuccessful)
            return 'red';
    }

    self.getImportResultMessageColor = function (item) {
        if (!item.IsImportSuccessful)
            return 'red';
        return 'green';
    }

    self.getDateColor = function (item) {
        //if (item.IsNeedToHighlightingDate)
        return 'dateWarnings';
    }


    self.filterOptions.filterText.subscribe(function (data) {
        self.setPagingData(self.allItems);
    });

    self.getPagedData();


    var options =
        {
            afterSelectionChange: function () { return true; },
            showColumnMenu: false,
            showFilter: false,
            data: self.items,
            selectedItems: self.selectedItems,
            enablePaging: false,
            filterOptions: self.filterOptions,
            sortInfo: self.sortInfo,
            columnDefs: [
                {
                    field: 'Status.Name', width: '**',
                    displayName: 'Status', cellTemplate: '<div class="kgCellText"><div class="status-color" data-bind="style: { backgroundColor : $parent.entity.Status.Color }"></div><span data-bind="text: $parent.entity.Status.Name"></span></div>'
                },
                { field: 'ScTitle', width: '**', displayName: 'Sitecore Title' },
                { field: 'GcItem.Name', width: '**', displayName: 'GatherContent Item Name' },
                { field: 'GcProject.Name', width: '**', displayName: 'GatherContent Project' },
                {
                    field: 'LastUpdatedInGc', width: '**', displayName: 'Last updated in GatherContent',
                    sortFn: dateSort
                },
                {
                    field: 'LastUpdatedInSitecore', width: '**', displayName: 'Last updated in Sitecore',
                    sortFn: dateSort
                },
                { field: 'GcTemplate.Name', width: '**', displayName: 'GatherContent Template' },
                { field: 'ScTemplateName', width: '**', displayName: 'Sitecore Template' },
                { displayName: 'Open in Sitecore', width: 70, cellClass: 'cell-padding', sortable: false, cellTemplate: '<a data-bind="if: $parent.entity.CmsLink!=null, click: function(){$parent.$userViewModel.openCmsLink($parent.entity)}">Open</a>' },
                { displayName: 'Open in GatherContent', width: 70, cellClass: 'cell-padding', sortable: false, cellTemplate: '<a data-bind="click: function(){$parent.$userViewModel.openGcLink($parent.entity)}">Open</a>' }
            ],
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
                    field: 'Status.Name', width: '**',
                    displayName: 'Status', cellTemplate: '<div class="kgCellText"><div class="status-color" data-bind="style: { backgroundColor : $parent.entity.Status.Color }"></div><span data-bind="text: $parent.entity.Status.Name"></span></div>'
                },
                { field: 'ScTitle', width: '**', displayName: 'Sitecore Title' },
                { field: 'GcItem.Name', width: '**', displayName: 'GatherContent Item Name' },
                { field: 'GcProject.Name', width: '**', displayName: 'GatherContent Project' },
                {
                    field: 'LastUpdatedInGc', width: '**', displayName: 'Last updated in GatherContent',
                    sortFn: dateSort
                },
                {
                    field: 'LastUpdatedInSitecore', width: '**', displayName: 'Last updated in Sitecore',
                    sortFn: dateSort
                },
                { field: 'GcTemplate.Name', width: '**', displayName: 'GatherContent Template' },
                { field: 'ScTemplateName', width: '**', displayName: 'Sitecore Template' },
                { displayName: 'Open in Sitecore', cellClass: 'cell-padding', sortable: false, cellTemplate: '<a data-bind="if: $parent.entity.CmsLink!=null, click: function(){$parent.$userViewModel.openCmsLink($parent.entity)}">Open</a>' },
                { displayName: 'Open in GatherContent', cellClass: 'cell-padding', sortable: false, cellTemplate: '<a data-bind="click: function(){$parent.$userViewModel.openGcLink($parent.entity)}">Open</a>' }
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

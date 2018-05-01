namespace GatherContent.Connector.WebControllers.Areas.Gc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    using IoC;

    using Managers.Interfaces;
    using Managers.Models.UpdateItems;

    using Models.Import;
    using Models.Update;

    using Newtonsoft.Json;

    using Sitecore.Diagnostics;

    /// <summary>
    /// 
    /// </summary>
    public class UpdateController : BaseController
    {
        protected IUpdateManager UpdateManager;
        protected IImportManager ImportManager;
        protected ILinkManager LinkManager;


        public UpdateController()
        {
            ImportManager = ServiceFactory.ImportManager;
            LinkManager = ServiceFactory.LinkManager;
            UpdateManager = ServiceFactory.UpdateManager;
        }

        private FiltersViewModel GetFilters(UpdateFiltersModel filters)
        {
            var filtersViewModel = new FiltersViewModel();

            foreach (var project in filters.Projects)
            {
                filtersViewModel.Projects.Add(new ProjectViewModel
                {
                    Id = project.Id,
                    Name = project.Name
                });
            }

            foreach (var status in filters.Statuses)
            {
                var project = filters.Projects.FirstOrDefault(i => i.Id == status.ProjectId);
                var statusName = status.Name;
                if (project != null)
                {
                    statusName = statusName + " (" + project.Name + ")";
                }
                filtersViewModel.Statuses.Add(new StatusViewModel
                {
                    Id = status.Id,
                    Name = statusName
                });
            }

            foreach (var template in filters.Templates)
            {
                filtersViewModel.Templates.Add(new TemplateViewModel
                {
                    Id = template.Id,
                    Name = template.Name
                });
            }


            return filtersViewModel;
        }

        public string Get(string id, string db)
        {
            try
            {
                var language = Sitecore.Context.Language;
                var updateModel = UpdateManager.GetItemsForUpdate(id, language.CultureInfo.TwoLetterISOLanguageName);
                var importViewModel = new UpdateViewModel {Languages = this.GetLanguages(db)};

                foreach (var updateItemModel in updateModel.Items)
                {
                    importViewModel.Items.Add(new UpdateListItemViewModel
                    {
                        Id = updateItemModel.CmsId,
                        ScTitle = updateItemModel.Title,
                        ScTemplateName = updateItemModel.CmsTemplate.Name,
                        CmsLink = updateItemModel.CmsLink,
                        GcLink = updateItemModel.GcLink,
                        LastUpdatedInGc = updateItemModel.GcItem.LastUpdatedInGc,
                        LastUpdatedInSitecore = updateItemModel.LastUpdatedInCms,
                        GcProject = new ProjectViewModel
                        {
                            Id = updateItemModel.Project.Id,
                            Name = updateItemModel.Project.Name
                        },
                        GcTemplate = new TemplateViewModel
                        {
                            Id = updateItemModel.GcTemplate.Id,
                            Name = updateItemModel.GcTemplate.Name
                        },
                        Status = new StatusViewModel
                        {
                            Id = updateItemModel.Status.Id,
                            Name = updateItemModel.Status.Name,
                            Color = updateItemModel.Status.Color
                        },
                        GcItem = new ItemViewModel
                        {
                            Id = updateItemModel.GcItem.Id,
                            Name = updateItemModel.GcItem.Title
                        },

                    });
                }

                importViewModel.Filters = this.GetFilters(updateModel.Filters);

                var model = JsonConvert.SerializeObject(importViewModel);
                return model;
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return exception.Message + " Please check your credentials";
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return exception.Message;
            }
        }

        public ActionResult UpdateItems(string id, string statusId, string language, bool expandLinks)
        {
            try
            {
                List<UpdateListIds> items;
                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                {
                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                }
                using (var reader = new StreamReader(System.Web.HttpContext.Current.Request.InputStream))
                {
                    var body = reader.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<UpdateListIds>>(body);
                }
                var model = new List<ImportResultViewModel>();
                var result = UpdateManager.UpdateItems(id, items, language);
                foreach (var item in result)
                {
                    if (expandLinks && item.IsImportSuccessful)
                    {
                        LinkManager.ExpandLinksInText(item.CmsId, false);
                    }

                    model.Add(new ImportResultViewModel
                    {
                        Title = item.GcItem.Title,
                        IsImportSuccessful = item.IsImportSuccessful,
                        Message = item.ImportMessage,
                        CmsLink = item.CmsLink,
                        GcLink = item.GcLink,
                        Status = new StatusViewModel
                        {
                            Color = item.Status.Color,
                            Name = item.Status.Name
                        },
                        GcTemplateName = item.GcTemplate.Name
                    });
                }
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return Json(new { status = "error", message = exception.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
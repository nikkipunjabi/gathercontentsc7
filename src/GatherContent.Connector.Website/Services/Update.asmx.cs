using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Newtonsoft.Json;

using Sitecore.Diagnostics;
using System.Web.Services;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.WebControllers.IoC;
using System.Web.Script.Services;
using GatherContent.Connector.WebControllers.Models.Update;
using GatherContent.Connector.WebControllers.Models.Import;
using System.Web.Script.Serialization;
using GatherContent.Connector.Managers.Models.UpdateItems;

namespace GatherContent.Connector.Website.Services
{
    /// <summary>
    /// Summary description for Update
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Update : System.Web.Services.WebService
    {

        protected IUpdateManager UpdateManager;
        protected IImportManager ImportManager;
        protected ILinkManager LinkManager;


        public Update()
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


        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void Get(string id, string db)
        {
            object resultObject = null;
            try
            {
                var language = Sitecore.Context.Language;
                var updateModel = UpdateManager.GetItemsForUpdate(id, language.CultureInfo.TwoLetterISOLanguageName);
                var importViewModel = new UpdateViewModel { Languages = this.GetLanguages(db) };

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

                resultObject = importViewModel;

                //var model = JsonConvert.SerializeObject(importViewModel);
                //return model;
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
                //return exception.Message + " Please check your credentials";
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = "GatherContent message: " + exception.Message };
                //return exception.Message;
            }
            finally
            {
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(resultObject));
            }
        }

        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdateItems()
        {
            string id = System.Web.HttpContext.Current.Request.QueryString["id"];
            string statusId = System.Web.HttpContext.Current.Request.QueryString["statusId"];
            string language = System.Web.HttpContext.Current.Request.QueryString["language"];
            bool expandLinks = System.Boolean.Parse(System.Web.HttpContext.Current.Request.QueryString["expandLinks"]);

            object resultObject = null;
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

                resultObject = model;
                //return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
                //return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = "GatherContent message: " + exception.Message };
                //return Json(new { status = "error", message = exception.Message }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(resultObject));
            }

        }

        protected List<LanguageViewModel> GetLanguages(string databse)
        {
            var model = new List<LanguageViewModel>();
            var database = Sitecore.Configuration.Factory.GetDatabase(databse);
            var languages = database.GetLanguages();

            foreach (var language in languages)
            {
                model.Add(new LanguageViewModel
                {
                    Name = language.CultureInfo.DisplayName,
                    IsoCode = language.CultureInfo.Name
                });
            }
            return model;
        }
    }
}

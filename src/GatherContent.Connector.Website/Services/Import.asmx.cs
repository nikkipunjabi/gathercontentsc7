﻿using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Models.ImportItems;
using GatherContent.Connector.WebControllers.IoC;
using GatherContent.Connector.WebControllers.Models.Import;
using Newtonsoft.Json;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace GatherContent.Connector.Website.Services
{
    /// <summary>
    /// Summary description for Import
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Import : System.Web.Services.WebService
    {

        protected IImportManager ImportManager;
        protected IDropTreeManager DropTreeManager;
        protected ILinkManager LinkManager;

        public Import()
        {
            ImportManager = ServiceFactory.ImportManager;
            LinkManager = ServiceFactory.LinkManager;
            DropTreeManager = ServiceFactory.DropTreeManager;
        }

        //[WebMethod(enableSession: true)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public FiltersViewModel GetFilters(string projectId)
        {
            //object resultObject = null;

            //try
            //{

            var filtersViewModel = new FiltersViewModel();
            var filters = ImportManager.GetFilters(projectId);

            if (filters.CurrentProject != null)
            {
                filtersViewModel.Project = new ProjectViewModel
                {
                    Id = filters.CurrentProject.Id,
                    Name = filters.CurrentProject.Name
                };
            }

            filtersViewModel.Projects.Add(new ProjectViewModel
            {
                Id = "0",
                Name = "Select project"
            });
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
                filtersViewModel.Statuses.Add(new StatusViewModel
                {
                    Id = status.Id,
                    Name = status.Name
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

            //resultObject = filtersViewModel;
            //}
            //finally
            //{
            //    Context.Response.Clear();
            //    Context.Response.ContentType = "application/json";
            //    JavaScriptSerializer js = new JavaScriptSerializer();
            //    Context.Response.Write(js.Serialize(resultObject));
            //}
            return filtersViewModel;
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


        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void Get(string id, string projectId, string db)
        {

            object resultObject = null;
            try
            {
                var items = ImportManager.GetImportDialogModel(id, projectId);
                var importViewModel = new ImportViewModel();
                importViewModel.Languages = this.GetLanguages(db);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var importItem = new ImportListItemViewModel
                        {
                            Id = item.GcItem.Id,
                            Title = item.GcItem.Title,
                            Template = new TemplateViewModel
                            {
                                Name = item.GcTemplate.Name,
                                Id = item.GcTemplate.Id
                            },
                            Status = new StatusViewModel
                            {
                                Color = item.Status.Color,
                                Name = item.Status.Name,
                                Id = item.Status.Id
                            },
                            Breadcrumb = item.Breadcrumb,
                            LastUpdatedInGC = item.GcItem.LastUpdatedInGc

                        };


                        foreach (var availableMapping in item.AvailableMappings.Mappings)
                        {
                            importItem.AvailableMappings.Mappings.Add(new AvailableMappingViewModel
                            {
                                Id = availableMapping.Id,
                                Title = availableMapping.Title
                            });
                        }

                        importViewModel.Items.Add(importItem);
                    }
                }

                importViewModel.Filters = this.GetFilters(projectId);

                resultObject = importViewModel;
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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]

        public void GetMultiLocation(string id, string projectId, string db)
        {
            object resultObject = null;
            try
            {
                var items = ImportManager.GetImportDialogModel(id, projectId);
                var importViewModel = new ImportViewModel();

                importViewModel.Languages = this.GetLanguages(db);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var importItem = new ImportListItemViewModel
                        {
                            Id = item.GcItem.Id,
                            Title = item.GcItem.Title,
                            Template = new TemplateViewModel
                            {
                                Name = item.GcTemplate.Name,
                                Id = item.GcTemplate.Id
                            },
                            Status = new StatusViewModel
                            {
                                Color = item.Status.Color,
                                Name = item.Status.Name,
                                Id = item.Status.Id
                            },
                            Breadcrumb = item.Breadcrumb,
                            LastUpdatedInGC = item.GcItem.LastUpdatedInGc

                        };

                        foreach (var availableMapping in item.AvailableMappings.Mappings)
                        {
                            importItem.AvailableMappings.Mappings.Add(new AvailableMappingViewModel
                            {
                                Id = availableMapping.Id,
                                Title = availableMapping.Title,
                                OpenerId = "drop-tree" + Guid.NewGuid(),
                                DefaultLocation = availableMapping.DefaultLocationId,
                                DefaultLocationTitle = availableMapping.DefaultLocationTitle,
                                ScTemplate = availableMapping.CmsTemplateName,
                            });
                        }

                        importViewModel.Items.Add(importItem);
                    }
                }

                importViewModel.Filters = this.GetFilters(projectId);
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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void ImportItems()
        {

            string id = System.Web.HttpContext.Current.Request.QueryString["id"];
            string projectId = System.Web.HttpContext.Current.Request.QueryString["projectId"];
            string statusId = System.Web.HttpContext.Current.Request.QueryString["statusId"];
            string language = System.Web.HttpContext.Current.Request.QueryString["language"];
            bool expandLinks = System.Boolean.Parse(System.Web.HttpContext.Current.Request.QueryString["expandLinks"]);

            object resultObject = null;
            try
            {
                List<ImportItemModel> items;
                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                {
                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                }
                using (var reader = new StreamReader(System.Web.HttpContext.Current.Request.InputStream))
                {
                    var body = reader.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<ImportItemModel>>(body);
                }
                var model = new List<ImportResultViewModel>();
                var result = ImportManager.ImportItems(id, items, projectId, statusId, language);
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

        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ImportItemsWithLoc()
        {

            string projectId = System.Web.HttpContext.Current.Request.QueryString["projectId"];
            string statusId = System.Web.HttpContext.Current.Request.QueryString["statusId"];
            string language = System.Web.HttpContext.Current.Request.QueryString["language"];
            bool expandLinks = System.Boolean.Parse(System.Web.HttpContext.Current.Request.QueryString["expandLinks"]);

            object resultObject = null;
            try
            {
                List<LocationImportItemModel> items;
                if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                {
                    System.Web.HttpContext.Current.Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                }
                using (var reader = new StreamReader(System.Web.HttpContext.Current.Request.InputStream))
                {
                    var body = reader.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<LocationImportItemModel>>(body);
                }
                var model = new List<ImportResultViewModel>();
                var result = ImportManager.ImportItemsWithLocation(items, projectId, statusId, language);
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
    }
}

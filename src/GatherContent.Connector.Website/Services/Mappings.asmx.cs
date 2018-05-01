using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using Sitecore.Diagnostics;

namespace GatherContent.Connector.Website.Services
{
    using System.Web.Script.Services;

    using Managers.Interfaces;

    using Newtonsoft.Json;

    using WebControllers.IoC;
    using WebControllers.Models.Mapping;
    using System.Globalization;
    using System.Net;
    using Managers.Models.Mapping;
    using WebControllers.Models.Import;

    /// <summary>
    /// Summary description for Mappings
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Mappings : System.Web.Services.WebService
    {
        protected IMappingManager MappingManager;
        protected ILinkManager LinkManager;

        Newtonsoft.Json.JsonSerializer s = new JsonSerializer();

        public Mappings()
        {
            MappingManager = ServiceFactory.MappingManager;
            LinkManager = ServiceFactory.LinkManager;
        }

        private Dictionary<string, string> GetMapRules()
        {
            return new Dictionary<string, string>
            {
                {"text", "Single-Line Text, Multi-Line Text, Rich Text, Datetime, Date, General Link, Checkbox"},
                {"section", "Single-Line Text, Multi-Line Text, Rich Text"},
                {"choice_radio", "Droptree, Checklist, Multilist, Multilist with Search, Treelist, TreelistEx"},
                {"choice_checkbox", "Checklist, Multilist, Multilist with Search, Treelist, TreelistEx, Checkbox"},
                {"files", "Image, File, Droptree, Multilist, Multilist with Search, Treelist, TreelistEx"}
            };
        }

        private static List<FieldMappingModel> GetFieldMappings(IEnumerable<TemplateTab> tabs)
        {
            var fieldMappings = new List<FieldMappingModel>();
            foreach (var tab in tabs)
            {
                foreach (var field in tab.Fields)
                {
                    if (field.SelectedScField != "0")
                    {
                        var fieldMapping = new FieldMappingModel
                        {
                            CmsTemplateId = field.SelectedScField,
                            GcFieldId = field.FieldId,
                            GcFieldName = field.FieldName
                        };
                        fieldMappings.Add(fieldMapping);
                    }
                }
            }
            return fieldMappings;
        }


        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]

        public void Try()
        {
            Context.Response.Clear();
            Context.Response.BufferOutput = true;
            try
            {
                var data = MappingManager.GetAllGcProjects();
                //Context.Response.ContentType = "application/json";

                var message = "OK";
                Context.Response.Write(message);
            }
            catch (Exception ex)
            {
                Context.Response.StatusCode = 204;
                Context.Response.Write(ex.Message);
            }
        }

        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void Get()
        {

            object resultObject = null;
            try
            {
                var model = new List<TemplateMappingViewModel>();
                var mappings = MappingManager.GetMappingModel();
                foreach (var mapping in mappings)
                {
                    var mappingModel = new TemplateMappingViewModel
                    {
                        GcProjectName = mapping.GcProject.Name,
                        GcTemplateId = mapping.GcTemplate.Id,
                        GcTemplateName = mapping.GcTemplate.Name,
                        ScTemplateName = mapping.CmsTemplate.Name,
                        ScMappingId = mapping.MappingId,
                        MappingTitle = mapping.MappingTitle,
                        LastMappedDateTime = mapping.LastMappedDateTime,
                        LastUpdatedDate = mapping.LastUpdatedDate,
                        IsMapped = mapping.LastMappedDateTime != "never",
                        RemovedFromGc = mapping.LastUpdatedDate == "Removed from GatherContent"
                    };

                    if (mappingModel.IsMapped)
                    {
                        DateTime lastMappedDate;
                        DateTime.TryParseExact(mapping.LastMappedDateTime, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastMappedDate);

                        DateTime lastUpdateDate;
                        DateTime.TryParseExact(mapping.LastUpdatedDate, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastUpdateDate);

                        mappingModel.IsHighlightingDate = lastMappedDate < lastUpdateDate;
                    }

                    model.Add(mappingModel);
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
                resultObject = new { status = "error", message = exception.Message };
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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void GetMapping(string gcTemplateId, string scMappingId)
        {
            object resultObject = null;
            try
            {
                var mappingModel = MappingManager.GetSingleMappingModel(gcTemplateId, scMappingId);

                var model = new TemplateMapViewModel
                {
                    ScMappingId = mappingModel.MappingId,
                };

                if (mappingModel.GcProject != null)
                {
                    model.GcProjectId = mappingModel.GcProject.Id;
                    model.GcProjectName = mappingModel.GcProject.Name;
                }
                var addMappingModel = new AddMappingViewModel
                {
                    DefaultLocationTitle = mappingModel.DefaultLocationTitle,
                    DefaultLocation = mappingModel.DefaultLocationId,
                    GcMappingTitle = mappingModel.MappingTitle,
                    OpenerId = "drop-tree" + Guid.NewGuid(),

                };

                if (mappingModel.GcTemplate != null)
                {
                    model.GcTemplateName = mappingModel.GcTemplate.Name;
                    addMappingModel.GcTemplateId = mappingModel.GcTemplate.Id;
                }

                if (mappingModel.CmsTemplate != null)
                {
                    addMappingModel.SelectedTemplateId = mappingModel.CmsTemplate.Id;
                }

                model.AddMappingModel = addMappingModel;

                foreach (var fieldMapping in mappingModel.FieldMappings)
                {
                    model.SelectedFields.Add(new FieldMappingViewModel
                    {
                        SitecoreTemplateId = fieldMapping.CmsTemplateId,
                        GcFieldId = fieldMapping.GcFieldId,
	                    CmsTemplateNamesAdditional = fieldMapping.CmsTemplateNamesAdditional,
	                    CmsFieldMappingItemId = fieldMapping.CmsFieldMappingItemId,
	                    CmsFieldMappingItemUrl = fieldMapping.CmsFieldMappingItemUrl,
					});
                }


                #region Available templates

                var sitecoreTemplates = new List<SitecoreTemplateViewModel>();


                var defaultTemplate = new SitecoreTemplateViewModel
                {
                    SitrecoreTemplateName = "Select sitecore template *",
                    SitrecoreTemplateId = "0",

                };
                defaultTemplate.SitecoreFields.Add(new SitecoreTemplateField
                {
                    SitecoreFieldId = "0",
                    SitrecoreFieldName = "Do not map"
                });

                sitecoreTemplates.Add(defaultTemplate);
                var availableCmsTemplates = MappingManager.GetAvailableTemplates();
                foreach (var template in availableCmsTemplates)
                {
                    var st = new SitecoreTemplateViewModel
                    {
                        SitrecoreTemplateId = template.Id,
                        SitrecoreTemplateName = template.Name
                    };
                    st.SitecoreFields.Add(new SitecoreTemplateField
                    {
                        SitecoreFieldId = "0",
                        SitrecoreFieldName = "Do not map",
                    });
                    foreach (var field in template.Fields)
                    {
                        st.SitecoreFields.Add(new SitecoreTemplateField
                        {
                            SitecoreFieldId = field.Id,
                            SitrecoreFieldName = field.Name,
                            SitecoreFieldType = field.Type
                        });
                    }
                    sitecoreTemplates.Add(st);
                }
                model.SitecoreTemplates = sitecoreTemplates;

                #endregion


                model.Rules = this.GetMapRules();
                model.GcProjects.Add(new ProjectViewModel
                {
                    Id = "0",
                    Name = "Select project *"
                });

                var projects = MappingManager.GetAllGcProjects();
                foreach (var project in projects)
                {
                    model.GcProjects.Add(new ProjectViewModel
                    {
                        Id = project.Id,
                        Name = project.Name
                    });
                }

                if (string.IsNullOrEmpty(gcTemplateId) && string.IsNullOrEmpty(scMappingId))
                {
                    model.IsEdit = false;
                }
                else
                {
                    model.IsEdit = true;
                }

                resultObject = model;
                //return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (WebException exception)
            {

                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
                // return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message };
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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void GetTemplByProjectId(string gcProjectId)
        {
            object resultObject = null;
            try
            {
                var model = new List<TemplateViewModel>();

                model.Add(new TemplateViewModel
                {
                    Id = "0",
                    Name = "Select template *"
                });

                if (gcProjectId != "0")
                {
                    var templates = MappingManager.GetTemplatesByProjectId(gcProjectId);
                    foreach (var template in templates)
                    {
                        var templateModel = new TemplateViewModel
                        {
                            Id = template.Id,
                            Name = template.Name,
                        };

                        model.Add(templateModel);
                    }
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
                resultObject = new { status = "error", message = exception.Message };
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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public void GetFieldsByTemplateId(string gcTemplateId)
        {
            object resultObject = null;
            try
            {
                var model = new List<TemplateTab>();

                if (gcTemplateId != "0")
                {
                    var tabs = MappingManager.GetFieldsByTemplateId(gcTemplateId);

                    foreach (var tab in tabs)
                    {
                        var templateTab = new TemplateTab
                        {
                            TabName = tab.TabName
                        };

                        foreach (var templateField in tab.Fields)
                        {
                            var field = new TemplateField
                            {
                                FieldId = templateField.Id,
                                FieldName = templateField.Name,
                                FieldType = templateField.Type,
                            };
                            templateTab.Fields.Add(field);
                        }
                        model.Add(templateTab);
                    }
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
                resultObject = new { status = "error", message = exception.Message };
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
        public void Post(PostMappingViewModel model)
        {
            object resultObject = null;
            try
            {
                if (model.TemplateId == null)
                {
                    resultObject = new { status = "error", message = "GatherContent template isn't selected" };
                    //return Json(new { status = "error", message = "GatherContent template isn't selected" }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var postMappingModel = new MappingModel
                    {
                        DefaultLocationId = model.DefaultLocation,
                        MappingTitle = model.GcMappingTitle,
                        MappingId = model.ScMappingId,
                        GcTemplate = new GcTemplateModel { Id = model.TemplateId },
                        CmsTemplate = new CmsTemplateModel { Id = model.SelectedTemplateId },
                        FieldMappings = GetFieldMappings(model.TemplateTabs)
                    };
                    if (model.IsEdit)
                    {
                        MappingManager.UpdateMapping(postMappingModel);
                    }
                    else
                    {
                        MappingManager.CreateMapping(postMappingModel);
                    }

                    //return new EmptyResult();
                }
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
                //return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error("GatherContent message: " + e.Message + e.StackTrace, e);
                resultObject = new { status = "error", message = e.Message };
                //return Json(new { status = "error", message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                JavaScriptSerializer js = new JavaScriptSerializer();
                if(resultObject != null)
                    Context.Response.Write(js.Serialize(resultObject));
                else
                    Context.Response.Write(String.Empty);
            }

        }

        [WebMethod(enableSession: true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json,UseHttpGet =true)]
        public void Delete(string scMappingId)
        {
            object resultObject = null;
            try
            {
                MappingManager.DeleteMapping(scMappingId);
                //return new EmptyResult();
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                resultObject = new { status = "error", message = exception.Message + " Please check your credentials" };
                //return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error("GatherContent message: " + e.Message + e.StackTrace, e);
                resultObject = new { status = "error", message = e.Message };
                //return Json(new { status = "error", message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                JavaScriptSerializer js = new JavaScriptSerializer();
                if (resultObject != null)
                    Context.Response.Write(js.Serialize(resultObject));
                else
                    Context.Response.Write(String.Empty);
            }
        }
    }
}

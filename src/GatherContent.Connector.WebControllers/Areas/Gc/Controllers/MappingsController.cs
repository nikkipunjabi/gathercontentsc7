namespace GatherContent.Connector.WebControllers.Areas.Gc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Web.Mvc;

    using IoC;

    using Managers.Interfaces;
    using Managers.Models.Mapping;

    using Models.Import;
    using Models.Mapping;

    using Sitecore.Diagnostics;

    public class MappingsController : BaseController
    {
        protected IMappingManager MappingManager;
        protected ILinkManager LinkManager;


        public MappingsController()
        {
            MappingManager = ServiceFactory.MappingManager;
            LinkManager = ServiceFactory.LinkManager;
        }


        private Dictionary<string, string> GetMapRules()
        {
            return new Dictionary<string, string>
            {
                {"text", "Single-Line Text, Multi-Line Text, Rich Text, Datetime, Date, General Link, Checkbox, Number"},
                {"section", "Single-Line Text, Multi-Line Text, Rich Text"},
                {"choice_radio", "Droptree, Checklist, Multilist, Multilist with Search, Treelist, TreelistEx"},
                {"choice_checkbox", "Checklist, Multilist, Multilist with Search, Treelist, TreelistEx"},
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

        public ActionResult Get()
        {
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

        public ActionResult GetMapping(string gcTemplateId, string scMappingId)
        {
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
                        GcFieldId = fieldMapping.GcFieldId
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

        public ActionResult GetTemplatesByProjectId(string gcProjectId)
        {
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

        public ActionResult GetFieldsByTemplateId(string gcTemplateId)
        {
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

        [HttpPost]
        public ActionResult Post(PostMappingViewModel model)
        {
            if (model.TemplateId == null)
                return Json(new { status = "error", message = "GatherContent template isn't selected" }, JsonRequestBehavior.AllowGet);
            try
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

                return new EmptyResult();
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error("GatherContent message: " + e.Message + e.StackTrace, e);

                return Json(new { status = "error", message = e.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpDelete]
        public ActionResult Delete(string scMappingId)
        {
            try
            {
                MappingManager.DeleteMapping(scMappingId);
                return new EmptyResult();
            }
            catch (WebException exception)
            {
                Log.Error("GatherContent message: " + exception.Message + exception.StackTrace, exception);
                return Json(new { status = "error", message = exception.Message + " Please check your credentials" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error("GatherContent message: " + e.Message + e.StackTrace, e);

                return Json(new { status = "error", message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// used for 'test connection' button
        /// </summary>
        /// <returns></returns>
        public ActionResult Try()
        {
            try
            {
                var data = MappingManager.GetAllGcProjects();
                return Content("OK");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(204, ex.Message);
            }
        }
    }
}


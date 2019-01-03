using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.IRepositories.Models.Mapping;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Models.ImportItems.New;
using GatherContent.Connector.Managers.Models.Mapping;
using GatherContent.Connector.Managers.Models.UpdateItems;
using GatherContent.Connector.Managers.Models.UpdateItems.New;
using GatherContent.Connector.SitecoreRepositories.Repositories;
using Constants = GatherContent.Connector.SitecoreRepositories.Repositories.Constants;

namespace GatherContent.Connector.Managers.Managers
{
	/// <summary>
	/// 
	/// </summary>
	public class UpdateManager : BaseManager, IUpdateManager
	{
		protected IItemsRepository ItemsRepository;

		protected IMappingRepository MappingRepository;

		protected IItemsService ItemsService;
		protected readonly ILogger Log;

		protected GCAccountSettings GcAccountSettings;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemsRepository"></param>
		/// <param name="mappingRepository"></param>
		/// <param name="itemsService"></param>
		/// <param name="accountsService"></param>
		/// <param name="projectsService"></param>
		/// <param name="templateService"></param>
		/// <param name="cacheManager"></param>
		/// <param name="logger"></param>
		/// <param name="gcAccountSettings"></param>
		public UpdateManager(
			IItemsRepository itemsRepository,
			IMappingRepository mappingRepository,
			IItemsService itemsService,
			IAccountsService accountsService,
			IProjectsService projectsService,
			ITemplatesService templateService,
			ICacheManager cacheManager,
			ILogger logger,
			GCAccountSettings gcAccountSettings)
			: base(accountsService, projectsService, templateService, cacheManager)
		{
			ItemsRepository = itemsRepository;

			MappingRepository = mappingRepository;

			ItemsService = itemsService;
			Log = logger ?? new NullLogger();

			GcAccountSettings = gcAccountSettings;
		}

		#region Utilities

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmsItems"></param>
		/// <returns></returns>
		private UpdateModel MapUpdateItems(IEnumerable<CmsItem> cmsItems)
		{
			var model = new UpdateModel();
			var projectsDictionary = new Dictionary<int, Project>();
			var templatesDictionary = new Dictionary<int, GCTemplate>();

			var statuses = new List<GcStatusModel>();
			var templates = new List<GcTemplateModel>();
			var projects = new List<GcProjectModel>();


			var items = new List<UpdateItemModel>();

			foreach (var cmsItem in cmsItems)
			{
				var idField = cmsItem.Fields.FirstOrDefault(f => f.TemplateField.FieldName == "GC Content Id");
				if (idField != null && !string.IsNullOrEmpty(idField.Value.ToString()))
				{
					ItemEntity entity = null;
					try
					{
						entity = ItemsService.GetSingleItem(idField.Value.ToString());
					}
					catch (WebException exception)
					{
						Log.Error("GatherContent message. Api Server error has happened during getting Item with id = " + idField.Value, exception);
						using (var response = exception.Response)
						{
							var httpResponse = (HttpWebResponse)response;
							if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
							{
								throw;
							}
						}
					}
					if (entity != null)
					{
						var gcItem = entity.Data;
						var project = GetProject(projectsDictionary, gcItem.ProjectId);
						if (project != null)
						{
							if (projects.All(i => i.Id != project.Id.ToString()))
							{
								projects.Add(new GcProjectModel
								{
									Id = project.Id.ToString(),
									Name = project.Name
								});
							}
						}
						if (gcItem.TemplateId.HasValue)
						{
							var template = GetTemplate(templatesDictionary, gcItem.TemplateId.Value);

							if (templates.All(i => i.Id != template.Id.ToString()))
							{
								templates.Add(new GcTemplateModel
								{
									Id = template.Id.ToString(),
									Name = template.Name,
								});
							}


							string gcLink = null;
							if (!string.IsNullOrEmpty(GcAccountSettings.GatherContentUrl))
							{
								gcLink = GcAccountSettings.GatherContentUrl + "/item/" + gcItem.Id;
							}
							var dateFormat = GcAccountSettings.DateFormat;
							if (string.IsNullOrEmpty(dateFormat))
							{
								dateFormat = Constants.DateFormat;
							}
							var cmsLink =
								string.Format(
									"http://{0}/sitecore/shell/Applications/Content Editor?fo={1}&sc_content=master&sc_bw=1",
									HttpContext.Current.Request.Url.Host, cmsItem.Id);


							var lastUpdate = new DateTime();
							string cmsTemplateName = null;
							var lastUpdateField = cmsItem.Fields.FirstOrDefault(f => f.TemplateField.FieldName == "Last Sync Date");
							if (lastUpdateField != null)
							{
								lastUpdate = (DateTime)lastUpdateField.Value;
							}

							var cmsTemplateNameField = cmsItem.Fields.FirstOrDefault(f => f.TemplateField.FieldName == "Template");
							if (cmsTemplateNameField != null)
							{
								cmsTemplateName = cmsTemplateNameField.Value.ToString();
							}

							var status = gcItem.Status.Data;

							if (statuses.All(i => i.Id != status.Id))
							{
								statuses.Add(new GcStatusModel
								{
									Id = status.Id,
									Name = status.Name,
									Color = status.Color,
									ProjectId = gcItem.ProjectId.ToString()
								});
							}

							var listItem = new UpdateItemModel
							{
								CmsId = cmsItem.Id,
								Title = cmsItem.Title,
								CmsLink = cmsLink,
								GcLink = gcLink,
								LastUpdatedInCms = lastUpdate.ToString(dateFormat),
								Project = new GcProjectModel { Name = project.Name },
								CmsTemplate = new CmsTemplateModel { Name = cmsTemplateName },
								GcTemplate = new GcTemplateModel
								{
									Id = template.Id.ToString(),
									Name = template.Name
								},
								Status = new GcStatusModel
								{
									Id = status.Id,
									Name = status.Name,
									Color = status.Color
								},
								GcItem = new GcItemModel
								{
									Id = gcItem.Id.ToString(),
									Title = gcItem.Name,
									LastUpdatedInGc = TimeZoneInfo.ConvertTimeFromUtc(gcItem.Updated.Date, TimeZoneInfo.Local).ToString(dateFormat),
								}
							};

							items.Add(listItem);

						}
					}
				}
			}

			items = items.OrderBy(item => item.Status.Name).ToList();

			model.Items = items;
			model.Filters = new UpdateFiltersModel
			{
				Projects = projects,
				Statuses = statuses,
				Templates = templates
			};

			return model;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="templates"></param>
		/// <param name="templateId"></param>
		/// <returns></returns>
		private GCTemplate GetTemplate(Dictionary<int, GCTemplate> templates, int templateId)
		{
			GCTemplate template;
			templates.TryGetValue(templateId, out template);

			if (template == null)
			{
				template = GetGcTemplateEntity(templateId.ToString()).Data;
				templates.Add(templateId, template);
			}

			return template;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="projects"></param>
		/// <param name="projectId"></param>
		/// <returns></returns>
		private Project GetProject(Dictionary<int, Project> projects, int projectId)
		{
			Project project;
			projects.TryGetValue(projectId, out project);

			if (project == null)
			{
				project = GetGcProjectEntity(projectId.ToString()).Data;
				projects.Add(projectId, project);
			}

			return project;
		}

		private object GetValue(IEnumerable<Element> fields)
		{
			string value = string.Join("", fields.Select(i => i.Value));
			return value;
		}

		private List<string> GetOptions(IEnumerable<Element> fields)
		{
			var result = new List<string>();
			foreach (Element field in fields)
			{
				if (field.Options != null)
					result.AddRange(field.Options.Where(x => x.Selected).Select(x => x.Label));
			}
			return result;
		}



		#endregion


		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="languageId"></param>
		/// <returns></returns>
		public UpdateModel GetItemsForUpdate(string itemId, string languageId)
		{
			var cmsItems = ItemsRepository.GetItems(itemId, languageId).ToList();
			var model = MapUpdateItems(cmsItems);
			return model;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="models"></param>
		/// <param name="language"></param>
		/// <returns></returns>
		public List<ItemResultModel> UpdateItems(string itemId, List<UpdateListIds> models, string language)
		{
			var model = new List<ItemResultModel>();

			var gcItems = new Dictionary<GCItem, string>();

			foreach (var item in models)
			{
				GCItem gcItem = ItemsService.GetSingleItem(item.GCId).Data;
				gcItems.Add(gcItem, item.CMSId);
			}

			//var templates = MappingRepository.GetMappings();
			var templatesDictionary = new Dictionary<int, GCTemplate>();

			foreach (var item in gcItems)
			{
				GCItem gcItem = item.Key; //gc item
				string cmsId = item.Value; // corresponding cms id
				var itemResponseModel = new ItemResultModel
				{
					IsImportSuccessful = true,
					ImportMessage = "Update Successful"
				};
				if (!string.IsNullOrEmpty(GcAccountSettings.GatherContentUrl))
				{
					itemResponseModel.GcLink = string.Concat(GcAccountSettings.GatherContentUrl, "/item/", gcItem.Id);
				}
				itemResponseModel.GcItem = new GcItemModel
				{
					Id = gcItem.Id.ToString(),
					Title = gcItem.Name
				};

				itemResponseModel.Status = new GcStatusModel
				{
					Color = gcItem.Status.Data.Color,
					Name = gcItem.Status.Data.Name,
				};

				GCTemplate gcTemplate;
				int templateId = gcItem.TemplateId.Value;
				templatesDictionary.TryGetValue(templateId, out gcTemplate);
				if (gcTemplate == null)
				{
					gcTemplate = TemplatesService.GetSingleTemplate(templateId.ToString()).Data;
					templatesDictionary.Add(templateId, gcTemplate);
				}

				itemResponseModel.GcTemplate = new GcTemplateModel
				{
					Id = gcTemplate.Id.ToString(),
					Name = gcTemplate.Name
				};
				var cmsLink = ItemsRepository.GetCmsItemLink(HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, cmsId);
				itemResponseModel.CmsLink = cmsLink;
				itemResponseModel.CmsId = cmsId;

				//MappingResultModel cmsItem;
				//TryMapItem(gcItem, gcTemplate, templates, out cmsItem);
				//result.Add(cmsItem);
				List<Element> gcFields = gcItem.Config.SelectMany(i => i.Elements).ToList();

				//var templateMapping = templates.First(x => x.GcTemplate.GcTemplateId == gcItem.TemplateId.ToString());

				var templateMapping = MappingRepository.GetMappingByItemId(cmsId, language);
				if (templateMapping != null) // template found, now map fields here
				{
					var gcContentIdField = templateMapping.FieldMappings.FirstOrDefault(fieldMapping => fieldMapping.CmsField.TemplateField.FieldName == "GC Content Id");
					if (gcContentIdField != null)
					{
						templateMapping.FieldMappings.Remove(gcContentIdField);
					}

					var files = new List<File>();
					if (gcItem.Config.SelectMany(config => config.Elements).Any(element => element.Type == "files"))
					{
						foreach (var file in ItemsService.GetItemFiles(gcItem.Id.ToString()).Data)
						{
							files.Add(new File
							{
								FileName = file.FileName,
								Url = file.Url,
								FieldId = file.Field,
								UpdatedDate = file.Updated
							});
						}
					}

					bool fieldError = CheckFieldError(templateMapping, gcFields, files, itemResponseModel);

					if (!fieldError)
					{
						var cmsContentIdField = new FieldMapping
						{
							CmsField = new CmsField
							{
								TemplateField = new CmsTemplateField { FieldName = "GC Content Id" },
								Value = gcItem.Id.ToString()
							}
						};
						templateMapping.FieldMappings.Add(cmsContentIdField);

						var cmsItem = new CmsItem
						{
							Template = templateMapping.CmsTemplate,
							Title = gcItem.Name,
							Fields = templateMapping.FieldMappings.Select(x => x.CmsField).ToList(),
							Language = language,
							Id = cmsId
						};

						var fieldMappings = templateMapping.FieldMappings;

						ItemsRepository.AddNewVersion(cmsItem);

						// one CMS text field can be mapped to several GC fields
						// in this case we concatenate their texts and put into one CMS field
						foreach (IGrouping<string, FieldMapping> fields in fieldMappings.GroupBy(f => f.CmsField.TemplateField.FieldName))
						{
							var fieldMappingsAdditionals = fields
								.Where(f => f.CmsFieldAdditionalMapping != null &&
											f.CmsFieldAdditionalMapping.Any()).ToList()
								.SelectMany(f => f.CmsFieldAdditionalMapping).ToList();

							FieldMapping field = fields.First();
							if (field.GcField != null)
							{
								foreach (var fieldMappingsAdditional in fieldMappingsAdditionals)
								{
									fieldMappingsAdditional.Value = field.CmsField.Value;
									fieldMappingsAdditional.Files = field.CmsField.Files;
									fieldMappingsAdditional.Options = field.CmsField.Options;
								}

								switch (field.GcField.Type)
								{
									case "choice_radio":
									case "choice_checkbox":
										{
											ItemsRepository.MapChoice(cmsItem, field.CmsField);
											foreach (var fieldMappingsAdditional in fieldMappingsAdditionals)
											{
												ItemsRepository.MapChoice(cmsItem, fieldMappingsAdditional);
											}
										}
										break;
									case "files":
										{
											ItemsRepository.ResolveAttachmentMapping(cmsItem, field.CmsField);
											foreach (var fieldMappingsAdditional in fieldMappingsAdditionals)
											{
												ItemsRepository.ResolveAttachmentMapping(cmsItem, fieldMappingsAdditional);
											}
										}
										break;
									default:
										{
											if (field.CmsField.TemplateField.FieldType == "Datetime" || field.CmsField.TemplateField.FieldType == "Date")
											{
												ItemsRepository.MapDateTime(cmsItem, field.CmsField);
												foreach (var fieldMappingsAdditional in fieldMappingsAdditionals)
												{
													ItemsRepository.MapDateTime(cmsItem, fieldMappingsAdditional);
												}
											}
											else
											{
												if (fields.Count() > 1)
												{
													var newValue = string.Join("\r\n", fields.Select(f => f.CmsField.Value.ToString()));
													field.CmsField.Value = newValue;
													fieldMappingsAdditionals.ForEach(f => f.Value = newValue);
												}

												ItemsRepository.MapText(cmsItem, field.CmsField);
												foreach (var fieldMappingsAdditional in fieldMappingsAdditionals)
												{
													ItemsRepository.MapText(cmsItem, fieldMappingsAdditional);
												}
											}
										}
										break;
								}
							}
						}

                        //Publish Parent Image Folder with SubItems?

                        string parentMediaFolder = SimpleMediaRepository.publishParentMediaFolder;
                        ItemsRepository.PublishItems(parentMediaFolder, isMediaFolder: true);
                        ItemsRepository.PublishItems(cmsItem.Id, isContentItem: true);

                        var cmsSyncDateField = new CmsField
						{
							TemplateField = new CmsTemplateField { FieldName = "Last Sync Date" },
							Value = DateTime.UtcNow.ToString("yyyyMMddTHHmmss")
						};

						ItemsRepository.MapText(cmsItem, cmsSyncDateField);
					}

				}
				else
				{
					//no template mapping, set error message
					itemResponseModel.ImportMessage = "Update failed: Template not mapped";
					itemResponseModel.IsImportSuccessful = false;
				}
				model.Add(itemResponseModel);

			}

			return model;
		}

		private bool CheckFieldError(TemplateMapping templateMapping, List<Element> gcFields, List<File> files, ItemResultModel itemResponseModel)
		{
			bool fieldError = false;

			var groupedFields = templateMapping.FieldMappings.GroupBy(i => i.CmsField);

			foreach (var grouping in groupedFields)
			{
				CmsField cmsField = grouping.Key;

				var gcFieldIds = grouping.Select(i => i.GcField.Id);
				var gcFieldsToMap = grouping.Select(i => i.GcField);

				IEnumerable<Element> gcFieldsForMapping =
					gcFields.Where(i => gcFieldIds.Contains(i.Name)).ToList();

				var gcField = gcFieldsForMapping.FirstOrDefault();

				if (gcField != null)
				{
					var value = GetValue(gcFieldsForMapping);
					var options = GetOptions(gcFieldsForMapping);

					cmsField.Files = files.Where(x => x.FieldId == gcField.Name).ToList();
					cmsField.Value = value;
					cmsField.Options = options;

					//update GC fields' type
					foreach (var field in gcFieldsToMap)
					{
						field.Type = gcField.Type;
					}
				}
				else
				{
					//if field error, set error message
					itemResponseModel.ImportMessage = "Update failed: Template fields mismatch";
					itemResponseModel.IsImportSuccessful = false;
					fieldError = true;
					break;
				}
			}
			return fieldError;
		}
	}
}

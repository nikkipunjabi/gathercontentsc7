using System;
using System.IO;
using System.Linq;
using System.Net;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using File = GatherContent.Connector.IRepositories.Models.Import.File;

namespace GatherContent.Connector.SitecoreRepositories.Repositories
{
    public class SimpleMediaRepository : BaseSitecoreRepository, IMediaRepository<Item>
    {
        protected readonly string MediaFolderRoot;

        protected readonly string DealsFolderRoot;

        protected readonly string EventsFolderRoot;

        public static string publishParentMediaFolder { get; set; }

        public SimpleMediaRepository()
        {
            var path = Sitecore.Configuration.Settings.GetSetting("GatherContent.MediaLibrary.MainFolder");
            var dealsPath = Sitecore.Configuration.Settings.GetSetting("GatherContent.MediaLibrary.DealsFolder");
            var eventsPath = Sitecore.Configuration.Settings.GetSetting("GatherContent.MediaLibrary.EventFolder");
            if (!string.IsNullOrWhiteSpace(path))
            {
                MediaFolderRoot = path.TrimEnd('/');
            }
            else
            {
                MediaFolderRoot = "/sitecore/media library/GatherContent";
            }

            if (!string.IsNullOrWhiteSpace(dealsPath))
            {
                DealsFolderRoot = dealsPath.TrimEnd('/');
            }
            else
            {
                DealsFolderRoot = "/sitecore/media library/GatherContent";
            }

            if (!string.IsNullOrWhiteSpace(eventsPath))
            {
                EventsFolderRoot = eventsPath.TrimEnd('/');
            }
            else
            {
                EventsFolderRoot = "/sitecore/media library/GatherContent";
            }
        }

        public Item UploadFile(string targetPath, File fileInfo, string altText)
        {
            string uri = fileInfo.Url.StartsWith("http") ? fileInfo.Url : "https://gathercontent.s3.amazonaws.com/" + fileInfo.Url;

            string extension = string.Empty;
            if (fileInfo.FileName.Contains("."))
            {
                extension = fileInfo.FileName.Substring(fileInfo.FileName.LastIndexOf('.') + 1);
            }

            var request = (HttpWebRequest)WebRequest.Create(uri);
            var resp = (HttpWebResponse)request.GetResponse();
            var stream = resp.GetResponseStream();

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                if (stream != null)
                {
                    stream.CopyTo(memoryStream);
                }

                if (memoryStream.Length > 0)
                {
                    altText = ItemUtil.ProposeValidItemName(altText);
                    var media = CreateMedia(targetPath, fileInfo, extension, memoryStream, altText);
                    return media;
                }

            }

            return null;
        }

        public virtual string ResolveMediaPath(CmsItem item, Item createdItem, CmsField cmsField)
        {
            string path;

            //Field scField = createdItem.Fields[new ID(cmsField.TemplateField.FieldId)];
            //string dataSourcePath = GetItem(scField.ID.ToString())["Source"];
            //if (!string.IsNullOrEmpty(dataSourcePath) && GetItem(dataSourcePath) != null)
            //{
            //	path = dataSourcePath;
            //}
            //else
            //{

            if (string.Equals(item.Template.TemplateName.ToLower(), "deal"))
            {
                publishParentMediaFolder = string.Format(DealsFolderRoot + "/{0}/", ItemUtil.ProposeValidItemName(item.Title));

                path = string.IsNullOrEmpty(cmsField.TemplateField.FieldName)
                    ? string.Format(DealsFolderRoot + "/{0}/", ItemUtil.ProposeValidItemName(item.Title))
                    : string.Format(DealsFolderRoot + "/{0}/{1}/", ItemUtil.ProposeValidItemName(item.Title), ItemUtil.ProposeValidItemName(cmsField.TemplateField.FieldName));

                SetDatasourcePath(createdItem, cmsField.TemplateField.FieldId, DealsFolderRoot);
            }
            else if (string.Equals(item.Template.TemplateName.ToLower(), "event"))
            {
                publishParentMediaFolder = string.Format(EventsFolderRoot + "/{0}/", ItemUtil.ProposeValidItemName(item.Title));

                path = string.IsNullOrEmpty(cmsField.TemplateField.FieldName)
                    ? string.Format(EventsFolderRoot + "/{0}/", ItemUtil.ProposeValidItemName(item.Title))
                    : string.Format(EventsFolderRoot + "/{0}/{1}/", ItemUtil.ProposeValidItemName(item.Title), ItemUtil.ProposeValidItemName(cmsField.TemplateField.FieldName));

                SetDatasourcePath(createdItem, cmsField.TemplateField.FieldId, EventsFolderRoot);
            }
            else
            {
                publishParentMediaFolder = string.Format(MediaFolderRoot + "/{0}/", ItemUtil.ProposeValidItemName(item.Title));

                path = string.IsNullOrEmpty(cmsField.TemplateField.FieldName)
                    ? string.Format(MediaFolderRoot + "/{0}/", ItemUtil.ProposeValidItemName(item.Title))
                    : string.Format(MediaFolderRoot + "/{0}/{1}/", ItemUtil.ProposeValidItemName(item.Title), ItemUtil.ProposeValidItemName(cmsField.TemplateField.FieldName));

                SetDatasourcePath(createdItem, cmsField.TemplateField.FieldId, MediaFolderRoot);
            }
            //}

            return path;
        }

        protected virtual Item CreateMedia(string rootPath, File mediaFile, string extension, Stream mediaStream, string altText)
        {
            using (new SecurityDisabler())
            {
                var validItemName = ItemUtil.ProposeValidItemName(mediaFile.FileName);

                var filesFolder = GetItemByPath(rootPath);
                if (filesFolder != null)
                {
                    var files = filesFolder.Children;
                    var item = files.FirstOrDefault(f => f.Name == validItemName &&
                                                         DateUtil.IsoDateToDateTime(f.Fields["__Created"].Value) >=
                                                         mediaFile.UpdatedDate);
                    if (item != null)
                    {
                        return item;
                    }
                }

                var mediaOptions = new MediaCreatorOptions
                {
                    Database = ContextDatabase,
                    FileBased = false,
                    IncludeExtensionInItemName = false,
                    KeepExisting = true,
                    Versioned = false,
                    Destination = string.Concat(rootPath, "/", validItemName),
                };

                var previewImgItem = MediaManager.Creator.CreateFromStream(mediaStream, validItemName + "." + extension, mediaOptions);
                //mediaOptions.AlternateText -- This is not working on the PROD.
                //So forciing ALT Update using below code --- This works on the PROD.
                //Update ALT Text
                previewImgItem.Editing.BeginEdit();
                previewImgItem["Alt"] = altText;
                previewImgItem.Editing.EndEdit();
                return previewImgItem;
            }
        }

        protected void SetDatasourcePath(Item updatedItem, string fieldId, string path)
        {
            var scField = updatedItem.Fields[new ID(fieldId)];
            var scItem = GetItem(scField.ID.ToString());

            if (string.IsNullOrEmpty(scItem["Source"]))
            {
                using (new SecurityDisabler())
                {
                    scItem.Editing.BeginEdit();
                    scItem["Source"] = path;
                    scItem.Editing.EndEdit();
                }
            }
        }

        public void CleanUp(string targetPath)
        {
            var mediaPath = ContextDatabase.GetItem(targetPath);

            if (mediaPath != null)
            {

                var allDescendants = mediaPath.Axes.GetDescendants();
                if (allDescendants != null && allDescendants.Any())
                {
                    foreach (var item in allDescendants)
                    {
                        Globals.LinkDatabase.UpdateItemVersionReferences(item);
                        var itemLisnks = Globals.LinkDatabase.GetReferrers(item);
                        bool anyLinkedItem = itemLisnks.Any(l => l.TargetItemID.ToString().ToLower() == item.ID.ToString().ToLower());
                        if (!anyLinkedItem)
                        {
                            using (new SecurityDisabler())
                            {
                                item.Delete();
                            }
                        }
                    }
                }
            }
        }
    }
}
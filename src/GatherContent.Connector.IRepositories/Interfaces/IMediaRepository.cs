﻿using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    public interface IMediaRepository<T>: IRepository where T: class 
    {
        T UploadFile(string targetPath, File fileInfo, string altText);

        string ResolveMediaPath(CmsItem item, T createdItem, CmsField field);

        void CleanUp(string targetPath);
    }
}

using System;
using GatherContent.Connector.Entities;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Managers;
using Sitecore.Data.Items;

namespace GatherContent.Connector.WebControllers.IoC
{
    public static class ServiceFactory
    {
        private static IAccountsRepository AccountsRepository
        {
            get
            {
                return Sitecore.Configuration.Factory.CreateObject("gatherContent.connector/components/accountsRepository", true) as IAccountsRepository;
            }
        }

        public static GCAccountSettings Settings
        {
            get { return AccountsRepository.GetAccountSettings(); }
        }

        private static IItemsRepository ItemsRepository
        {
            get
            {
                return Sitecore.Configuration.Factory.CreateObject("gatherContent.connector/components/itemsRepository", true) as IItemsRepository;
            }
        }

        private static IMediaRepository<Item> MediaRepository
        {
            get
            {
                return Sitecore.Configuration.Factory.CreateObject("gatherContent.connector/components/mediaRepository", true) as IMediaRepository<Item>;
            }
        }

        private static IMappingRepository MappingRepository
        {
            get
            {
                return Sitecore.Configuration.Factory.CreateObject("gatherContent.connector/components/mappingRepository", true) as IMappingRepository;
            }
        }

        private static IDropTreeRepository DropTreeRepository
        {
            get
            {
                return Sitecore.Configuration.Factory.CreateObject("gatherContent.connector/components/dropTreeRepository", true) as IDropTreeRepository;
            }
        }

        public static IMappingManager MappingManager
        {
            get
            {
                return new MappingManager(
                    MappingRepository,
                    new AccountsService(Settings), 
                    new ProjectsService(Settings),
                    new TemplatesService(Settings),
                    new ItemsService(Settings),
                    new CacheManager(), 
                    new ScLogger(), 
                    Settings
                    );
            }
        }

        public static ILinkManager LinkManager
        {
            get
            {
                return new LinkManager(ItemsRepository);
            }
        }

        public static IImportManager ImportManager
        {
            get
            {
                return new ImportManager(
                    ItemsRepository,
                    MappingRepository,
                    new ItemsService(Settings), 
                    new AccountsService(Settings),
                    new ProjectsService(Settings),
                    new TemplatesService(Settings),
                    new CacheManager(), 
                    Settings);
            }
        }

        public static IDropTreeManager DropTreeManager
        {
            get
            {
                return new DropTreeManager(
                    DropTreeRepository,
                    AccountsRepository,
                    Settings);
            }
        }

        public static IUpdateManager UpdateManager
        {
            get
            {
                return new UpdateManager(
                    ItemsRepository,
                    MappingRepository,
                    new ItemsService(Settings),
                    new AccountsService(Settings),
                    new ProjectsService(Settings),
                    new TemplatesService(Settings),
                    new CacheManager(), 
                    new ScLogger(), 
                    Settings);
            }
        }
    }

    public class ScLogger : ILogger
    {
        public void Warn(string message, object sender)
        {
            Sitecore.Diagnostics.Log.Warn(message, sender);
        }

        public void Error(string message, Exception exception)
        {
            Sitecore.Diagnostics.Log.Error(message, exception, this);
        }
    }
}

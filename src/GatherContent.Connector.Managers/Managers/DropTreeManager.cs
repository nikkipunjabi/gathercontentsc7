using System.Linq;
using GatherContent.Connector.Entities;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.Managers.Models.ImportItems;
using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.Managers.Interfaces;

namespace GatherContent.Connector.Managers.Managers
{
    public class DropTreeManager : IDropTreeManager
    {
        protected IDropTreeRepository DropTreeRepository;
        protected IAccountsRepository AccountsRepository;
        protected GCAccountSettings GcAccountSettings;

        public DropTreeManager(IDropTreeRepository dropTreeRepository, IAccountsRepository accountsRepository, GCAccountSettings gcAccountSettings)
        {
            AccountsRepository = accountsRepository;
            GcAccountSettings = gcAccountSettings;
            DropTreeRepository = dropTreeRepository;
        }

        private List<DropTreeModel> CreateChildrenTree(string id, List<CmsItem> items)
        {
            var list = new List<DropTreeModel>();

            if (items.Select(i => i.Id.ToString()).Contains(id))
            {
                foreach (var item in items)
                {
                    if (id == item.Id)
                    {
                        var node = new DropTreeModel
                        {
                            title = item.Title,
                            key = item.Id,
                            isLazy = true,
                            icon = item.Icon,
                            selected = true
                        };
                        list.Add(node);
                    }
                    else
                    {
                        var node = new DropTreeModel
                        {
                            title = item.Title,
                            key = item.Id,
                            isLazy = true,
                            icon = item.Icon,
                            selected = false
                        };
                        list.Add(node);
                    }
                }
            }
            else
            {
                foreach (var item in items)
                {
                    var node = new DropTreeModel
                    {
                        title = item.Title,
                        key = item.Id,
                        isLazy = false,
                        icon = item.Icon,
                        selected = false,
                        children = CreateChildrenTree(id, item.Children),
                        expanded = true
                    };
                    list.Add(node);
                }
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<DropTreeModel> GetTopLevelNode(string id)
        {
            var model = new List<DropTreeModel>();
            var items = DropTreeRepository.GetHomeNode(id);

            if (string.IsNullOrEmpty(id) || id == "null")
            {
                foreach (var cmsItem in items)
                {
                    model.Add(new DropTreeModel
                    {
                        title = cmsItem.Title,
                        key = cmsItem.Id,
                        icon = cmsItem.Icon,
                        isLazy = true
                    });
                }
            }
            else
            {
             
               var dropTreeHomeNode = DropTreeRepository.GetHomeNodeId();

                foreach (var cmsItem in items)
                {
                    model.Add(new DropTreeModel
                    {
                        title = cmsItem.Title,
                        key = cmsItem.Id,
                        icon = cmsItem.Icon,
                        isLazy = false,
                        selected = id == dropTreeHomeNode,
                        expanded = true,
                        children = CreateChildrenTree(id, cmsItem.Children),
                    });
                }
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<DropTreeModel> GetChildrenNodes(string id)
        {
            var model = new List<DropTreeModel>();
            var items = DropTreeRepository.GetChildren(id);
            foreach (var cmsItem in items)
            {
                model.Add(new DropTreeModel
                {
                    title = cmsItem.Title,
                    key = cmsItem.Id,
                    icon = cmsItem.Icon,
                    isLazy = true
                });
            }

            return model;
        }
    }
}

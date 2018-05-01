namespace GatherContent.Connector.Managers.Managers
{
    using Interfaces;
    using IRepositories.Interfaces;

    public class LinkManager : ILinkManager, IManager
    {
        private readonly IItemsRepository _itemsRepository;

        public LinkManager(IItemsRepository itemsRepository)
        {
            _itemsRepository = itemsRepository;
        }

        public string GetLinkedItemUrl(int gcId)
        {
            return _itemsRepository.GetLinkedItemUrl(gcId);
        }

        public void ExpandLinksInText(string cmsRootId, bool includeDescendants)
        {
            _itemsRepository.ExpandLinksInText(cmsRootId, includeDescendants);
        }
    }
}
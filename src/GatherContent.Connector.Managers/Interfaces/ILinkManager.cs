namespace GatherContent.Connector.Managers.Interfaces
{
    public interface ILinkManager
    {
        string GetLinkedItemUrl(int gcId);

        void ExpandLinksInText(string cmsRootId, bool includeDescendants);
    }
}
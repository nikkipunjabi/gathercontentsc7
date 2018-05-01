using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.Managers.Interfaces;

namespace GatherContent.Connector.Managers.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class TestConnectionManager : BaseManager
    {
        public TestConnectionManager(
            IAccountsService accountsService,
            IProjectsService projectsService,
            ITemplatesService templateService,
            ICacheManager cacheManager) : base(accountsService, projectsService, templateService, cacheManager)
        {
        }

        public bool TestConnection()
        {
            try
            {
                AccountsService.GetAccounts();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}

namespace GatherContent.Connector.WebControllers.Areas.Gc
{
    using System.Web.Mvc;

    public class GcAreaRegistration : AreaRegistration
    {
        public override void RegisterArea(AreaRegistrationContext context)
        {
            
        }

        public override string AreaName
        {
            get { return "GC"; }
        }
    }
}

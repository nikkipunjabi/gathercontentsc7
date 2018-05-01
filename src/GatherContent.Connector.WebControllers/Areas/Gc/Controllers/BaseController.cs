namespace GatherContent.Connector.WebControllers.Areas.Gc.Controllers
{
    using System.Collections.Generic;

    using Models.Import;

    using Sitecore.Mvc.Controllers;

    public class BaseController : SitecoreController
    {
        /// <summary>
        /// 
        /// </summary>
        public BaseController()
        {
        }

        public List<LanguageViewModel> GetLanguages(string databse) 
        {
            var model = new List<LanguageViewModel>();
            var database = Sitecore.Configuration.Factory.GetDatabase(databse);
            var languages = database.GetLanguages();

            foreach (var language in languages)
            {
                model.Add(new LanguageViewModel
                {
                    Name = language.CultureInfo.DisplayName,
                    IsoCode = language.CultureInfo.Name
                });
            }
            return model;
        }
    }
}
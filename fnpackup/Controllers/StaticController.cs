
using Microsoft.AspNetCore.Mvc;
namespace fnpackup.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StaticController : BaseController
    {
        private readonly DynamicFileProvider dynamicFileProvider;
        public StaticController(DynamicFileProvider dynamicFileProvider)
        {
            this.dynamicFileProvider = dynamicFileProvider;
        }

        [HttpGet]
        public List<FileProviderInfo> List()
        {
            return dynamicFileProvider.List();
        }
        [HttpPost]
        public async Task Search()
        {
            await dynamicFileProvider.Search();
        }
    }
}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MappingProject.Startup))]
namespace MappingProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

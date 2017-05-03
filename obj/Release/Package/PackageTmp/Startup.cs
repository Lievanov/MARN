using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LaboratorioMarn.Startup))]
namespace LaboratorioMarn
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

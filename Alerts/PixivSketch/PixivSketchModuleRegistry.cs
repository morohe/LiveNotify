using Autofac;

namespace PixivSketch
{
    public class PixivSketchModuleRegistry
        : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<LiveNotify.Models.Alerts.PixivSketch>().As<LiveNotify.Models.Alerts.IAlertModel>();
        }
    }
}

using Autofac;

namespace TestAlert
{
    public class TestAlertModuleRegistry
        : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<LiveNotify.Models.Alerts.TestAlert>().As<LiveNotify.Models.Alerts.IAlertModel>();
        }
    }
}

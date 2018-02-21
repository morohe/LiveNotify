using Autofac;
using Hardcodet.Wpf.TaskbarNotification;
using Prism.Events;
using System.Text;

namespace LiveNotify.Views
{
    public partial class SystemTrayView
    {
        public SystemTrayView(IContainer container)
        {
            InitializeComponent();

            container.Resolve<Models.NotifyBalloonMessage>().GetEvent<PubSubEvent<string>>().Subscribe(x =>
            {
                ShowBalloonTip("New live stream arrived", x, BalloonIcon.Info);
            });
        }
    }
}

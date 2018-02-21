using Autofac;
using Prism.Mvvm;
using Reactive.Bindings;
using System;

namespace LiveNotify.ViewModels
{
    public class SetupViewModel : BindableBase
    {
        private IContainer _Container = null;

        public SetupViewModel(IContainer container)
        {
            _Container = container;
            CheckInterval = new ReactiveProperty<int>((int)_Container.Resolve<Models.AlertManager>().CheckInterval.TotalMinutes);

            ApplyCommand = new ReactiveCommand();
            ApplyCommand.Subscribe(() =>
            {
                var manager = _Container.Resolve<Models.AlertManager>();
                manager.CheckInterval = TimeSpan.FromMinutes(CheckInterval.Value);
                manager.Save();
            });

            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(() =>
            {
                CheckInterval.Value = (int)_Container.Resolve<Models.AlertManager>().CheckInterval.TotalMinutes;
            });
#if DEBUG
            var conf = container.Resolve<Models.LocalTestConfigulator>();
            if (conf.IsLocalTest)
            {
                IntervalMinimum = 1;
            }
#endif
        }

        public int IntervalMinimum { get; } = 10;
        public int IntervalMaximum { get; } = 60;

        public ReactiveProperty<int> CheckInterval { get; }

        public ReactiveCommand ApplyCommand { get; }

        public ReactiveCommand CancelCommand { get; }
    }
}

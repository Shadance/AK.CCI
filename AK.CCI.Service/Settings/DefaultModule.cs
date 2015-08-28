using Ninject.Modules;

namespace AK.CCI.Service.Settings
{
	public class DefaultModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IConfiguration>().To<Configuration>().InSingletonScope();
			Bind<IStrategyManager>().To<StrategyManager>().InSingletonScope();

			//Bind<IConnectorManager>().To<RandConnectorManager>().InSingletonScope();
			Bind<IConnectorManager>().To<QuikConnectorManager>().InSingletonScope();
			//Bind<IConnectorManager>().To<QuikEmulationConnectorManager>().InSingletonScope();

			//Bind<IStrategy>().To<CCIStrategy>();
			Bind<IStrategy>().To<QStrategy>();
			Bind<IStrategyConfiguration>().To<StrategyConfiguration>();
		}
	}
}
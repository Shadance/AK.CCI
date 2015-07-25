using Ninject.Modules;

namespace AK.CCI.Service
{
	public class DefaultModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IConfiguration>().To<Configuration>().InSingletonScope();
			Bind<IStrategyManager>().To<StrategyManager>().InSingletonScope();
			Bind<IConnectionManager>().To<QuikConnectionManager>().InSingletonScope();

			Bind<IStrategy>().To<CCIStrategy>();
			Bind<IStrategyConfiguration>().To<StrategyConfiguration>();
		}
	}
}
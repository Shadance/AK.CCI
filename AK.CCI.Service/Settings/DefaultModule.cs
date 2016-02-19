using AK.CCI.Service.Strategies;
using Ninject.Modules;

namespace AK.CCI.Service.Settings
{
	public class DefaultModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IConfiguration>().To<Configuration>().InSingletonScope();

            //Bind<IConnectorManager>().To<RandConnectorManager>().InSingletonScope();
//            Bind<IConnectorManager>().To<QuikConnectorManager>().InSingletonScope();
            Bind<IConnectorManager>().To<AlfaConnectorManager>().InSingletonScope();
            //Bind<IConnectorManager>().To<QuikEmulationConnectorManager>().InSingletonScope();
            
            //Bind<AKStrategy>().To<CCIStrategy>();
            Bind<AKStrategy>().To<QStrategy>();
            Bind<IStrategyManager>().To<StrategyManager>().InSingletonScope();
           //			Bind<IStrategyConfiguration>().To<StrategyConfiguration>();
        }
    }
}
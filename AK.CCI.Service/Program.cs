using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AK.CCI.Service.Settings;
using Topshelf;
using Topshelf.Ninject;

namespace AK.CCI.Service
{
	class Program
	{
		static void Main()
		{
			HostFactory.Run(c =>
			{
				c.UseNinject(new DefaultModule());

				c.Service<Service>(s =>
				{
					s.ConstructUsingNinject();
					s.WhenStarted(service => service.Start());
					s.WhenStopped(service => service.Stop());
				});

				c.SetServiceName("CCIService");
				c.SetDisplayName("AK CCI Service");
				c.SetDescription("Monitors CCI indicator and set orders.");

				c.StartAutomatically();
			});
		}
	}
}

﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using UPnPNet.Extensions;
using UPnPNet.Gena;
using UPnPNet.Services;

namespace UPnPNet
{
	public class UPnPServer
	{
		public GenaSubscriptionHandler Handler { get; }
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		public int Port { get; private set; }

		public UPnPServer()
		{
			Handler = new GenaSubscriptionHandler();
		}

		public UPnPServer(GenaSubscriptionHandler handler)
		{
			Handler = handler;
		}

		public void Start(int port, string host = "*")
		{
			Port = port;
			IWebHost webHost = new WebHostBuilder()
				.UseKestrel()
				.UseUrls("http://" + host + ":" + port)
				.Configure(app =>
				{
					app.Run(handler =>
					{
						StreamReader reader = new StreamReader(handler.Request.Body);
						IDictionary<string, string> headers =
							handler.Request.Headers.ToDictionary(
								x => x.Key,
								x => x.Value.Aggregate((prev, next) => prev + " " + next));

						Handler.HandleNotify(headers, reader.ReadToEnd());

						handler.Response.StatusCode = 200;
						return Task.FromResult(0);
					});

				})
				.Build();

			Task.Run(() => webHost.Run(_cancellationTokenSource.Token));
		}

		public void Stop()
		{
			_cancellationTokenSource.Cancel();
		}

		public async void SubscribeToControl<T>(UPnPLastChangeServiceControl<T> control)
		{
			await control.SubscribeToEvents(Handler, "http://" + control.Service.GetInterfaceToHost() + ":" + Port, 3600);
		}
	}
}

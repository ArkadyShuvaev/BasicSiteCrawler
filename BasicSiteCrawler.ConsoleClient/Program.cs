﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using BasicSiteCrawler.Library.Abstractions;
using BasicSiteCrawler.Library.Models;
using BasicSiteCrawler.Library.Services;
using Microsoft.Extensions.Logging;

namespace BasicSiteCrawler.ConsoleClient
{
	class Program
	{
		private const string ResultLog = "result.log";

		static void Main(string[] args)
		{
			var loggerFactory = new LoggerFactory().AddConsole(LogLevel.Trace);
			var logger = loggerFactory.CreateLogger<BasicCrawler>();

#if DEBUG
			//args = new[] { "http://www.bbc.com/" };
			//args = new[] { "wrongProtocol://www.bbc.com/" };
			args = new[] { "http://127.0.0.1" };
#endif

			try
			{
				if (!IsParamValid(args))
				{
					const string errMsg = "Please define a start link.";
					Console.WriteLine(errMsg);
					logger.LogError(errMsg);
					return;
				}


				MainImpl(logger, args[0]);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				logger.LogCritical(e.ToString());
				throw;
			}
		}

		private static void MainImpl(ILogger<BasicCrawler> logger, string startLink)
		{
			if (File.Exists(ResultLog))
			{
				File.Delete(ResultLog);
			}
			
			using (var clientHandler = new HttpClientHandler())
			using (var streamWriter = new StreamWriter(ResultLog))
			{
				var networkProvider = new NetworkProvider(clientHandler);
				IHtmlParser htmlParser = new SimpleHtmlParser();
				var temporaryUrlStorage = new UrlMemoryStorage();

				var simpleOutputWriter = new SimpleOutputWriter(streamWriter);
				var crawlerService = new BasicCrawler(networkProvider, logger, htmlParser, 
					temporaryUrlStorage);

				crawlerService.UrlCrawled += (sender, args) => CrawlerServiceOnUrlCrawled(args, simpleOutputWriter);
				
				crawlerService.CrawlUrl(startLink);
			}
		}

		private static void CrawlerServiceOnUrlCrawled(CrawlingUrlArgs crawlingUrlArgs, IOutputWriter streamWriter)
		{
			streamWriter.WriteLine(crawlingUrlArgs.CrawlingUrl.ToString());
		}

		private static bool IsParamValid(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				return false;
			}
			return true;
		}

	}
}

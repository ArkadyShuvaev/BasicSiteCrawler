﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BasicSiteCrawler.Abstractions;
using BasicSiteCrawler.Library.Services;
using BasicSiteCrawler.Models;
using BasicSiteCrawler.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BasicSiteCrawler.Web.Services
{
    public class CrawlMediator
    {
	    private readonly IHubContext<CrawlUrlHub> _crawlUrlHub;
	    private readonly ILogger<CrawlMediator> _logger;
	    private readonly ILoggerFactory _loggerFactory;
	    private BasicCrawler _crawlerService;

	    public CrawlMediator(IHubContext<CrawlUrlHub> crawlUrlHub, ILogger<CrawlMediator> logger, ILoggerFactory loggerFactory)
	    {
		    _crawlUrlHub = crawlUrlHub;
		    _logger = logger;
		    _loggerFactory = loggerFactory;
	    }

	    public void StartCrawl(string startUrlForCrawl)
	    {
		    
			using (var clientHandler = new HttpClientHandler())
		    {
			    var networkProvider = new NetworkProvider(clientHandler);
			    IHtmlParser htmlParser = new SimpleHtmlParser();
			    var temporaryUrlStorage = new UrlMemoryStorage();

			    _crawlerService = new BasicCrawler(networkProvider, _loggerFactory.CreateLogger<BasicCrawler>(), htmlParser,
				    temporaryUrlStorage);

			    _crawlerService.UrlCrawled += CrawlerServiceOnUrlCrawled;

			    _crawlerService.CrawlUrl(startUrlForCrawl);
				
		    }
		}

	    public void StopCrawl()
	    {
			_crawlerService.ResetSubscriptions();
	    }

	    private void CrawlerServiceOnUrlCrawled(object sender, CrawlingUrlArgs crawlingUrlArgs)
	    {
			_crawlUrlHub.Clients.All.SendAsync("Send", crawlingUrlArgs.CrawlingUrl.ToString());
		}
    }
}

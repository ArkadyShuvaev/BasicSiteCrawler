﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BasicSiteCrawler.Abstractions;
using BasicSiteCrawler.Models;

namespace BasicSiteCrawler.Services
{
	public class UrlMemoryStorage : IUrlStorage
	{
		private readonly ConcurrentBag<CrawlingUrl> _urls;

		public UrlMemoryStorage()
		{
			_urls = new ConcurrentBag<CrawlingUrl>();
		}

		public bool TryAdd(CrawlingUrlForCreation url)
		{
			if (url == null) throw new ArgumentNullException(nameof(url));

			
			var existingUrl = _urls.FirstOrDefault(u => u.LocalPath.Equals(url.LocalPath, StringComparison.CurrentCultureIgnoreCase) &&
			                                            u.Authority.Equals(url.Authority, StringComparison.CurrentCultureIgnoreCase));
			if (existingUrl != null)
			{
				return false;
			}

			var id = CreateId();
			var crawlingUrl = new CrawlingUrl
			{
				Id = id,
				LocalPath = url.LocalPath,
				Authority = url.Authority,
				Scheme = url.Scheme
			};
			_urls.Add(crawlingUrl);

			return true;
		}
		
		private int CreateId()
		{
			return _urls.Count + 1;
		}

		public IEnumerable<CrawlingUrl> GetUncrawledUrls()
		{
			return _urls.Where(u => !u.IsCrawled);
		}

		public bool IsUncrawledUrlExist => _urls.Any(u => u.IsCrawled == false);
		
		public void MarkUrlAsCrawled(int id)
		{
			if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
			var existingUrl = GetExistingUrlAndThrowIfNoExist(id);
			existingUrl.IsCrawled = true;
		}

		public void MarkUrlAsProcessed(int id)
		{
			if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
			var existingUrl = GetExistingUrlAndThrowIfNoExist(id);
			existingUrl.IsProcessed = true;
		}

		public void MarkUrlAsIncorrected(int id)
		{
			var existingUrl = GetExistingUrlAndThrowIfNoExist(id);
			existingUrl.IsIncorrected = true;
		}

		private CrawlingUrl GetExistingUrlAndThrowIfNoExist(int id)
		{
			var existingUrl = _urls.FirstOrDefault(u => u.Id == id);
			if (existingUrl == null) throw new ArgumentNullException(nameof(existingUrl));
			return existingUrl;
		}
	}
}
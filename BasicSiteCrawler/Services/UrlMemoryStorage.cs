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

		public CrawlingUrl Add(CrawlingUrlForCreation url)
		{
			if (url == null) throw new ArgumentNullException(nameof(url));

			
			var existingUrl = _urls.FirstOrDefault(u => u.Url.Equals(url.Url, StringComparison.CurrentCultureIgnoreCase));
			if (existingUrl != null)
			{
				return existingUrl;
			}

			var id = CreateId();
			var crawlingUrl = new CrawlingUrl
			{
				Id = id,
				Url = url.Url
			};
			_urls.Add(crawlingUrl);

			return crawlingUrl;
		}

		private int CreateId()
		{
			return _urls.Count + 1;
		}

		public IEnumerable<CrawlingUrl> GetUncrawledUrls()
		{
			return _urls.Where(u => !u.IsCrawled);
		}

		public bool AreUncrawledUrlsExist()
		{
			return _urls.Any(u => u.IsCrawled == false);
		}

		public bool IsCrawled(int id)
		{
			var existingUrl = _urls.FirstOrDefault(u => u.Id == id);
			return existingUrl != null && existingUrl.IsCrawled;
		}

		public void MarkUrlAsCrawled(int id)
		{
			if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

			var existingUrl = _urls.FirstOrDefault(u => u.Id == id);
			if (existingUrl == null) throw new ArgumentNullException(nameof(existingUrl));

			existingUrl.IsCrawled = true;
		}
		
		public string GetUrlAndMarkAsSaved(string scheme, int id)
		{
			var crawledItems = _urls.FirstOrDefault(u => u.Id == id);

			if (crawledItems == null)
			{
				return string.Empty;
			}

			return crawledItems.ConvertToFullUrl(scheme);
		}

		
		public IEnumerable<string> GetUrlsAndMarkAsSaved(string currentScheme)
		{
			var result = new List<string>();
			foreach (var url in _urls.Where(u => u.IsCrawled && !u.IsSaved))
			{
				result.Add(url.ConvertToFullUrl(currentScheme));
			}

			return result;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MauiApp1.Models;
using MauiApp1.Infrastructure;
using MauiApp1.Services;

namespace MauiApp1.Services
{
    public class PersonSearchService
    {
        private readonly HttpClient _http;

        public PersonSearchService(HttpClient? httpClient = null)
        {
            if (httpClient != null)
            {
                _http = httpClient;
            }
            else
            {
                // Resolve from app ServiceProvider if available
                _http = ServiceLocator.ServiceProvider?.GetService(typeof(HttpClient)) as HttpClient
                        ?? new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
            }
        }

        // Authorization is handled by AuthMessageHandler (delegating handler) configured on HttpClient

        public async Task<PagedResult<PersonSummaryDto>> SearchBaptismPagedAsync(string firstName, string lastName, int page = 1, int pageSize = 25)
        {
            var url = $"api/personsearch/baptism/paged?first={Uri.EscapeDataString(firstName)}&last={Uri.EscapeDataString(lastName)}&page={page}&pageSize={pageSize}";
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                // fallback to local cache when offline or server error
                var cache = new LocalCacheService();
                var cached = await cache.QueryByNameAsync(firstName, lastName, pageSize, (page-1)*pageSize);
                return new PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = cached.Count, Items = cached };
            }

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var paged = JsonSerializer.Deserialize<PagedResult<PersonSummaryDto>>(json, options);
            if (paged != null && paged.Items != null)
            {
                // cache fetched items locally for offline fallback
                var cache = new LocalCacheService();
                await cache.UpsertPersonsAsync(paged.Items);
            }
            return paged ?? new PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<PagedResult<PersonSummaryDto>> SearchFirstHolyCommunionPagedAsync(string firstName, string lastName, int page = 1, int pageSize = 25)
        {
            var url = $"api/personsearch/firstholycommunion/paged?first={Uri.EscapeDataString(firstName)}&last={Uri.EscapeDataString(lastName)}&page={page}&pageSize={pageSize}";
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                var cache = new LocalCacheService();
                var cached = await cache.QueryByNameAsync(firstName, lastName, pageSize, (page-1)*pageSize);
                return new PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = cached.Count, Items = cached };
            }

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var paged = JsonSerializer.Deserialize<PagedResult<PersonSummaryDto>>(json, options);
            if (paged != null && paged.Items != null)
            {
                var cache = new LocalCacheService();
                await cache.UpsertPersonsAsync(paged.Items);
            }

            return paged ?? new PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<PagedResult<PersonSummaryDto>> SearchConfirmationPagedAsync(string firstName, string lastName, int page = 1, int pageSize = 25)
        {
            var url = $"api/personsearch/confirmation/paged?first={Uri.EscapeDataString(firstName)}&last={Uri.EscapeDataString(lastName)}&page={page}&pageSize={pageSize}";
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                var cache = new LocalCacheService();
                var cached = await cache.QueryByNameAsync(firstName, lastName, pageSize, (page-1)*pageSize);
                return new PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = cached.Count, Items = cached };
            }

            var json = await res.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var paged = JsonSerializer.Deserialize<PagedResult<PersonSummaryDto>>(json, options);
            if (paged != null && paged.Items != null)
            {
                var cache = new LocalCacheService();
                await cache.UpsertPersonsAsync(paged.Items);
            }

            return paged ?? new PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Trisecmed.Web.Models;

namespace Trisecmed.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ApiClient(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private async Task SetAuthHeader()
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // Devices
    public async Task<PagedResult<DeviceDto>> GetDevicesAsync(int page = 1, int pageSize = 25, string? search = null, string? status = null)
    {
        await SetAuthHeader();
        var url = $"api/v1/devices?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        return await _http.GetFromJsonAsync<PagedResult<DeviceDto>>(url) ?? new();
    }

    public async Task<DeviceDto?> GetDeviceAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<DeviceDto>($"api/v1/devices/{id}");
    }

    public async Task<HttpResponseMessage> CreateDeviceAsync(object device)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync("api/v1/devices", device);
    }

    public async Task<HttpResponseMessage> UpdateDeviceAsync(Guid id, object device)
    {
        await SetAuthHeader();
        return await _http.PutAsJsonAsync($"api/v1/devices/{id}", device);
    }

    public async Task<HttpResponseMessage> DeleteDeviceAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.DeleteAsync($"api/v1/devices/{id}");
    }

    // Device status change
    public async Task<HttpResponseMessage> ChangeDeviceStatusAsync(Guid id, object data)
    {
        await SetAuthHeader();
        return await _http.PatchAsJsonAsync($"api/v1/devices/{id}/status", data);
    }

    // Device inspections
    public async Task<List<InspectionDto>> GetDeviceInspectionsAsync(Guid deviceId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<InspectionDto>>($"api/v1/devices/{deviceId}/inspections") ?? [];
    }

    public async Task<HttpResponseMessage> CreateInspectionAsync(Guid deviceId, object data)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync($"api/v1/devices/{deviceId}/inspections", data);
    }

    // Device import
    public async Task<HttpResponseMessage> ImportDevicesAsync(MultipartFormDataContent content)
    {
        await SetAuthHeader();
        return await _http.PostAsync("api/v1/devices/import", content);
    }

    // Failures
    public async Task<PagedResult<FailureDto>> GetFailuresAsync(int page = 1, int pageSize = 25, string? status = null, string? priority = null)
    {
        await SetAuthHeader();
        var url = $"api/v1/failures?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (!string.IsNullOrEmpty(priority)) url += $"&priority={priority}";
        return await _http.GetFromJsonAsync<PagedResult<FailureDto>>(url) ?? new();
    }

    public async Task<FailureDto?> GetFailureAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<FailureDto>($"api/v1/failures/{id}");
    }

    public async Task<HttpResponseMessage> CreateFailureAsync(object failure)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync("api/v1/failures", failure);
    }

    public async Task<HttpResponseMessage> ChangeFailureStatusAsync(Guid id, object data)
    {
        await SetAuthHeader();
        return await _http.PatchAsJsonAsync($"api/v1/failures/{id}/status", data);
    }

    public async Task<HttpResponseMessage> AssignServiceProviderAsync(Guid failureId, object data)
    {
        await SetAuthHeader();
        return await _http.PatchAsJsonAsync($"api/v1/failures/{failureId}/assign", data);
    }

    public async Task<HttpResponseMessage> ResolveFailureAsync(Guid id, object data)
    {
        await SetAuthHeader();
        return await _http.PatchAsJsonAsync($"api/v1/failures/{id}/resolve", data);
    }

    public async Task<List<FailureStatusHistoryDto>> GetFailureHistoryAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<FailureStatusHistoryDto>>($"api/v1/failures/{id}/history") ?? [];
    }

    // GDPR export
    public async Task<byte[]> DownloadGdprExportAsync(Guid userId)
    {
        await SetAuthHeader();
        var response = await _http.GetAsync($"api/v1/reports/export/gdpr/{userId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    // Categories
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<CategoryDto>>("api/v1/categories") ?? [];
    }

    public async Task<HttpResponseMessage> CreateCategoryAsync(object data)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync("api/v1/categories", data);
    }

    public async Task<HttpResponseMessage> UpdateCategoryAsync(Guid id, object data)
    {
        await SetAuthHeader();
        return await _http.PutAsJsonAsync($"api/v1/categories/{id}", data);
    }

    public async Task<HttpResponseMessage> DeleteCategoryAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.DeleteAsync($"api/v1/categories/{id}");
    }

    // Departments
    public async Task<List<DepartmentDto>> GetDepartmentsAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<DepartmentDto>>("api/v1/departments") ?? [];
    }

    public async Task<HttpResponseMessage> CreateDepartmentAsync(object data)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync("api/v1/departments", data);
    }

    public async Task<HttpResponseMessage> UpdateDepartmentAsync(Guid id, object data)
    {
        await SetAuthHeader();
        return await _http.PutAsJsonAsync($"api/v1/departments/{id}", data);
    }

    public async Task<HttpResponseMessage> DeleteDepartmentAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.DeleteAsync($"api/v1/departments/{id}");
    }

    // Service Providers
    public async Task<List<ServiceProviderDto>> GetServiceProvidersAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ServiceProviderDto>>("api/v1/service-providers") ?? [];
    }

    public async Task<HttpResponseMessage> CreateServiceProviderAsync(object data)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync("api/v1/service-providers", data);
    }

    public async Task<HttpResponseMessage> UpdateServiceProviderAsync(Guid id, object data)
    {
        await SetAuthHeader();
        return await _http.PutAsJsonAsync($"api/v1/service-providers/{id}", data);
    }

    public async Task<HttpResponseMessage> DeleteServiceProviderAsync(Guid id)
    {
        await SetAuthHeader();
        return await _http.DeleteAsync($"api/v1/service-providers/{id}");
    }

    // Reports
    public async Task<byte[]> DownloadReportAsync(string endpoint, object request)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/v1/reports/{endpoint}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<(byte[] Data, string FileName)> DownloadReportWithNameAsync(string endpoint, object request)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/v1/reports/{endpoint}", request);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadAsByteArrayAsync();
        var contentDisposition = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "raport";
        return (data, contentDisposition);
    }

    // Notifications
    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(int page = 1, int pageSize = 25, string? type = null, bool? isSent = null)
    {
        await SetAuthHeader();
        var url = $"api/v1/notifications?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(type)) url += $"&type={Uri.EscapeDataString(type)}";
        if (isSent.HasValue) url += $"&isSent={isSent.Value}";
        return await _http.GetFromJsonAsync<PagedResult<NotificationDto>>(url) ?? new();
    }

    // Users
    public async Task<List<UserDto>> GetUsersAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<UserDto>>("api/v1/users") ?? [];
    }

    public async Task<HttpResponseMessage> CreateUserAsync(object data)
    {
        await SetAuthHeader();
        return await _http.PostAsJsonAsync("api/v1/users", data);
    }
}

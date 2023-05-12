using AsmResolver.DotNet;
using CommonHelper;
using FileService.WebAPI.Uploader;
using FileServiceDomain;
using FileServiceInfrastrucure;
using FileServiceInfrastrucure.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zack.Commons;

namespace FileService.SDK.NETCore
{
    public class FileServiceClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly Uri serverRoot;
        private readonly JWTOptions optionsSnapshot;
        private readonly IOptionsSnapshot<SMBStorageOptions> smbOption;

        public FileServiceClient(IHttpClientFactory httpClientFactory, Uri serverRoot, JWTOptions optionsSnapshot, IOptionsSnapshot<SMBStorageOptions> smbOption)
        {
            this.httpClientFactory = httpClientFactory;
            this.serverRoot = serverRoot;
            this.optionsSnapshot = optionsSnapshot;
            this.smbOption = smbOption;
        }
        public Task<FileExistsResponse> FileExistsAsync(long fileSize, string sha256Hash, CancellationToken stoppingToken = default)
        {
            string relativeUrl = FormattableStringHelper.BuildUrl($"api/Uploader/FileExists?fileSize={fileSize}&sha256Hash={sha256Hash}");
            Uri requestUri = new Uri(serverRoot, relativeUrl);
            var httpClient = httpClientFactory.CreateClient();
            return httpClient.GetJsonAsync<FileExistsResponse>(requestUri, stoppingToken)!;
        }

        string BuildToken()
        {
            //因为JWT的key等机密信息只有服务器端知道，因此可以这样非常简单的读到配置
            Claim claim = new Claim(ClaimTypes.Role, "Admin");
            return TokenService.BuildToken(new Claim[] { claim }, optionsSnapshot);
        }

        public async Task<Uri> UploadCloudAsync(FileInfo file, CancellationToken stoppingToken = default)
        {
            string token = BuildToken();
            using MultipartFormDataContent content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenRead());
            content.Add(fileContent, "file", file.Name);
            var httpClient = httpClientFactory.CreateClient();
            Uri requestUri = new Uri(serverRoot + "/Uploader/Upload");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var respMsg = await httpClient.PostAsync(requestUri, content, stoppingToken);
            if (!respMsg.IsSuccessStatusCode)
            {
                string respString = await respMsg.Content.ReadAsStringAsync(stoppingToken);
                throw new HttpRequestException($"上传失败，状态码：{respMsg.StatusCode}，响应报文：{respString}");
            }
            else
            {
                string respString = await respMsg.Content.ReadAsStringAsync(stoppingToken);
                return respString.ParseJson<Uri>()!;
            }
        }

        public async Task<Uri> UploadAsync(FileInfo file, CancellationToken stoppingToken = default)
        {
            string fileName = file.Name;
            DateTime today = DateTime.Today;
            string key = $"Completed/{today.Year}/{today.Month}/{today.Day}/{fileName}";
            string workingDir = smbOption.Value.WorkingDir;
            //给文件安排一个位置
            string fullPath = Path.Combine(workingDir, key);
            string? fullDir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            //写入
            file.CopyTo(fullPath);
            return new Uri(fullPath);
        }
    }
}

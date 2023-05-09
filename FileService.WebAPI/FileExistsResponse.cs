namespace FileService.WebAPI
{
    public record FileExistsResponse(bool IsExists, Uri? Url);
}

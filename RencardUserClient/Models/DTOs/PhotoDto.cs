namespace RencardUserClient.Models.DTOs
{
    public record PhotoDto(
        string? BlobUrl = null,
        DateTime UploadedAt = default
    );
}

namespace RencardUserClient.Models.DTOs
{
    public record AboutDto(
         string Description = "",
         bool IsSmoker = false,
         string Alcohol = "None",
         string Religion = "Other"
     );
}

using NetTopologySuite.Geometries;
using RencardUserClient.Models.About;
using RencardUserClient.Models.Location;
using RencardUserClient.Models.Photos;
using RencardUserClient.Models.Preferences;
using RencardUserClient.Models.DTOs.Update;

namespace RencardUserClient.Models.DTOs.Extensions
{
    public static class EntityExtensions
    {
        // Converters from Entity to DTO
        public static AboutDto ToDto(this AboutUser entity) => new(
            entity.Description,
            entity.IsSmoker,
            entity.Alcohol,
            entity.Religion
        );

        public static LocationDto ToDto(this UserLocation entity) => new(
            entity.Geography?.X ?? 0,
            entity.Geography?.Y ?? 0,
            entity.LastUpdated
        );

        public static PreferencesDto ToDto(this UserPreferences entity) => new(
            entity.PreferredSex,
            entity.MinPreferredAge,
            entity.MaxPreferredAge,
            entity.SearchRadiusKm
        );

        // Converters from DTO to Entity
        public static AboutUser ToEntity(this AboutDto dto, string userId) => new AboutUser
        {
            UserId = userId,
            Description = dto.Description,
            IsSmoker = dto.IsSmoker,
            Alcohol = dto.Alcohol,
            Religion = dto.Religion
        };

        public static UserPreferences ToEntity(this PreferencesDto dto, string userId) => new UserPreferences
        {
            UserId = userId,
            PreferredSex = dto.PreferredSex,
            MinPreferredAge = dto.MinPreferredAge,
            MaxPreferredAge = dto.MaxPreferredAge,
            SearchRadiusKm = dto.SearchRadiusKm
        };

        public static UserLocation ToEntity(this LocationDto dto, string userId) => new UserLocation
        {
            UserId = userId,
            Geography = null,               // intentionally null for uninitialized location
            LastUpdated = DateTime.UtcNow
        };

        public static UserPhoto ToEntity(this PhotoDto dto, string userId) => new UserPhoto
        {
            UserId = userId,
            BlobUrl = dto.BlobUrl,
            UploadedAt = dto.UploadedAt
        };

        // Update existing entities from DTO
        public static void UpdateFrom(this AboutUser entity, AboutDto dto)
        {
            entity.Description = dto.Description;
            entity.IsSmoker = dto.IsSmoker;
            entity.Alcohol = dto.Alcohol;
            entity.Religion = dto.Religion;
        }

        public static void UpdateFrom(this UserPreferences entity, PreferencesDto dto)
        {
            entity.PreferredSex = dto.PreferredSex;
            entity.MinPreferredAge = dto.MinPreferredAge;
            entity.MaxPreferredAge = dto.MaxPreferredAge;
            entity.SearchRadiusKm = dto.SearchRadiusKm;
        }

        public static void UpdateFrom(this UserLocation entity, LocationDto dto)
        {
            entity.Geography = new Point(dto.Longitude, dto.Latitude) { SRID = 4326 };
            entity.LastUpdated = DateTime.UtcNow;
        }
        public static void UpdateFrom(this AboutUser entity, AboutPatchDto dto)
        {
            if (dto.Description is not null) entity.Description = dto.Description;
            if (dto.IsSmoker is not null) entity.IsSmoker = dto.IsSmoker.Value;
            if (dto.Alcohol is not null) entity.Alcohol = dto.Alcohol;
            if (dto.Religion is not null) entity.Religion = dto.Religion;
        }

        public static void UpdateFrom(this UserPreferences entity, PreferencesPatchDto dto)
        {
            if (dto.PreferredSex is not null) entity.PreferredSex = dto.PreferredSex;
            if (dto.MinPreferredAge is not null) entity.MinPreferredAge = dto.MinPreferredAge.Value;
            if (dto.MaxPreferredAge is not null) entity.MaxPreferredAge = dto.MaxPreferredAge.Value;
            if (dto.SearchRadiusKm is not null) entity.SearchRadiusKm = dto.SearchRadiusKm.Value;
        }

        public static void UpdateFrom(this UserLocation entity, LocationPatchDto dto)
        {
            if (dto.Longitude.HasValue && dto.Latitude.HasValue)
            {
                entity.Geography = new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 };
                entity.LastUpdated = DateTime.UtcNow;
            }
        }

        // конвертеры сущность → Update-DTO
        public static AboutPatchDto ToDtoUpdate(this AboutUser entity) => new(
            Description: entity.Description,
            IsSmoker: entity.IsSmoker,
            Alcohol: entity.Alcohol,
            Religion: entity.Religion
        );

        public static PreferencesPatchDto ToDtoUpdate(this UserPreferences entity) => new(
            PreferredSex: entity.PreferredSex,
            MinPreferredAge: entity.MinPreferredAge,
            MaxPreferredAge: entity.MaxPreferredAge,
            SearchRadiusKm: entity.SearchRadiusKm
        );

        public static LocationPatchDto ToDtoUpdate(this UserLocation entity) => new(
            Longitude: entity.Geography?.X,
            Latitude: entity.Geography?.Y
        );
    }
}

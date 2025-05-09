using RencardUserClient.Models.Identity;

namespace RencardUserClient.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateAsync(string email, string password, string phone, DateTime birthDate, string sex);
        // Дополнительные методы при необходимости
    }
}

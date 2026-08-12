using ContactManager.Models;

namespace ContactManager.Data
{
    public interface IContactRepository
    {
        Task<(IEnumerable<Contact> Items, int Total)> GetContactsAsync(
            string? sortBy, string? sortDir, string? filter, int page, int pageSize);
        Task InsertContactsAsync(IEnumerable<Contact> contacts);
        Task UpdateContactAsync(Contact contact);
        Task DeleteContactAsync(int id);
        Task<HashSet<string>> GetExistingPhonesAsync();
    }
}
using System.Data;
using Dapper;
using ContactManager.Models;

namespace ContactManager.Data
{
    public class ContactRepository : IContactRepository
    {
        private readonly DapperContext _context;

        public ContactRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Contact> Items, int Total)> GetContactsAsync(
            string? sortBy, string? sortDir, string? filter, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 10 : pageSize;

            var sortColumn = sortBy?.ToLower() switch
            {
                "dateofbirth" => "DateOfBirth",
                "married" => "Married",
                "phone" => "Phone",
                "salary" => "Salary",
                _ => "Name"
            };
            var direction = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            var whereClause = string.IsNullOrWhiteSpace(filter)
    ? ""
    : @"WHERE Name LIKE @Filter 
        OR Phone LIKE @Filter 
        OR CAST(DateOfBirth AS NVARCHAR) LIKE @Filter
        OR CAST(Salary AS NVARCHAR) LIKE @Filter
        OR (CASE WHEN Married = 1 THEN 'married' ELSE 'single' END) LIKE @Filter";

            var sql = $@"
                SELECT COUNT(*) FROM Contacts {whereClause};

                SELECT Id, Name, DateOfBirth, Married, Phone, Salary
                FROM Contacts
                {whereClause}
                ORDER BY {sortColumn} {direction}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new DynamicParameters();
            parameters.Add("Filter", $"%{filter}%");
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            using var connection = _context.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var total = await multi.ReadSingleAsync<int>();
            var items = await multi.ReadAsync<Contact>();

            return (items, total);
        }

        public async Task InsertContactsAsync(IEnumerable<Contact> contacts)
        {
            const string sql = @"
                INSERT INTO Contacts (Name, DateOfBirth, Married, Phone, Salary)
                VALUES (@Name, @DateOfBirth, @Married, @Phone, @Salary);";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, contacts);
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            const string sql = @"
                UPDATE Contacts
                SET Name = @Name, DateOfBirth = @DateOfBirth, Married = @Married,
                    Phone = @Phone, Salary = @Salary
                WHERE Id = @Id;";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, contact);
        }

        public async Task DeleteContactAsync(int id)
        {
            const string sql = "DELETE FROM Contacts WHERE Id = @Id;";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }
        public async Task<HashSet<string>> GetExistingPhonesAsync()
        {
            const string sql = "SELECT Phone FROM Contacts;";
            using var connection = _context.CreateConnection();
            var phones = await connection.QueryAsync<string>(sql);
            return new HashSet<string>(phones);
        }
    }
}
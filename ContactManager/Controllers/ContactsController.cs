using System.Globalization;
using ContactManager.Data;
using ContactManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IContactRepository _repository;
        public ContactsController(IContactRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["Error"] = "Please select a CSV file.";
                return RedirectToAction(nameof(Index));
            }

            var contacts = new List<Contact>();
            var parseErrors = new List<string>();

            using (var reader = new StreamReader(csvFile.OpenReadStream()))
            {
                string? line;
                var isHeader = true;
                var lineNumber = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    if (isHeader) { isHeader = false; continue; }
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 5)
                    {
                        parseErrors.Add($"Line {lineNumber}: expected 5 columns, got {parts.Length}.");
                        continue;
                    }

                    try
                    {
                        contacts.Add(new Contact
                        {
                            Name = parts[0].Trim(),
                            DateOfBirth = DateTime.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
                            Married = bool.Parse(parts[2].Trim()),
                            Phone = parts[3].Trim(),
                            Salary = decimal.Parse(parts[4].Trim(), CultureInfo.InvariantCulture)
                        });
                    }
                    catch (Exception ex)
                    {
                        parseErrors.Add($"Line {lineNumber}: {ex.Message}");
                    }
                }
            }

            var existingPhones = await _repository.GetExistingPhonesAsync();
            var newContacts = contacts.Where(c => !existingPhones.Contains(c.Phone)).ToList();
            var duplicateCount = contacts.Count - newContacts.Count;

            if (newContacts.Count > 0)
            {
                await _repository.InsertContactsAsync(newContacts);
            }

            TempData["Success"] = $"{newContacts.Count} contact(s) imported.";

            var errorMessages = new List<string>();
            if (duplicateCount > 0)
            {
                errorMessages.Add($"{duplicateCount} skipped (duplicate phone).");
            }
            if (parseErrors.Count > 0)
            {
                errorMessages.Add($"{parseErrors.Count} row(s) had errors: " + string.Join(" | ", parseErrors.Take(5)));
            }
            if (errorMessages.Count > 0)
            {
                TempData["Error"] = string.Join(" ", errorMessages);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetContacts(
            string? sortBy, string? sortDir, string? filter, int page = 1, int pageSize = 10)
        {
            var (items, total) = await _repository.GetContactsAsync(sortBy, sortDir, filter, page, pageSize);
            return Json(new { data = items, total, page, pageSize });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContact([FromForm] int id)
        {
            await _repository.DeleteContactAsync(id);
            return Json(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact([FromBody] Contact contact)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }

            await _repository.UpdateContactAsync(contact);
            return Json(new { success = true });
        }
    }

}

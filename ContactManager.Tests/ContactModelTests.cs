using ContactManager.Models;
using Xunit;

namespace ContactManager.Tests
{
    public class ContactModelTests
    {
        [Fact]
        public void Contact_DefaultName_IsEmptyString_NotNull()
        {
            var contact = new Contact();

            Assert.NotNull(contact.Name);
            Assert.Equal(string.Empty, contact.Name);
        }

        [Fact]
        public void Contact_CanSetAllProperties()
        {
            var contact = new Contact
            {
                Id = 1,
                Name = "John Smith",
                DateOfBirth = new DateTime(1990, 5, 12),
                Married = true,
                Phone = "+380931112233",
                Salary = 45000.50m
            };

            Assert.Equal(1, contact.Id);
            Assert.Equal("John Smith", contact.Name);
            Assert.Equal(new DateTime(1990, 5, 12), contact.DateOfBirth);
            Assert.True(contact.Married);
            Assert.Equal("+380931112233", contact.Phone);
            Assert.Equal(45000.50m, contact.Salary);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void BoolParse_ParsesCsvValuesCorrectly(string input, bool expected)
        {
            var result = bool.Parse(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Csv_Line_SplitsIntoCorrectNumberOfFields()
        {
            var line = "John Smith,1990-05-12,true,+380931112233,45000.50";
            var parts = line.Split(',');

            Assert.Equal(5, parts.Length);
            Assert.Equal("John Smith", parts[0]);
        }
    }
}
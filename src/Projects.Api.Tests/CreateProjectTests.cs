using Xunit;
using Projects.Api.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Tests {
    public static class ValidationHelper {
        public static IList<ValidationResult> ValidateModel(object model) {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
            return validationResults;
        }
    }
    public class CreateProjectTests {
        [Theory]
        [InlineData(null)]
        [InlineData("")]


        public void Validate_MissingName_ShouldReturnError(string? invalidName) {
            var model = new ProjectRequestDTO { name = invalidName };
            var results = ValidationHelper.ValidateModel(model);

            Assert.Contains(results, v => v.MemberNames.Contains(nameof(model.name)));
        }

        [Fact]
        public void Validate_NameLongerThan100_ShouldReturnError() {
            var model = new ProjectRequestDTO { name = new string('A', 101) };
            var results = ValidationHelper.ValidateModel(model);

            Assert.Contains(results, v => v.MemberNames.Contains(nameof(model.name)));
        }

        [Fact]
        public void Validate_DescriptionLongerThan500_ShouldReturnError() {
            var model = new ProjectRequestDTO { name = "Valid", description = new string('B', 501) };
            var results = ValidationHelper.ValidateModel(model);

            Assert.Contains(results, v => v.MemberNames.Contains(nameof(model.description)));
        }
        [Fact]
        public void Validate_ValidData_ShouldPassValidation() {
            // Arrange
            var model = new ProjectRequestDTO {
                name = "Valid Project Name",
                description = "This is a valid description."
            };

            // Act
            var results = ValidationHelper.ValidateModel(model);

            // Assert
            Assert.Empty(results); // Якщо модель валідна, список помилок має бути порожнім
        }
    }
}
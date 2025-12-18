using System.ComponentModel.DataAnnotations;

namespace L.R._Gcaleka__Co
{
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _expectedValue;

        public RequiredIfAttribute(string dependentProperty, object expectedValue)
        {
            _dependentProperty = dependentProperty;
            _expectedValue = expectedValue;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dependentProperty = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (dependentProperty == null)
            {
                return new ValidationResult($"Unknown property: {_dependentProperty}");
            }

            var dependentValue = dependentProperty.GetValue(validationContext.ObjectInstance);

            if (dependentValue != null && dependentValue.Equals(_expectedValue))
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
                }
            }
            return ValidationResult.Success;

        }
    }
    //public class RequiredAttribute : ValidationAttribute
    //{
    //    private readonly string _dependentProperty;
    //    private readonly object _expectValue;

    //    public RequiredAttribute(string dependentProperty, object expectValue)
    //    {
    //        _dependentProperty = dependentProperty;
    //        _expectValue = expectValue;
    //    }


    //}
}

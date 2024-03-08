using System.Globalization;
using System.Windows.Controls;

namespace CampaignEditor
{
    public class CharLengthValidationRule : ValidationRule
    {
        public int MaxLength { get; set; }

        public CharLengthValidationRule(int maxLength)
        {
            MaxLength = maxLength;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && value.ToString().Length > MaxLength)
            {
                return new ValidationResult(false, $"Input must be {MaxLength} characters or less.");
            }

            return ValidationResult.ValidResult;
        }
    }
}

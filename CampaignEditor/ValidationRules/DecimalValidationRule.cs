using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor.ValidationRules
{
    public class DecimalValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (string.IsNullOrEmpty(input))
            {
                return new ValidationResult(false, "Input cannot be empty.");
            }

            // Regex for decimal numbers with up to 8 decimal places
            string pattern = @"^\d+(\.\d{1,8})?$";
            if (Regex.IsMatch(input, pattern))
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Invalid decimal number. Up to 8 decimal places are allowed.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class SelectionValidationRule : System.Windows.Controls.ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return value == null
                ? new ValidationResult(false, "Please select one")
                : new ValidationResult(true, null);
        }

    }

    public class KeyVaultSelectionValidationRule : System.Windows.Controls.ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return value == null
                ? new ValidationResult(false, "Please select a Key Vault before proceeding if you wish to migrate a private certificate to IA")
                : new ValidationResult(true, null);
        }

    }
}

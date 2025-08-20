using Rankora_API.Scripts.Rankora.Types;

namespace Rankora_API.Scripts.Utils.RankoraValidation
{
    public static class RankoraValidation
    {
        public struct ValidationResult
        {
            public bool IsValid;
            public string ErrorMessage;
            public ValidationResult(bool isValid, string errorMessage)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }
        public static ValidationResult IsValidPlayerName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return new ValidationResult(false, "Player name cannot be empty!");
            }

            if (playerName.Length < 3 || playerName.Length > 50)
            {
                return new ValidationResult(false, "Player name must be between 3 and 50 characters!");
            }

            foreach (char c in playerName)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != ' ' && c != '-')
                {
                    return new ValidationResult(false, "Player name can only contain letters, numbers, spaces, underscores, and hyphens!");
                }
            }

            return new ValidationResult(true, string.Empty);
        }

        public const int MAX_METADATA_KEYS = 10;     
        public const int MAX_KEY_LENGTH = 32;
        public const int MAX_VALUE_LENGTH = 64;

        public static ValidationResult IsValidMetadata(Metadata metadata)
        {
            if (metadata == null)
            {
                return new ValidationResult(true, string.Empty);
            }

            var items = metadata.GetData();
            if (items.Count > MAX_METADATA_KEYS)
            {
                return new ValidationResult(false, $"Metadata cannot have more than {MAX_METADATA_KEYS} keys.");
            }

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    return new ValidationResult(false, $"Metadata key cannot be null or empty.");
                }

                if (item.Key.Length > MAX_KEY_LENGTH)
                {
                    return new ValidationResult(false, $"Metadata key '{item.Key}' exceeds max length of {MAX_KEY_LENGTH}.");
                }

                if (item.Value == null)
                {
                    return new ValidationResult(false, $"Metadata value for key '{item.Key}' cannot be null.");
                }

                var type = item.Value.GetType();
                bool isSupported = type == typeof(string) || type == typeof(int) || type == typeof(float) ||
                                   type == typeof(double) || type == typeof(bool);

                if (!isSupported)
                {
                    return new ValidationResult(false, $"Metadata value for '{item.Key}' must be a primitive (string, int, float, double, or bool).");
                }

                string valueStr = item.Value.ToString();
                if (valueStr.Length > MAX_VALUE_LENGTH)
                {
                    return new ValidationResult(false, $"Metadata value for '{item.Key}' exceeds max length of {MAX_VALUE_LENGTH}.");
                }
            }

            return new ValidationResult(true, string.Empty);
        }

    }
}

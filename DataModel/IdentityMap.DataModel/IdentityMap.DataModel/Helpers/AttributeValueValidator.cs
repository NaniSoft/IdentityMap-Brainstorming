using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;
using System.Text.RegularExpressions;

namespace IdentityMap.DataModel.Helpers
{
    public static class AttributeValueValidator
    {
        public static IReadOnlyList<string> Validate(
            ResourceAttributeValue value,
            ResourceAttributeDefinition definition)
        {
            var errors = new List<string>();

            if (definition.IsRequired && value.TypedValue is null)
            {
                errors.Add($"'{definition.Label}' is required.");
                return errors;
            }

            if (value.TypedValue is null) return errors;

            switch (definition.DataType)
            {
                case AttributeDataType.String:
                case AttributeDataType.Text:
                case AttributeDataType.Url:
                    {
                        var s = value.ValueString ?? string.Empty;
                        if (definition.MinLength.HasValue && s.Length < definition.MinLength)
                            errors.Add($"'{definition.Label}' must be at least {definition.MinLength} characters.");
                        if (definition.MaxLength.HasValue && s.Length > definition.MaxLength)
                            errors.Add($"'{definition.Label}' must not exceed {definition.MaxLength} characters.");
                        if (!string.IsNullOrEmpty(definition.RegexPattern)
                            && !Regex.IsMatch(s, definition.RegexPattern))
                            errors.Add($"'{definition.Label}' does not match the required format.");
                        break;
                    }

                case AttributeDataType.Integer:
                    {
                        var i = (double)(value.ValueInt ?? 0);
                        if (definition.MinValue.HasValue && i < definition.MinValue)
                            errors.Add($"'{definition.Label}' must be ≥ {definition.MinValue}.");
                        if (definition.MaxValue.HasValue && i > definition.MaxValue)
                            errors.Add($"'{definition.Label}' must be ≤ {definition.MaxValue}.");
                        break;
                    }

                case AttributeDataType.Double:
                    {
                        var d = value.ValueDouble ?? 0d;
                        if (definition.MinValue.HasValue && d < definition.MinValue)
                            errors.Add($"'{definition.Label}' must be ≥ {definition.MinValue}.");
                        if (definition.MaxValue.HasValue && d > definition.MaxValue)
                            errors.Add($"'{definition.Label}' must be ≤ {definition.MaxValue}.");
                        break;
                    }

                case AttributeDataType.Enum:
                    {
                        var chosen = value.ValueString;
                        var valid = definition.EnumOptions.Where(o => o.IsActive).Select(o => o.Value).ToHashSet();
                        if (!string.IsNullOrEmpty(chosen) && !valid.Contains(chosen))
                            errors.Add($"'{chosen}' is not a valid option for '{definition.Label}'.");
                        break;
                    }

                case AttributeDataType.ResourceReference:
                    if (!Guid.TryParse(value.ValueString, out _))
                        errors.Add($"'{definition.Label}' must be a valid resource identifier.");
                    break;
            }

            return errors;
        }
    }
}
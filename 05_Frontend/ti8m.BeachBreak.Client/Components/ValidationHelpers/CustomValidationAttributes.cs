using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.Client.Components.ValidationHelpers;

public class FutureDateAttribute : ValidationAttribute
{
    private readonly bool _allowToday;

    public FutureDateAttribute(bool allowToday = true)
    {
        _allowToday = allowToday;
        ErrorMessage = allowToday ?
            "Date cannot be in the past." :
            "Date must be in the future.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime date)
            return true; // Let Required attribute handle null values

        var today = DateTime.Today;
        return _allowToday ? date.Date >= today : date.Date > today;
    }
}

public class MaxDateRangeAttribute : ValidationAttribute
{
    private readonly int _maxYears;

    public MaxDateRangeAttribute(int maxYears = 1)
    {
        _maxYears = maxYears;
        ErrorMessage = $"Date cannot be more than {maxYears} year(s) in the future.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime date)
            return true;

        var maxDate = DateTime.Today.AddYears(_maxYears);
        return date.Date <= maxDate;
    }
}

public class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
    {
        ErrorMessage = "Please make a selection.";
    }

    public override bool IsValid(object? value)
    {
        if (value is Guid guid)
            return guid != Guid.Empty;

        return false;
    }
}

public class MaxLengthListAttribute : ValidationAttribute
{
    private readonly int _maxLength;

    public MaxLengthListAttribute(int maxLength)
    {
        _maxLength = maxLength;
        ErrorMessage = $"Selection cannot exceed {maxLength} items.";
    }

    public override bool IsValid(object? value)
    {
        if (value is System.Collections.IList list)
            return list.Count <= _maxLength;

        return true;
    }
}

public class MinLengthListAttribute : ValidationAttribute
{
    private readonly int _minLength;

    public MinLengthListAttribute(int minLength)
    {
        _minLength = minLength;
        ErrorMessage = $"Please select at least {minLength} item(s).";
    }

    public override bool IsValid(object? value)
    {
        if (value is System.Collections.IList list)
            return list.Count >= _minLength;

        return false;
    }
}

public class StaggerValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is int staggerDays && staggerDays >= 1 && staggerDays <= 30;
    }

    public override string FormatErrorMessage(string name)
    {
        return "Stagger days must be between 1 and 30.";
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace OpenStore.Data.EntityFramework;

public class EntityValidationException : Exception
{
    public IImmutableList<ValidationResult> ValidationResults { get; }

    public EntityValidationException(string message, IEnumerable<ValidationResult> validationResults) : base(message)
    {
        ValidationResults = validationResults.ToImmutableList();
    }
}
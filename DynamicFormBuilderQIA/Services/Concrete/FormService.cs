using DynamicFormBuilderQIA.Models;
using DynamicFormBuilderQIA.Repository.interfaces;
using DynamicFormBuilderQIA.Services.Abstraction;
using DynamicFormBuilderQIA.ViewModels;

namespace DynamicFormBuilderQIA.Services.Concrete;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;

    public FormService(IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<List<FieldOption>> GetAllFieldOptionsAsync()
    {
        // Business logic: Could add caching here
        return await _formRepository.GetAllFieldOptionsAsync();
    }

    public async Task<int> CreateFormAsync(CreateFormViewModel model)
    {
        // Business logic: Validate form before saving
        ValidateFormModel(model);

        // Business logic: Ensure each field has proper display order
        if (model.Fields != null && model.Fields.Any())
        {
            for (int i = 0; i < model.Fields.Count; i++)
            {
                if (model.Fields[i].DisplayOrder == 0)
                {
                    model.Fields[i].DisplayOrder = i + 1;
                }
            }
        }

        return await _formRepository.SaveFormAsync(model);
    }

    public async Task<(List<FormListItemViewModel> data, int totalRecords, int filteredRecords)> GetAllFormsAsync(DataTableRequest request)
    {
        // Business logic: Validate pagination parameters
        if (request.Length <= 0)
        {
            request.Length = 10; // Default page size
        }

        if (request.Start < 0)
        {
            request.Start = 0;
        }

        return await _formRepository.GetAllFormsAsync(request);
    }

    public async Task<Form> GetFormByIdAsync(int formId)
    {
        // Business logic: Validate formId
        if (formId <= 0)
        {
            throw new ArgumentException("Form ID must be greater than zero.", nameof(formId));
        }

        var form = await _formRepository.GetFormByIdAsync(formId);

        if (form == null)
        {
            throw new KeyNotFoundException($"Form with ID {formId} not found.");
        }

        // Business logic: Sort form fields by display order
        if (form.FormFields != null && form.FormFields.Any())
        {
            form.FormFields = form.FormFields.OrderBy(f => f.DisplayOrder).ToList();
        }

        return form;
    }

    public async Task DeleteFormAsync(int formId)
    {
        // Business logic: Validate formId
        if (formId <= 0)
        {
            throw new ArgumentException("Form ID must be greater than zero.", nameof(formId));
        }

        // Business logic: Check if form exists before deletion
        var form = await _formRepository.GetFormByIdAsync(formId);

        if (form == null)
        {
            throw new KeyNotFoundException($"Form with ID {formId} not found.");
        }

        // Business logic: Could add additional checks here
        // e.g., Check if form has submissions, add soft delete, log deletion, etc.

        await _formRepository.DeleteFormAsync(formId);
    }

    // Private helper method for validation
    private void ValidateFormModel(CreateFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FormTitle))
        {
            throw new ArgumentException("Form title is required.", nameof(model.FormTitle));
        }

        if (model.FormTitle.Length > 200)
        {
            throw new ArgumentException("Form title cannot exceed 200 characters.", nameof(model.FormTitle));
        }

        if (model.Fields == null || !model.Fields.Any())
        {
            throw new ArgumentException("Form must have at least one field.", nameof(model.Fields));
        }

        // Validate each field
        foreach (var field in model.Fields)
        {
            if (string.IsNullOrWhiteSpace(field.FieldLabel))
            {
                throw new ArgumentException("All fields must have a label.");
            }
        }
    }
}

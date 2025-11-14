using DynamicFormBuilderQIA.Models;
using DynamicFormBuilderQIA.ViewModels;

namespace DynamicFormBuilderQIA.Services.Abstraction;

public interface IFormService
{
    Task<List<FieldOption>> GetAllFieldOptionsAsync();
    Task<int> CreateFormAsync(CreateFormViewModel model);
    Task<(List<FormListItemViewModel> data, int totalRecords, int filteredRecords)> GetAllFormsAsync(DataTableRequest request);
    Task<Form> GetFormByIdAsync(int formId);
    Task DeleteFormAsync(int formId);
}

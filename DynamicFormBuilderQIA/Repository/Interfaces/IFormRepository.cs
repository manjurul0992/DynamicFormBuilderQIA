using DynamicFormBuilderQIA.Models;
using DynamicFormBuilderQIA.ViewModels;

namespace DynamicFormBuilderQIA.Repository.interfaces;

public interface IFormRepository
{
    Task<List<FieldOption>> GetAllFieldOptionsAsync();
    Task<int> SaveFormAsync(CreateFormViewModel model);
    Task<(List<FormListItemViewModel> data, int totalRecords, int filteredRecords)> GetAllFormsAsync(DataTableRequest request);
    Task<Form> GetFormByIdAsync(int formId);
    Task DeleteFormAsync(int formId);
}
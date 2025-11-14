using DynamicFormBuilderQIA.Models;
using DynamicFormBuilderQIA.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilderQIA.ViewComponents;

public class DropdownViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string fieldId,
        string fieldName,
        string fieldLabel,
        bool isRequired,
        List<FieldOption> options,
        string selectedValue = null)
    {
        var model = new DropdownViewModel
        {
            FieldId = fieldId,
            FieldName = fieldName,
            FieldLabel = fieldLabel,
            IsRequired = isRequired,
            Options = options ?? new List<FieldOption>(),
            SelectedValue = selectedValue
        };

        return View(model);
    }
}

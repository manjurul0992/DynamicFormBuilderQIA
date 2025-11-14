using DynamicFormBuilderQIA.Models;

namespace DynamicFormBuilderQIA.ViewModels;

public class DropdownViewModel
{
    public string FieldId { get; set; }
    public string FieldName { get; set; }
    public string FieldLabel { get; set; }
    public bool IsRequired { get; set; }
    public List<FieldOption> Options { get; set; }
    public string SelectedValue { get; set; }
}

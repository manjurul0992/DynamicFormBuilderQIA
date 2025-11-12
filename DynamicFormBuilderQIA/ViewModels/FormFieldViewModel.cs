namespace DynamicFormBuilderQIA.ViewModels;

public class FormFieldViewModel
{
    public string FieldLabel { get; set; }
    public int FieldLevel { get; set; }
    public bool IsRequired { get; set; }
    public string SelectedOption { get; set; }
    public int DisplayOrder { get; set; }
}

namespace DynamicFormBuilderQIA.ViewModels;

public class CreateFormViewModel
{
    public string FormTitle { get; set; }
    public List<FormFieldViewModel> Fields { get; set; } = new List<FormFieldViewModel>();
}

namespace DynamicFormBuilderQIA.Models;

public class FormField
{
    public int FieldId { get; set; }
    public int FormId { get; set; }
    public string FieldLabel { get; set; }
    public int FieldLevel { get; set; }
    public bool IsRequired { get; set; }
    public string SelectedOption { get; set; }
    public int DisplayOrder { get; set; }
}

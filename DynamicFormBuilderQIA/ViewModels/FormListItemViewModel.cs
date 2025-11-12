namespace DynamicFormBuilderQIA.ViewModels;

public class FormListItemViewModel
{
    public int FormId { get; set; }
    public string FormTitle { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int FieldCount { get; set; }
}

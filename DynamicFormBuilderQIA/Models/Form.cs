using Microsoft.AspNetCore.Http;

namespace DynamicFormBuilderQIA.Models;

public class Form
{
    public int FormId { get; set; }
    public string FormTitle { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<FormField> FormFields { get; set; } = new List<FormField>();
}

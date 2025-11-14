using DynamicFormBuilderQIA.Repository.interfaces;
using DynamicFormBuilderQIA.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilderQIA.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FormApiController : ControllerBase
{
    private readonly IFormRepository _formRepository;

    public FormApiController(IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

    [HttpGet("GetFieldOptions")]
    public async Task<IActionResult> GetFieldOptions()
    {
        try
        {
            var options = await _formRepository.GetAllFieldOptionsAsync();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving field options", error = ex.Message });
        }
    }

    [HttpPost("SaveForm")]
    public async Task<IActionResult> SaveForm([FromBody] CreateFormViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data", errors = ModelState });
            }

            if (string.IsNullOrWhiteSpace(model.FormTitle))
            {
                return BadRequest(new { message = "Form title is required" });
            }

            if (model.Fields == null || !model.Fields.Any())
            {
                return BadRequest(new { message = "At least one field is required" });
            }

            var formId = await _formRepository.SaveFormAsync(model);
            return Ok(new { success = true, formId = formId, message = "Form saved successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error saving form", error = ex.Message });
        }
    }

    //[HttpPost("GetFormsDataTable")]
    //public async Task<IActionResult> GetFormsDataTable([FromBody] DataTableRequest request)
    //{
    //    try
    //    {
    //        var (data, totalRecords, filteredRecords) = await _formRepository.GetAllFormsAsync(request);

    //        var response = new
    //        {
    //            draw = request.Draw,
    //            recordsTotal = totalRecords,
    //            recordsFiltered = filteredRecords,
    //            data = data.Select(f => new
    //            {
    //                formId = f.FormId,
    //                formTitle = f.FormTitle,
    //                createdDate = f.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
    //                fieldCount = f.FieldCount,
    //                actions = $"<a href='/Form/Preview/{f.FormId}' class='btn btn-sm btn-primary'>Preview</a> <a href='/Form/Preview/{f.FormId}' class='btn btn-sm btn-primary'> Modal Preview</a>"
    //            }).ToList()
    //        };

    //        return Ok(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { message = "Error retrieving forms", error = ex.Message });
    //    }
    //}
    [HttpPost("GetFormsDataTable")]
    public async Task<IActionResult> GetFormsDataTable([FromBody] DataTableRequest request)
    {
        try
        {
            var (data, totalRecords, filteredRecords) = await _formRepository.GetAllFormsAsync(request);
            var response = new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.Select(f => new
                {
                    formId = f.FormId,
                    formTitle = f.FormTitle,
                    createdDate = f.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                    fieldCount = f.FieldCount,
                    actions = $@"
                    <a href='/Form/Preview/{f.FormId}' class='btn btn-sm btn-primary'>
                        <i class='fas fa-eye'></i> Preview
                    </a>
                    <button type='button' class='btn btn-sm btn-info btn-modal-preview' data-form-id='{f.FormId}'>
                        <i class='fas fa-window-restore'></i> Modal Preview
                    </button>"
                }).ToList()
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving forms", error = ex.Message });
        }
    }

    [HttpGet("GetFormPreview/{formId}")]
    public async Task<IActionResult> GetFormPreview(int formId)
    {
        try
        {
            // Fetch the form with its fields
            var form = await _formRepository.GetFormByIdAsync(formId);

            if (form == null)
            {
                return NotFound(new { message = "Form not found" });
            }

            // Fetch field options (adjust based on your implementation)
            var fieldOptions = await _formRepository.GetAllFieldOptionsAsync();

            // Build HTML content
            var htmlContent = $@"
            <div class='form-info'>
                <strong>Form Title:</strong> {form.FormTitle}<br>
                <small class='text-muted'>Created: {form.CreatedDate:yyyy-MM-dd HH:mm}</small>
            </div>
            <form id='modalPreviewForm'>";

            foreach (var field in form.FormFields)
            {
                htmlContent += $@"
                <div class='mb-3'>
                    <label for='field_{field.FieldId}' class='form-label'>
                        {field.FieldLabel}
                        {(field.IsRequired ? "<span class='required-asterisk'>*</span>" : "")}
                    </label>
                    <select class='form-select' id='field_{field.FieldId}' name='field_{field.FieldId}' {(field.IsRequired ? "required" : "")}>
                        <option value=''>-- Select --</option>";

                foreach (var option in fieldOptions)
                {
                    var selected = option.OptionId.ToString() == field.SelectedOption ? "selected" : "";
                    htmlContent += $"<option value='{option.OptionId}' {selected}>{option.OptionValue}</option>";
                }

                htmlContent += @"
                    </select>
                </div>";
            }

            htmlContent += "</form>";

            return Content(htmlContent, "text/html");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error loading form preview", error = ex.Message });
        }
    }


    [HttpGet("GetFormById/{id}")]
    public async Task<IActionResult> GetFormById(int id)
    {
        try
        {
            var form = await _formRepository.GetFormByIdAsync(id);

            if (form == null)
            {
                return NotFound(new { message = "Form not found" });
            }

            return Ok(form);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving form", error = ex.Message });
        }
    }

    [HttpDelete("DeleteForm/{id}")]
    public async Task<IActionResult> DeleteForm(int id)
    {
        try
        {
            await _formRepository.DeleteFormAsync(id);
            return Ok(new { success = true, message = "Form deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error deleting form", error = ex.Message });
        }
    }
}

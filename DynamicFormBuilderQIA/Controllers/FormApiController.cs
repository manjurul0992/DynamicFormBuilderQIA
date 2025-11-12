using DynamicFormBuilderQIA.Repository.interfaces;
using DynamicFormBuilderQIA.ViewModels;
using Microsoft.AspNetCore.Http;
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

    [HttpPost("GetFormsDataTable")]
    public async Task<IActionResult> GetFormsDataTable([FromBody] DataTableRequest request)
    {
        try
        {
            var (data, totalRecords, filteredRecords) = await _formRepository.GetAllFormsAsync(request);

            var response = new DataTableResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = filteredRecords,
                Data = data.Select(f => new
                {
                    f.FormId,
                    f.FormTitle,
                    CreatedDate = f.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                    f.FieldCount,
                    Actions = $"<a href='/Form/Preview/{f.FormId}' class='btn btn-sm btn-primary'>Preview</a>"
                }).ToList<object>()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving forms", error = ex.Message });
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

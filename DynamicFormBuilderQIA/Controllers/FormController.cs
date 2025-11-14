using DynamicFormBuilderQIA.Repository.interfaces;
using DynamicFormBuilderQIA.Services.Abstraction;
using DynamicFormBuilderQIA.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilderQIA.Controllers;

public class FormController : Controller
{
    //private readonly IFormRepository _formRepository;

    //public FormController(IFormRepository formRepository)
    //{
    //    _formRepository = formRepository;
    //}
    private readonly IFormService _formService;

    public FormController(IFormService formService)
    {
        _formService = formService;
    }

    public async Task<IActionResult> Create()
     // GET: Form/Create
    {
        ViewBag.FieldOptions = await _formService.GetAllFieldOptionsAsync();
        return View();
    }

    // POST: Form/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.FieldOptions = await _formService.GetAllFieldOptionsAsync();
            return View(model);
        }

        try
        {
            var formId = await _formService.CreateFormAsync(model);
            TempData["SuccessMessage"] = "Form created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.FieldOptions = await _formService.GetAllFieldOptionsAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error saving form: " + ex.Message);
            ViewBag.FieldOptions = await _formService.GetAllFieldOptionsAsync();
            return View(model);
        }
    }

    // GET: Form/Index (List all forms)
    public IActionResult Index() => View();
    //{
    //    return View();
    //}

    // GET: Form/Preview/5
    public async Task<IActionResult> Preview(int id)
    {
        try
        {
            var form = await _formService.GetFormByIdAsync(id);
            ViewBag.FieldOptions = await _formService.GetAllFieldOptionsAsync();
            return View(form);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Form not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error loading form: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Form/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _formService.DeleteFormAsync(id);
            TempData["SuccessMessage"] = "Form deleted successfully!";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Form not found.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error deleting form: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
    //public async Task<IActionResult> GetDropdown(string name)
    //{
    //    return ViewComponent("FieldDropdown", new { name });
    //}

}
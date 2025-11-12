using DynamicFormBuilderQIA.Repository.interfaces;
using DynamicFormBuilderQIA.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilderQIA.Controllers;

public class FormController : Controller
{
    private readonly IFormRepository _formRepository;

    public FormController(IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

   public async Task<IActionResult> Create()
     // GET: Form/Create
    {
        ViewBag.FieldOptions = await _formRepository.GetAllFieldOptionsAsync();
        return View();
    }

    // POST: Form/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.FieldOptions = await _formRepository.GetAllFieldOptionsAsync();
            return View(model);
        }

        try
        {
            var formId = await _formRepository.SaveFormAsync(model);
            TempData["SuccessMessage"] = "Form created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error saving form: " + ex.Message);
            ViewBag.FieldOptions = await _formRepository.GetAllFieldOptionsAsync();
            return View(model);
        }
    }

    // GET: Form/Index (List all forms)
    public IActionResult Index()
    {
        return View();
    }

    // GET: Form/Preview/5
    public async Task<IActionResult> Preview(int id)
    {
        try
        {
            var form = await _formRepository.GetFormByIdAsync(id);

            if (form == null)
            {
                TempData["ErrorMessage"] = "Form not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FieldOptions = await _formRepository.GetAllFieldOptionsAsync();
            return View(form);
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
            await _formRepository.DeleteFormAsync(id);
            TempData["SuccessMessage"] = "Form deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error deleting form: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
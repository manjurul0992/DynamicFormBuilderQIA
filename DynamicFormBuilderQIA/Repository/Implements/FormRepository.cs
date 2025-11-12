using DynamicFormBuilderQIA.Models;
using DynamicFormBuilderQIA.Repository.interfaces;
using DynamicFormBuilderQIA.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace DynamicFormBuilderQIA.Repository.Implements;

public class FormRepository : IFormRepository
{
    private readonly string _connectionString;

    public FormRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<FieldOption>> GetAllFieldOptionsAsync()
    {
        var options = new List<FieldOption>();

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("sp_GetAllFieldOptions", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        options.Add(new FieldOption
                        {
                            OptionId = reader.GetInt32("OptionId"),
                            OptionText = reader.GetString("OptionText"),
                            OptionValue = reader.GetString("OptionValue")
                        });
                    }
                }
            }
        }

        return options;
    }

    public async Task<int> SaveFormAsync(CreateFormViewModel model)
    {
        int formId = 0;

        var fieldsJson = JsonSerializer.Serialize(model.Fields);

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("sp_SaveForm", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@FormTitle", model.FormTitle);
                command.Parameters.AddWithValue("@FormFieldsJson", fieldsJson);

                var outputParam = new SqlParameter("@FormId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                formId = (int)outputParam.Value;
            }
        }

        return formId;
    }

    public async Task<(List<FormListItemViewModel> data, int totalRecords, int filteredRecords)> GetAllFormsAsync(DataTableRequest request)
    {
        var forms = new List<FormListItemViewModel>();
        int totalRecords = 0;
        int filteredRecords = 0;

        int pageNumber = (request.Start / request.Length) + 1;
        int pageSize = request.Length;
        string searchValue = request.Search?.Value ?? string.Empty;

        string sortColumn = "FormId";
        string sortDirection = "DESC";

        if (request.Order != null && request.Order.Any())
        {
            var order = request.Order[0];
            if (request.Columns != null && order.Column < request.Columns.Count)
            {
                sortColumn = request.Columns[order.Column].Data ?? "FormId";
                sortDirection = order.Dir?.ToUpper() ?? "DESC";
            }
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("sp_GetAllForms", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@PageNumber", pageNumber);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.Parameters.AddWithValue("@SearchValue", searchValue);
                command.Parameters.AddWithValue("@SortColumn", sortColumn);
                command.Parameters.AddWithValue("@SortDirection", sortDirection);

                var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(totalRecordsParam);

                var filteredRecordsParam = new SqlParameter("@FilteredRecords", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(filteredRecordsParam);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        forms.Add(new FormListItemViewModel
                        {
                            FormId = reader.GetInt32("FormId"),
                            FormTitle = reader.GetString("FormTitle"),
                            CreatedDate = reader.GetDateTime("CreatedDate"),
                            ModifiedDate = reader.GetDateTime("ModifiedDate"),
                            FieldCount = reader.GetInt32("FieldCount")
                        });
                    }
                }

                totalRecords = (int)totalRecordsParam.Value;
                filteredRecords = (int)filteredRecordsParam.Value;
            }
        }

        return (forms, totalRecords, filteredRecords);
    }

    public async Task<Form> GetFormByIdAsync(int formId)
    {
        Form form = null;

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("sp_GetFormById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FormId", formId);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // First result set: Form details
                    if (await reader.ReadAsync())
                    {
                        form = new Form
                        {
                            FormId = reader.GetInt32("FormId"),
                            FormTitle = reader.GetString("FormTitle"),
                            CreatedDate = reader.GetDateTime("CreatedDate"),
                            ModifiedDate = reader.GetDateTime("ModifiedDate")
                        };
                    }

                    // Second result set: Form fields
                    if (await reader.NextResultAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            form.FormFields.Add(new FormField
                            {
                                FieldId = reader.GetInt32("FieldId"),
                                FormId = reader.GetInt32("FormId"),
                                FieldLabel = reader.GetString("FieldLabel"),
                                FieldLevel = reader.GetInt32("FieldLevel"),
                                IsRequired = reader.GetBoolean("IsRequired"),
                                SelectedOption = reader.IsDBNull("SelectedOption") ? null : reader.GetString("SelectedOption"),
                                DisplayOrder = reader.GetInt32("DisplayOrder")
                            });
                        }
                    }
                }
            }
        }

        return form;
    }

    public async Task DeleteFormAsync(int formId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand("sp_DeleteForm", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FormId", formId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}

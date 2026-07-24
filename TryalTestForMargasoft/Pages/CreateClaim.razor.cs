using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using TryalTestForMargasoftShared.Lookups;
using TryalTestForMargasoftShared.MedicalClaims;

namespace TryalTestForMargasoft.Pages;

public partial class CreateClaim
{
    private readonly Dictionary<string, string> validationErrors = [];
    private CreateMedicalClaimRequest model = NewClaim();
    private IReadOnlyCollection<HospitalResponse> hospitals = [];
    private IReadOnlyCollection<InsuranceCompanyResponse> insuranceCompanies = [];
    private MedicalClaimResponse? createdClaim;
    private string? errorMessage;
    private bool isLoadingLookups;
    private bool isSubmitting;

    [Inject]
    private HttpClient Http { get; set; } = null!;

    private static string TodayValue => DateOnly
        .FromDateTime(DateTime.Today)
        .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private string? DateOfServiceValue => model.DateOfService == default
        ? null
        : model.DateOfService.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private decimal OpenBalance => Math.Max(model.AmountBilled - model.AmountPaid, 0);

    private decimal? UnderpaymentAmount => model.ExpectedPaymentAmount is null
        ? null
        : Math.Max(model.ExpectedPaymentAmount.Value - model.AmountPaid, 0);

    protected override async Task OnInitializedAsync()
    {
        await LoadLookupsAsync();
    }

    private async Task LoadLookupsAsync()
    {
        isLoadingLookups = true;
        errorMessage = null;

        try
        {
            var hospitalTask = Http.GetFromJsonAsync<IReadOnlyCollection<HospitalResponse>>("api/hospitals");
            var insuranceTask = Http.GetFromJsonAsync<IReadOnlyCollection<InsuranceCompanyResponse>>(
                "api/insurance-companies");

            await Task.WhenAll(hospitalTask, insuranceTask);
            hospitals = hospitalTask.Result ?? [];
            insuranceCompanies = insuranceTask.Result ?? [];
        }
        catch (HttpRequestException)
        {
            errorMessage =
                "Hospital and insurance company options could not be loaded. Start the API project and refresh the page.";
        }
        finally
        {
            isLoadingLookups = false;
        }
    }

    private async Task SubmitAsync()
    {
        errorMessage = null;
        if (!Validate())
        {
            errorMessage = "Review the highlighted fields before creating the claim.";
            return;
        }

        isSubmitting = true;

        try
        {
            using var response = await Http.PostAsJsonAsync("api/medical-claims", model);
            if (response.IsSuccessStatusCode)
            {
                createdClaim = await response.Content.ReadFromJsonAsync<MedicalClaimResponse>();
                if (createdClaim is null)
                {
                    errorMessage = "The claim was created, but its confirmation could not be loaded.";
                }

                return;
            }

            var apiError = await response.Content.ReadFromJsonAsync<ApiError>();
            errorMessage = apiError?.Error ?? (response.StatusCode == HttpStatusCode.BadRequest
                ? "The claim details were not accepted. Review the form and try again."
                : "The claim could not be created. Try again.");
        }
        catch (HttpRequestException)
        {
            errorMessage = "The claim service could not be reached. Start the API project and try again.";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private bool Validate()
    {
        validationErrors.Clear();

        AddRequiredText(nameof(model.ClaimNumber), model.ClaimNumber, "Enter a claim number.");
        AddRequiredText(nameof(model.PatientIdentifier), model.PatientIdentifier, "Enter a patient identifier.");

        if (model.HospitalId <= 0)
        {
            validationErrors[nameof(model.HospitalId)] = "Select a hospital.";
        }

        if (model.InsuranceCompanyId <= 0)
        {
            validationErrors[nameof(model.InsuranceCompanyId)] = "Select an insurance company.";
        }

        if (model.DateOfService == default)
        {
            validationErrors[nameof(model.DateOfService)] = "Enter the date of service.";
        }
        else if (model.DateOfService > DateOnly.FromDateTime(DateTime.Today))
        {
            validationErrors[nameof(model.DateOfService)] = "Date of service cannot be in the future.";
        }

        if (model.DateClaimSubmitted < model.DateOfService)
        {
            validationErrors[nameof(model.DateClaimSubmitted)] =
                "Submitted date cannot be before the date of service.";
        }

        AddNonNegative(nameof(model.AmountBilled), model.AmountBilled, "Amount billed cannot be negative.");
        AddNonNegative(nameof(model.AmountPaid), model.AmountPaid, "Amount paid cannot be negative.");
        if (model.ExpectedPaymentAmount is not null)
        {
            AddNonNegative(
                nameof(model.ExpectedPaymentAmount),
                model.ExpectedPaymentAmount.Value,
                "Expected payment cannot be negative.");
        }

        return validationErrors.Count == 0;
    }

    private void AddRequiredText(string field, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationErrors[field] = message;
        }
    }

    private void AddNonNegative(string field, decimal value, string message)
    {
        if (value < 0)
        {
            validationErrors[field] = message;
        }
    }

    private string FieldClass(string field)
    {
        var baseClass = field is nameof(model.HospitalId) or nameof(model.InsuranceCompanyId)
            ? "form-select"
            : "form-control";

        return validationErrors.ContainsKey(field) ? $"{baseClass} is-invalid" : baseClass;
    }

    private RenderFragment FieldError(string field) => builder =>
    {
        if (validationErrors.TryGetValue(field, out var message))
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "field-error");
            builder.AddContent(2, message);
            builder.CloseElement();
        }
    };

    private static string FormatInsuranceCompany(InsuranceCompanyResponse company)
    {
        return string.IsNullOrWhiteSpace(company.PayerCode)
            ? company.Name
            : $"{company.Name} ({company.PayerCode})";
    }

    private static string FormatMoney(decimal value)
    {
        return value.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }

    private void CreateAnother()
    {
        model = NewClaim();
        createdClaim = null;
        errorMessage = null;
        validationErrors.Clear();
    }

    private static CreateMedicalClaimRequest NewClaim()
    {
        return new CreateMedicalClaimRequest
        {
            DateOfService = DateOnly.FromDateTime(DateTime.Today),
            AmountBilled = 0,
            AmountPaid = 0,
            DocumentationComplete = false,
            Status = "New"
        };
    }

    private sealed class ApiError
    {
        public string? Error { get; set; }
    }
}

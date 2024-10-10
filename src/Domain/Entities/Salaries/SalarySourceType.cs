namespace Domain.Entities.Salaries;

public enum SalarySourceType
{
    Undefined = 0,

    /// <summary>
    /// Kolesa Group developers salary survey, 2022.
    /// </summary>
    KolesaDevelopersCsv2022 = 1,

    /// <summary>
    /// Kolesa Group Data-specialists salary survey, 2024.
    /// </summary>
    KolesaDataAnalystCsv2024 = 2,
}
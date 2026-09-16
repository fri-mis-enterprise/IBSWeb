using IBS.DataAccess.Data;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsPayable;
using IBS.Models.Filpride.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBS.Services
{
    public class CheckVoucherDocumentationService
    {
        private readonly ApplicationDbContext _db;

        public CheckVoucherDocumentationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task PrepareAsync(
            CheckVoucherDocumentationViewModel form,
            string? documentType,
            string? retainedCompanyName,
            CancellationToken cancellationToken)
        {
            form.DocumentType = documentType;
            form.Companies = await _db.Companies
                .AsNoTracking()
                .Where(company => company.IsActive)
                .OrderBy(company => company.CompanyCode)
                .ThenBy(company => company.CompanyName)
                .Select(company => new SelectListItem
                {
                    Value = company.CompanyName,
                    Text = company.CompanyCode + " " + company.CompanyName
                })
                .ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(retainedCompanyName)
                && form.Companies.All(company => company.Value != retainedCompanyName))
            {
                form.Companies.Insert(0, new SelectListItem
                {
                    Value = retainedCompanyName,
                    Text = retainedCompanyName + " (historical)"
                });
            }
        }

        public async Task<string?> ValidateAndNormalizeAsync(
            string? documentType,
            CheckVoucherDocumentationViewModel form,
            string? retainedCompanyName,
            CancellationToken cancellationToken)
        {
            form.DocumentType = documentType;

            if (documentType == nameof(DocumentType.Documented))
            {
                form.IsDocumentedByOtherCompany = null;
                form.DocumentedByCompanyName = null;
                return null;
            }

            if (documentType != nameof(DocumentType.Undocumented))
            {
                return "Select a valid document type.";
            }

            if (!form.IsDocumentedByOtherCompany.HasValue)
            {
                return "Specify whether this voucher is documented by another company.";
            }

            if (!form.IsDocumentedByOtherCompany.Value)
            {
                form.DocumentedByCompanyName = null;
                return null;
            }

            if (string.IsNullOrWhiteSpace(form.DocumentedByCompanyName))
            {
                return "Select the company that documents this voucher.";
            }

            if (form.DocumentedByCompanyName == retainedCompanyName)
            {
                return null;
            }

            string? companyName = await _db.Companies
                .AsNoTracking()
                .Where(company => company.IsActive && company.CompanyName == form.DocumentedByCompanyName)
                .Select(company => company.CompanyName)
                .FirstOrDefaultAsync(cancellationToken);

            if (companyName == null)
            {
                return "Select an active company from the Company master file.";
            }

            form.DocumentedByCompanyName = companyName;
            return null;
        }

        public static void Apply(
            FilprideCheckVoucherHeader header,
            CheckVoucherDocumentationViewModel form)
        {
            header.IsDocumentedByOtherCompany = form.IsDocumentedByOtherCompany;
            header.DocumentedByCompanyName = form.DocumentedByCompanyName;
        }

        public static CheckVoucherDocumentationViewModel FromHeader(FilprideCheckVoucherHeader header)
        {
            return new CheckVoucherDocumentationViewModel
            {
                DocumentType = header.Type,
                IsDocumentedByOtherCompany = header.IsDocumentedByOtherCompany,
                DocumentedByCompanyName = header.DocumentedByCompanyName
            };
        }
    }
}

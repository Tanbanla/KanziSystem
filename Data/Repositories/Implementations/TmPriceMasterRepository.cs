using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Globalization;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmPriceMasterRepository: BaseRepository<TM_PRICE_MASTER, long>, ITmPriceMasterRepository
    {

        private readonly COST_MANAGEMENTContext _priceMasterContext;
        public TmPriceMasterRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _priceMasterContext = context;
        }

        public async Task<List<TM_PRICE_MASTER>> GetByInternalPartCode(string internalPartCode)
        {
            return await _priceMasterContext.TM_PRICE_MASTERs
                .Where(p => p.CHR_INTERNAL_PART_CODE == internalPartCode)
                .OrderByDescending(p => p.DTM_UPDATE ?? p.DTM_CREATE)
                .ThenByDescending(p => p.DTM_UPLOAD)
                .ThenByDescending(p => p.ID)
                .ToListAsync();
        }
        public async Task<List<TM_PRICE_MASTER>> InsertByDetailQuotation(List<BaoGia_Detail_of_Quotation> details)
        {
            if (details == null || !details.Any())
            {
                throw new ArgumentException("Details list cannot be null or empty.", nameof(details));
            }

            var requestIds = details.Select(x => x.ID_RequestQuote).Distinct().ToList();
            var requests = await _priceMasterContext.BaoGia_Request_of_Quotations
                .Where(x => requestIds.Contains(x.ID))
                .ToDictionaryAsync(x => x.ID);

            var missingRequestId = requestIds.FirstOrDefault(x => !requests.ContainsKey(x));
            if (missingRequestId != 0)
            {
                throw new InvalidOperationException(
                    $"BaoGia_Request_of_Quotation with ID {missingRequestId} not found.");
            }

            var now = DateTime.Now;
            var priceMasters = details.Select(detail =>
            {
                var request = requests[detail.ID_RequestQuote];
                var unitPrice = detail.FL_VND ?? detail.FL_USD;
                var currency = detail.FL_VND.HasValue ? "VND" : detail.FL_USD.HasValue ? "USD" : null;

                if (string.IsNullOrWhiteSpace(request.CHR_MaHangNoiBo))
                {
                    throw new InvalidOperationException(
                        $"Quotation detail {detail.ID} does not have an internal part code.");
                }

                if (string.IsNullOrWhiteSpace(detail.CHR_CodeNCC) ||
                    string.IsNullOrWhiteSpace(detail.NVCHR_NameNCC))
                {
                    throw new InvalidOperationException(
                        $"Quotation detail {detail.ID} does not have complete vendor information.");
                }

                return new TM_PRICE_MASTER
                {
                    DTM_UPLOAD = now,
                    CHR_UPLOAD_USERID = detail.CHR_UpdateBy ?? detail.CHR_CreateBy,
                    CHR_QUOTATION_REQUEST_NO = request.CHR_MaDon,
                    INT_QUOTATION_DETAIL = detail.ID,
                    CHR_EQUIPMENT_CODE = request.CHR_MaThietBi,
                    CHR_INTERNAL_PART_CODE = request.CHR_MaHangNoiBo,
                    CHR_VENDOR_PART_CODE = detail.CHR_MaHangNCC,
                    NVCHR_PART_NAME_VN = detail.NVCHR_TenHangHQ ?? request.NVCHR_NameVN,
                    NVCHR_PART_NAME_EN = detail.CHR_NameEN,
                    NVCHR_UNIT = detail.NVCHR_DonVi ?? request.NVCHR_DonVi,
                    NVCHR_OTHER_REQUIREMENT = detail.NVCHR_Note,
                    NVCHR_MAKER_ORIGIN = request.NVCHR_NhaSanXuat,
                    DEC_QUANTITY = detail.INT_SoLuong ?? request.INT_SoLuong,
                    CHR_VENDOR_CODE = detail.CHR_CodeNCC,
                    NVCHR_VENDOR_NAME = detail.NVCHR_NameNCC,
                    DEC_UNIT_PRICE = unitPrice.HasValue ? Convert.ToDecimal(unitPrice.Value) : null,
                    CHR_CURRENCY = currency,
                    DEC_UNIT_PRICE_USD = detail.FL_USD.HasValue
                        ? Convert.ToDecimal(detail.FL_USD.Value)
                        : null,
                    INT_LEAD_TIME_DAY = int.TryParse(detail.DTM_LeadTime, out var leadTime)
                        ? leadTime
                        : null,
                    DEC_MOQ = decimal.TryParse(
                        detail.NVCHR_MOQ,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var moq)
                        ? moq
                        : null,
                    NVCHR_REMARK = detail.NVCHR_Note,
                    NVCHR_DELIVERY_TERM = detail.NVCHR_DeliveryTerm,
                    DEC_VAT_PERCENT = detail.FL_TaxRate.HasValue
                        ? Convert.ToDecimal(detail.FL_TaxRate.Value)
                        : null,
                    NVCHR_PAYMENT_TERM = detail.NVCHR_PaymentTerm,
                    DTM_PRICE_EFFECTIVE = detail.DTM_EffectiveDate,
                    DTM_PRICE_EXPIRATION = detail.DTM_ExpiryDate ?? detail.DTM_EndDate,
                    NVCHR_QTN_LINK = detail.NVCHR_File,
                    CHR_CRT_USERID = detail.CHR_CreateBy,
                    DTM_CREATE = now
                };
            }).ToList();

            await using var transaction = await _priceMasterContext.Database.BeginTransactionAsync();
            try
            {
                await _priceMasterContext.TM_PRICE_MASTERs.AddRangeAsync(priceMasters);
                await _priceMasterContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return priceMasters;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task InsertImportedAsync(IReadOnlyCollection<TM_PRICE_MASTER> priceMasters)
        {
            if (priceMasters.Count == 0) return;

            await using var transaction = await _priceMasterContext.Database.BeginTransactionAsync();
            try
            {
                var internalPartCodes = priceMasters
                    .Where(x => !string.IsNullOrWhiteSpace(x.CHR_INTERNAL_PART_CODE))
                    .Select(x => x.CHR_INTERNAL_PART_CODE)
                    .Distinct()
                    .ToList();

                var materials = await _priceMasterContext.MATERIALs
                    .Where(x => internalPartCodes.Contains(x.Material_Code))
                    .ToDictionaryAsync(x => x.Material_Code);

                foreach (var priceMaster in priceMasters)
                {
                    if (!materials.TryGetValue(priceMaster.CHR_INTERNAL_PART_CODE, out var material))
                        continue;

                    if (!string.IsNullOrWhiteSpace(priceMaster.NVCHR_PART_NAME_VN))
                        material.Material_Name_VN = priceMaster.NVCHR_PART_NAME_VN;
                    if (!string.IsNullOrWhiteSpace(priceMaster.NVCHR_PART_NAME_EN))
                        material.Material_Name_EN = priceMaster.NVCHR_PART_NAME_EN;
                    if (!string.IsNullOrWhiteSpace(priceMaster.NVCHR_UNIT))
                        material.Unit = priceMaster.NVCHR_UNIT;
                    if (priceMaster.DEC_UNIT_PRICE.HasValue)
                        material.Price = (double)priceMaster.DEC_UNIT_PRICE.Value;
                    if (!string.IsNullOrWhiteSpace(priceMaster.CHR_CURRENCY))
                        material.Currency = priceMaster.CHR_CURRENCY;
                }

                await _priceMasterContext.TM_PRICE_MASTERs.AddRangeAsync(priceMasters);
                await _priceMasterContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<TM_PRICE_MASTER> UpdatePriceMaster(TM_PRICE_MASTER tm)
        {
            var existingPriceMaster = await _priceMasterContext.TM_PRICE_MASTERs.FindAsync(tm.ID);
            if (existingPriceMaster != null)
            {
                _priceMasterContext.Entry(existingPriceMaster).CurrentValues.SetValues(tm);
                await _priceMasterContext.SaveChangesAsync();
                return existingPriceMaster;
            }
            else
            {
                throw new Exception($"TM_PRICE_MASTER with ID {tm.ID} not found.");
            }
        }
    }
}

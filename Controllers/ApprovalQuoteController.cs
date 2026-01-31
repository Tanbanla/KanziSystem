using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.ApprovalQuote;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ApprovalQuoteController : BaseAuthController
    {
        private readonly ILogger<ApprovalQuoteController> _logger;
        private readonly IHistoryApproverServive _historyApproverServive;
        private readonly INhomViTriService _nhomViTriService;
        private readonly IMaterialService _materialService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        public ApprovalQuoteController(ILogger<ApprovalQuoteController> logger,
            IHistoryApproverServive historyApproverServive, INhomViTriService nhomViTriService, IMaterialService materialService,
            IBaoGiaService baoGiaService, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaStatusService baoGiaStatusService
            , IBaoGiaStepService baoGiaStepService)
        {
            _logger = logger;
            _historyApproverServive = historyApproverServive;
            _nhomViTriService = nhomViTriService;
            _materialService = materialService;
            _baoGiaService = baoGiaService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaStepService = baoGiaStepService;
        }
        public async Task<IActionResult> Index()
        {
            var nhomViTriList = await GetNhomViTriList();
            var materialList = await GetMaterialList();
            var soDonList = await GetSoDonList();
            var statusBaoGiaList = await GetStatusBaoGiaList();
            var stepBaoGiaList = await GetStepBaoGiaList();
            var vm = new ApprovalQuoteViewModel
            {
                listNhomVitri = nhomViTriList,
                listMaterial = materialList,
                listSoDon = soDonList,
                listStatusBaoGia = statusBaoGiaList,
                listStepBaoGia = stepBaoGiaList
            };
            return View(vm);
        }
        // Lay thong tin phong ban
        public async Task<List<ACC_NHOMVITRIDTO>> GetNhomViTriList()
        {
            try
            {
                var result = await _nhomViTriService.GetAllAsync();
                return (List<ACC_NHOMVITRIDTO>)result.Data;
            }
            catch (Exception ex)
            {
                return new List<ACC_NHOMVITRIDTO>();
            }
        }
        // Lay thong tin hang hoa
        public async Task<List<MATERIALDTO>> GetMaterialList()
        {
            try
            {
                var result = await _materialService.SearchAsync("", "", "", 0, 0);
                return result.Data;
            }
            catch (Exception ex)
            {
                return new List<MATERIALDTO>();
            }
        }
        // Lay so don
        public async Task<List<string>> GetSoDonList()
        {
            try
            {
                var result = await _baoGiaService.GetListMaDonBGAsync();
                return result.Data;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
        // Lay status bao gia
        public async Task<List<BaoGia_StatusDTO>> GetStatusBaoGiaList()
        {
            try
            {
                var result = await _baoGiaStatusService.GetAllAsync();
                return (List<BaoGia_StatusDTO>)result.Data;
            }
            catch (Exception ex)
            {
                return new List<BaoGia_StatusDTO>();
            }
        }
        // Lay step bao gia
        public async Task<List<BaoGia_StepDTO>> GetStepBaoGiaList()
        {
            try
            {
                var result = await _baoGiaStepService.GetStepsApproverAsync();
                return (List<BaoGia_StepDTO>)result.Data;
            }
            catch (Exception ex)
            {
                return new List<BaoGia_StepDTO>();
            }
        }
        // Search đơn phê duyệt báo giá
        [HttpPost]
        public async Task<IActionResult> SearchApprovalQuote([FromBody] ApprovalQuoteSearchViewModel searchModel)
        {
            try
            {
                var adid = GetCurrentUserId() ?? string.Empty;
                var result = await _historyApproverServive.GetListApprover(
                    adid, searchModel.SoDon, searchModel.MaHang, searchModel.Section, searchModel.StatusApprover
                );
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Lưu thông tin phê duyệt báo giá
        [HttpPost]
        public async Task<IActionResult> SaveApprovalQuote([FromBody] List<BaoGia_History_Approver_of_QuotationDTO> saveModel)
        {
            try
            {
                var adid = GetCurrentUserId() ?? string.Empty;
                var result = await _historyApproverServive.AddHistoryListAsync(saveModel);
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Cập nhật thông tin đơn báo giá
        // case OK
        [HttpPost]
        public async Task<IActionResult> UpdateQuotationOK([FromBody] List<BaoGia_Request_of_QuotationDTO> updateModel)
        {
            try
            {
                var result = await _baoGiaService.CapNhatDanhSachBGAsync(updateModel);
                if (result.Success)
                {
                    var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                    // Luu lich su phe duyet
                    var approverHistories = insertedList.Select(b => new BaoGia_History_Approver_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        ID_BaoGiaStep = b.ID_StepBaoGia-1 ?? 0,
                        CHR_UserSendApprover = GetCurrentUserId() ?? string.Empty,
                        DTM_UserSendApprover   = DateTime.Now,
                        CHR_UserApprover   = GetCurrentUserId() ?? string.Empty,
                        DTM_UserApprover = DateTime.Now,
                        CHR_StatusFlag = "1",
                        BIT_SendMail = false,
                        NVCHR_ReturnReason = null,
                        BIT_Return = false,
                        CHR_SectionCodeSend = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameSend = GetCurrentUserDepartment() ?? string.Empty,
                        CHR_SectionCodeApprover = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameApprover = GetCurrentUserDepartment() ?? string.Empty
                    }).ToList();
                    if (approverHistories.Any())
                    {
                       await _historyApproverServive.AddHistoryListAsync(approverHistories);
                    }
                    // Luu lich su thay doi trang thai bao gia
                    var histories = insertedList.Select(b => new BaoGia_History_Request_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        CHR_MaDon = b.CHR_MaDon ?? string.Empty,
                        CHR_UpdateBy = GetCurrentUserId() ?? string.Empty,
                        NVCHR_UpdateName = GetCurrentUserFullName() ?? string.Empty,
                        CHR_Updatedate = DateTime.Now,
                        CHR_ChangedColumns = null,
                        CHR_OldData = null,
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(b),
                        NVCHR_LyDo = b.NVCHR_LyDo,
                        CHR_ActionType = StatusOld(b.ID_StepBaoGia)
                    }).ToList();

                    if (histories.Any())
                    {
                       await _baoGiaHistoryService.InsertHistoryListAsync(histories);
                    }

                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // case NG
        [HttpPost]
        public async Task<IActionResult> UpdateQuotationNG([FromBody] List<BaoGia_Request_of_QuotationDTO> updateModel)
        {
            try
            {
                var result = await _baoGiaService.CapNhatDanhSachBGAsync(updateModel);
                if (result.Success)
                {
                    var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                    // Luu lich su phe duyet
                    var approverHistories = insertedList.Select(b => new BaoGia_History_Approver_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        ID_BaoGiaStep = StepOld(b.ID_Status),
                        CHR_UserSendApprover = GetCurrentUserId() ?? string.Empty,
                        DTM_UserSendApprover = DateTime.Now,
                        CHR_UserApprover = GetCurrentUserId() ?? string.Empty,
                        DTM_UserApprover = DateTime.Now,
                        CHR_StatusFlag = "1",
                        BIT_SendMail = false,
                        NVCHR_ReturnReason = b.NVCHR_LyDo,
                        BIT_Return = true,
                        CHR_SectionCodeSend = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameSend = GetCurrentUserDepartment() ?? string.Empty,
                        CHR_SectionCodeApprover = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameApprover = GetCurrentUserDepartment() ?? string.Empty
                    }).ToList();
                    if (approverHistories.Any())
                    {
                        await _historyApproverServive.AddHistoryListAsync(approverHistories);
                    }
                    // Luu lich su thay doi trang thai bao gia
                    var histories = insertedList.Select(b => new BaoGia_History_Request_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        CHR_MaDon = b.CHR_MaDon ?? string.Empty,
                        CHR_UpdateBy = GetCurrentUserId() ?? string.Empty,
                        NVCHR_UpdateName = GetCurrentUserFullName() ?? string.Empty,
                        CHR_Updatedate = DateTime.Now,
                        CHR_ChangedColumns = null,
                        CHR_OldData = null,
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(b),
                        NVCHR_LyDo = b.NVCHR_LyDo,
                        CHR_ActionType = b.ID_Status
                    }).ToList();

                    if (histories.Any())
                    {
                       await _baoGiaHistoryService.InsertHistoryListAsync(histories);
                    }
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private int StepOld(string? status)
        {
            switch (status)
            {
                case "RETURN_QLSC": return 2; // Trả về QLSC phong ban
                case "RETURN_QLTC": return 3; // Trả về QLTC phong ban
                case "RETURN_PIC": return 4;    // Trả về PIC phong mua hang
                case "RETURN_QLSC_1": return 5; // Trả về QLSC phong ban mua hang
                default: return 1;
            }
        }
        private string StatusOld(int? step)
        {
            switch (step)
            {
                case 3: return "QLSC"; 
                case 4: return "QLTC"; 
                case 5: return "PIC";    
                case 6: return "QLSC_1"; 
                default: return "CREATE";
            }
        }
    }
}

using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System.Collections.Generic;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class MaterialService: BaseService<MATERIAL, int, MATERIALDTO>, IMaterialService
    {
        private readonly IMaterialRepository _repo;
        private readonly IWebHostEnvironment _env;
        public MaterialService(IMaterialRepository repository, IMapper mapper, IWebHostEnvironment env): base(repository, mapper)
        {
            _repo = repository;
            _env = env;
        }
        // Lấy theo mã hàng
        public async Task<GenericResponse<MATERIALDTO>> GetByMaHangAsync(string maHang)
        {
            var result = new GenericResponse<MATERIALDTO>();
            try
            {
                var material = await _repo.GetByMaHangAsync(maHang);
                result.Data = _mapper.Map<MATERIALDTO>(material);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Tìm kiếm hàng hóa và phân trang
        public async Task<GenericResponse<List<MATERIALDTO>>> SearchAsync(string? MaHang, string? Name, string? NhomHang, int? pageIndex, int? pageSize)
        {
            var result = new GenericResponse<List<MATERIALDTO>>();
            try
            {
                var materials = await _repo.SearchAsync(MaHang, Name, NhomHang, pageIndex, pageSize);
                result.Data = _mapper.Map<List<MATERIALDTO>>(materials);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy danh sách hàng hóa
        public async Task<GenericResponse<List<MATERIALDTO>>> GetMaterialsByNameOrCodeAsync(string keyword)
        {
            var result = new GenericResponse<List<MATERIALDTO>>();
            try
            {
                var materials = await _repo.GetMaterialsByNameOrCodeAsync(keyword);
                result.Data = _mapper.Map<List<MATERIALDTO>>(materials);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        public async Task<GenericResponse<List<dynamic>>> GetListMaterial()
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                var materials = await _repo.GetListMaterial();
                result.Data = materials;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update danh sách linh kiện
        public async Task<GenericResponse<bool>> UpdateMaterialAsync(List<MATERIALDTO> materials)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var materialsToUpdate = _mapper.Map<List<MATERIAL>>(materials);
                await _repo.UpdateMaterialAsync(materialsToUpdate);
                result.Data = true;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // update thông tin danh sách linh kiện
        public async Task<GenericResponse<bool>> UpdateListThongTin(List<MATERIALDTO> listDTO)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var materials = _mapper.Map<List<MATERIAL>>(listDTO);
                // You may need to implement this method in IMaterialRepository if not already present
                var updateResult = await _repo.UpdateMaterialAsync(materials);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        // check mã linh kiện 
        public async Task<GenericResponse<bool>> CheckMaHangExistsAsync(string codeMaterial)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var exists = await _repo.CheckMaHangExistsAsync(codeMaterial);
                result.Data = exists;
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        // Insert 
        public async Task<GenericResponse<bool>> InsertMaterial(MATERIALDTO mt)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var dto = _mapper.Map<MATERIAL>(mt);
                result.Data = await _repo.InsertMaterial(dto);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Insert nhiều cho ma hang No list
        public async Task<GenericResponse<bool>> UpdateListThongTinNoList(List<MATERIALDTO> listMT)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var materials = _mapper.Map<List<MATERIAL>>(listMT);
                var updateResult = await _repo.UpdateListThongTinNoList(materials);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // lay ma hang hien tai
        public async Task<GenericResponse<string>> MaterialCodeLater(string type)
        {
            var result = new GenericResponse<string>();
            try
            {
                result.Data = await _repo.MaterialCodeLater(type);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Message =ex.Message;
                result.Success = false;
            }
            return result;
        }
        // check ma hang
        public async Task<GenericResponse<string>> CheckMaterialCode(string codeNcc, string category, string NameEN)
        {
            var result = new GenericResponse<string>();
            try
            {
                result.Data = await _repo.CheckMaterialCode(codeNcc, category, NameEN);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Search date by Material View
        public async Task<GenericResponse<ListRequest<MATERIAL>>> SearchDateByMaterialViewAsync(SearchMaterialVM search)
        {
            var result = new GenericResponse<ListRequest<MATERIAL>>();
            try
            {
                var materials = await _repo.SearchDateByMaterialViewAsync(search);
                result.Data = materials;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Export danh sách linh kiện
        public async Task<GenericResponse<(byte[] FileBytes, string FileName, string ContentType)>> ExportMaterialViewToExcelAsync(SearchMaterialVM search)
        {
            var result = new GenericResponse<(byte[], string, string)>();

            try
            {
                var dataAsync = await _repo.SearchDateByMaterialViewAsync(search);
                var materials = dataAsync.Data;

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "MaterialMaster.xlsx");

                if (!System.IO.File.Exists(templatePath))
                {
                    result.Success = false;
                    result.Message = "Không tìm thấy file template";
                    return result;
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);

                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    result.Success = false;
                    result.Message = "Không tìm thấy worksheet";
                    return result;
                }

                int startRow = 3;
                foreach (var m in materials)
                {
                    ws.Cell(startRow, 1).Value = GetLoaiHang(m.Material_Code);
                    ws.Cell(startRow, 2).Value = m.Material_Code;
                    ws.Cell(startRow, 3).Value = m.Code_Suppiler;
                    ws.Cell(startRow, 4).Value = m.Material_Name_VN;
                    ws.Cell(startRow, 5).Value = m.Material_Name_EN;
                    ws.Cell(startRow, 6).Value = m.Category_VN;
                    ws.Cell(startRow, 7).Value = m.Group_Code;
                    ws.Cell(startRow, 8).Value = m.Shape;
                    ws.Cell(startRow, 9).Value = m.Material1;
                    ws.Cell(startRow, 10).Value = m.Composition;
                    ws.Cell(startRow, 11).Value = m.Dimension;
                    ws.Cell(startRow, 12).Value = m.UsedFor;
                    ws.Cell(startRow, 13).Value = m.Purpose;
                    startRow++;
                }

               // ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                var fileBytes = stream.ToArray(); 

                var fileName = $"Material_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                result.Success = true;
                result.Data = (
                    fileBytes,
                    fileName,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                );
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
        private string? GetLoaiHang(string materialCode)
        {
            if (string.IsNullOrEmpty(materialCode))
                return null;
            switch (materialCode.Substring(0, 1))
            {
                case "A":
                    return "A";
                case "B":
                    return "B";
                case "C":
                    return "C";
                case "E":
                    return "E";
                case "I":
                    return "I";
                default:
                    return "NO LIST";
            }
        }
        // Delete Material
        public async Task<GenericResponse<bool>> DeleteMaterialAsync(string codeMaterial)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var deleteResult = await _repo.DeleteMaterialAsync(codeMaterial);
                result.Data = deleteResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Update Material
        public async Task<GenericResponse<bool>> UpdateMaterialAsync(MATERIALDTO mt)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<MATERIAL>(mt);
                var updateResult = await _repo.UpdateMaterialAsync(data);
                result.Data = updateResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        // delete list material
        public async Task<GenericResponse<bool>> DeleteMaterials(List<string> listCodeMaterial)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var deleteResult = await _repo.DeleteMaterials(listCodeMaterial);
                result.Data = deleteResult;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
    }
}

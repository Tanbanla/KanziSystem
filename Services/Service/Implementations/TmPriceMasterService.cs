using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmPriceMasterService: BaseService<TM_PRICE_MASTER, int, TM_PRICE_MASTERDTO>, ITmPriceMasterService
    {
        private readonly ITmPriceMasterRepository _repo;
        private readonly IMapper _mapper;
        public TmPriceMasterService(ITmPriceMasterRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<GenericResponse<List<TM_PRICE_MASTERDTO>>> GetByInternalPartCode(string internalPartCode)
        {
            var result = new GenericResponse<List<TM_PRICE_MASTERDTO>>();

            try
            {
                var data = await _repo.GetByInternalPartCode(internalPartCode);
                result.Data = _mapper.Map<List<TM_PRICE_MASTERDTO>>(data);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<GenericResponse<List<TM_PRICE_MASTERDTO>>> InsertByDetailQuotation(List<BaoGia_Detail_of_Quotation> details)
        {
            var result = new GenericResponse<List<TM_PRICE_MASTERDTO>>();
            try
            {
                var data = await _repo.InsertByDetailQuotation(details);
                result.Data = _mapper.Map<List<TM_PRICE_MASTERDTO>>(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public async Task<GenericResponse<TM_PRICE_MASTERDTO>> UpdatePriceMaster(TM_PRICE_MASTERDTO tm)
        {
            var result = new GenericResponse<TM_PRICE_MASTERDTO>();
            try
            {
                var entity = _mapper.Map<TM_PRICE_MASTER>(tm);
                var updatedEntity = await _repo.UpdatePriceMaster(entity);
                result.Data = _mapper.Map<TM_PRICE_MASTERDTO>(updatedEntity);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}

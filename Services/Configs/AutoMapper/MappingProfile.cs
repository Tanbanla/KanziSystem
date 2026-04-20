using AutoMapper;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Agent;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;

namespace PRJ_WAREHOUSE_BIVN.Services.Configs.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<IM_NCC_NEW, IM_NCC_NEWDTO>();
            CreateMap<IM_NCC_NEWDTO, IM_NCC_NEW>();

            CreateMap<BaoGia_Request_of_Quotation, BaoGia_Request_of_QuotationDTO>();
            CreateMap<BaoGia_Request_of_QuotationDTO, BaoGia_Request_of_Quotation>();

            CreateMap<MATERIAL, MATERIALDTO>();
            CreateMap<MATERIALDTO, MATERIAL>();

            CreateMap<BaoGia_Master_Approver_Send_Mail, BaoGia_Master_Approver_Send_MailDTO>();
            CreateMap<BaoGia_Master_Approver_Send_MailDTO, BaoGia_Master_Approver_Send_Mail>();
        
            CreateMap<TM_USER, TM_USERDTO>();
            CreateMap<TM_USERDTO, TM_USER>();

            CreateMap<BaoGia_Step, BaoGia_StepDTO>();
            CreateMap<BaoGia_StepDTO, BaoGia_Step>();

            CreateMap<TM_SECTIONDTO, TM_SECTION>();
            CreateMap<TM_SECTION, TM_SECTIONDTO>();

            CreateMap<ACC_NHOMVITRI, ACC_NHOMVITRIDTO>();
            CreateMap<ACC_NHOMVITRIDTO, ACC_NHOMVITRI>();

            CreateMap<BaoGia_Status, BaoGia_StatusDTO>();
            CreateMap<BaoGia_StatusDTO, BaoGia_Status>();

            CreateMap<BaoGia_Confirm_Name_Quotation, BaoGia_Confirm_Name_QuotationDTO>();
            CreateMap<BaoGia_Confirm_Name_QuotationDTO, BaoGia_Confirm_Name_Quotation>();

            CreateMap<BaoGia_NCC, BaoGia_NCCDTO>();
            CreateMap<BaoGia_NCCDTO, BaoGia_NCC>();

            CreateMap<BaoGia_History_Request_of_Quotation, BaoGia_History_Request_of_QuotationDTO>();
            CreateMap<BaoGia_History_Request_of_QuotationDTO, BaoGia_History_Request_of_Quotation>();

            CreateMap<BaoGia_History_Approver_of_Quotation, BaoGia_History_Approver_of_QuotationDTO>();
            CreateMap<BaoGia_History_Approver_of_QuotationDTO, BaoGia_History_Approver_of_Quotation>();

            CreateMap<BaoGia_Detail_of_Quotation, BaoGia_Detail_of_QuotationDTO>();
            CreateMap<BaoGia_Detail_of_QuotationDTO, BaoGia_Detail_of_Quotation>();

            CreateMap<BaoGia_NCC_Category, BaoGia_NCC_CategoryDTO>();
            CreateMap<BaoGia_NCC_CategoryDTO, BaoGia_NCC_Category>();

            CreateMap<TM_Category, TM_CategoryDTO>();
            CreateMap<TM_CategoryDTO,TM_Category>();

            CreateMap<TM_MASTER_MAILDTO, TM_MASTER_MAIL>();
            CreateMap<TM_MASTER_MAIL, TM_MASTER_MAILDTO>();

            CreateMap<TM_EMPLOYEE, TM_EMPLOYEEDTO>();
            CreateMap<TM_EMPLOYEEDTO, TM_EMPLOYEE>();

            CreateMap<DEPARTMENT, DEPARTMENTDTO>();
            CreateMap<DEPARTMENTDTO, DEPARTMENT>();

             CreateMap<EXCHANGE_RATE, EXCHANGE_RATEDTO>();
            CreateMap<EXCHANGE_RATEDTO, EXCHANGE_RATE>();
        }
    }
}

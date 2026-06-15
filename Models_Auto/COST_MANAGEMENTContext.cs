using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class COST_MANAGEMENTContext : DbContext
{
    public COST_MANAGEMENTContext()
    {
    }

    public COST_MANAGEMENTContext(DbContextOptions<COST_MANAGEMENTContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ACCOUNT_NAME> ACCOUNT_NAMEs { get; set; }

    public virtual DbSet<ACCREPORT> ACCREPORTs { get; set; }

    public virtual DbSet<ACCREPORT_COMMON> ACCREPORT_COMMONs { get; set; }

    public virtual DbSet<ACCREPORT_PROPOTIONAL> ACCREPORT_PROPOTIONALs { get; set; }

    public virtual DbSet<ACCREPORT_SUMTONTRENLINE> ACCREPORT_SUMTONTRENLINEs { get; set; }

    public virtual DbSet<ACCREPORT_TONTRENLINE_E> ACCREPORT_TONTRENLINE_Es { get; set; }

    public virtual DbSet<ACCREPORT_TONTRENLINE_VITRI> ACCREPORT_TONTRENLINE_VITRIs { get; set; }

    public virtual DbSet<ACCREPORT_TONTRENLINE_XUATKHO> ACCREPORT_TONTRENLINE_XUATKHOs { get; set; }

    public virtual DbSet<ACC_NHOMHANG> ACC_NHOMHANGs { get; set; }

    public virtual DbSet<ACC_NHOMHANG_MATERIAL> ACC_NHOMHANG_MATERIALs { get; set; }

    public virtual DbSet<ACC_NHOMVITRI> ACC_NHOMVITRIs { get; set; }

    public virtual DbSet<ACC_NHOMVITRI_DEPARTMENT_VITRI> ACC_NHOMVITRI_DEPARTMENT_VITRIs { get; set; }

    public virtual DbSet<ACC_REPORT1> ACC_REPORT1s { get; set; }

    public virtual DbSet<ACC_REPORT3> ACC_REPORT3s { get; set; }

    public virtual DbSet<BaoGia_Confirm_Name_Quotation> BaoGia_Confirm_Name_Quotations { get; set; }

    public virtual DbSet<BaoGia_Detail_of_Quotation> BaoGia_Detail_of_Quotations { get; set; }

    public virtual DbSet<BaoGia_History_Approver_of_Quotation> BaoGia_History_Approver_of_Quotations { get; set; }

    public virtual DbSet<BaoGia_History_Detail_Request> BaoGia_History_Detail_Requests { get; set; }

    public virtual DbSet<BaoGia_History_Request_of_Quotation> BaoGia_History_Request_of_Quotations { get; set; }

    public virtual DbSet<BaoGia_Master_Approver_Send_Mail> BaoGia_Master_Approver_Send_Mails { get; set; }

    public virtual DbSet<BaoGia_NCC> BaoGia_NCCs { get; set; }

    public virtual DbSet<BaoGia_NCC_Category> BaoGia_NCC_Categories { get; set; }

    public virtual DbSet<BaoGia_Request_of_Quotation> BaoGia_Request_of_Quotations { get; set; }

    public virtual DbSet<BaoGia_Status> BaoGia_Statuses { get; set; }

    public virtual DbSet<BaoGia_Step> BaoGia_Steps { get; set; }

    public virtual DbSet<Baocao_ACC_KIEMKE> Baocao_ACC_KIEMKEs { get; set; }

    public virtual DbSet<Baocao_ACC_NHAP_THONGTIN> Baocao_ACC_NHAP_THONGTINs { get; set; }

    public virtual DbSet<CATAGORY_MATERIAL_MASTER> CATAGORY_MATERIAL_MASTERs { get; set; }

    public virtual DbSet<CHECK> CHECKs { get; set; }

    public virtual DbSet<CHIHONG> CHIHONGs { get; set; }

    public virtual DbSet<CHUNGTU> CHUNGTUs { get; set; }

    public virtual DbSet<CHUNGTU_DANHAN> CHUNGTU_DANHANs { get; set; }

    public virtual DbSet<CHUYENTIEN> CHUYENTIENs { get; set; }

    public virtual DbSet<CHUYENTIEN_CHITIET> CHUYENTIEN_CHITIETs { get; set; }

    public virtual DbSet<COMBOBOX> COMBOBOXes { get; set; }

    public virtual DbSet<COSTCENTER_MONTH_TOTAL_ESTIMATE> COSTCENTER_MONTH_TOTAL_ESTIMATEs { get; set; }

    public virtual DbSet<DAY_OFF> DAY_OFFs { get; set; }

    public virtual DbSet<DEPARTMENT> DEPARTMENTs { get; set; }

    public virtual DbSet<DEPARTMENT_1> DEPARTMENT_1s { get; set; }

    public virtual DbSet<DEPARTMENT_VITRI> DEPARTMENT_VITRIs { get; set; }

    public virtual DbSet<DONVIQUYDOI> DONVIQUYDOIs { get; set; }

    public virtual DbSet<EMAIL> EMAILs { get; set; }

    public virtual DbSet<ESTIMATE> ESTIMATEs { get; set; }

    public virtual DbSet<ESTIMATE_CHANGE> ESTIMATE_CHANGEs { get; set; }

    public virtual DbSet<ESTIMATE_DEADLINE_CHANGE> ESTIMATE_DEADLINE_CHANGEs { get; set; }

    public virtual DbSet<ESTIMATE_DEADLINE_CHANGE_DEFAULT> ESTIMATE_DEADLINE_CHANGE_DEFAULTs { get; set; }

    public virtual DbSet<EXCHANGE_RATE> EXCHANGE_RATEs { get; set; }

    public virtual DbSet<EXPORT_TEMPLATE> EXPORT_TEMPLATEs { get; set; }

    public virtual DbSet<GET_CODE_REQUEST_BY_MATERIAL_CODE> GET_CODE_REQUEST_BY_MATERIAL_CODEs { get; set; }

    public virtual DbSet<GROUP> GROUPs { get; set; }

    public virtual DbSet<GROUP_MEMBER> GROUP_MEMBERs { get; set; }

    public virtual DbSet<IM_DONVI> IM_DONVIs { get; set; }

    public virtual DbSet<IM_LOAITHANHTOAN> IM_LOAITHANHTOANs { get; set; }

    public virtual DbSet<IM_LOG> IM_LOGs { get; set; }

    public virtual DbSet<IM_NCC> IM_NCCs { get; set; }

    public virtual DbSet<IM_NCC_NEW> IM_NCC_NEWs { get; set; }

    public virtual DbSet<IM_PHUONGTHUCVANCHUYEN> IM_PHUONGTHUCVANCHUYENs { get; set; }

    public virtual DbSet<IM_PO> IM_POs { get; set; }

    public virtual DbSet<IM_PO_AUTO> IM_PO_AUTOs { get; set; }

    public virtual DbSet<IM_PO_DETAIL> IM_PO_DETAILs { get; set; }

    public virtual DbSet<IM_PO_DETAIL_AUTO> IM_PO_DETAIL_AUTOs { get; set; }

    public virtual DbSet<IM_PO_LYDONEEDNONEED> IM_PO_LYDONEEDNONEEDs { get; set; }

    public virtual DbSet<IM_PO_TRANGTHAI> IM_PO_TRANGTHAIs { get; set; }

    public virtual DbSet<KHO> KHOs { get; set; }

    public virtual DbSet<KHO_CHITIET> KHO_CHITIETs { get; set; }

    public virtual DbSet<KHO_DONVIQUYDOI> KHO_DONVIQUYDOIs { get; set; }

    public virtual DbSet<KHO_KIEMKE> KHO_KIEMKEs { get; set; }

    public virtual DbSet<KHO_NHAPXUAT> KHO_NHAPXUATs { get; set; }

    public virtual DbSet<KHO_XOA> KHO_XOAs { get; set; }

    public virtual DbSet<LOG> LOGs { get; set; }

    public virtual DbSet<LOG_REQUEST> LOG_REQUESTs { get; set; }

    public virtual DbSet<MAILED> MAILEDs { get; set; }

    public virtual DbSet<MATEIAL_REUSE> MATEIAL_REUSEs { get; set; }

    public virtual DbSet<MATERIAL> MATERIALs { get; set; }

    public virtual DbSet<MATERIAL_ACCOUNT> MATERIAL_ACCOUNTs { get; set; }

    public virtual DbSet<MATERIAL_ACOUNTCODE> MATERIAL_ACOUNTCODEs { get; set; }

    public virtual DbSet<MATERIAL_IT> MATERIAL_ITs { get; set; }

    public virtual DbSet<MATERIAL_MATONG> MATERIAL_MATONGs { get; set; }

    public virtual DbSet<Master_RECEIVE_EMAIL_PRICE> Master_RECEIVE_EMAIL_PRICEs { get; set; }

    public virtual DbSet<NHAP> NHAPs { get; set; }

    public virtual DbSet<OD> ODs { get; set; }

    public virtual DbSet<ORDER> ORDERs { get; set; }

    public virtual DbSet<OUT_INPUT> OUT_INPUTs { get; set; }

    public virtual DbSet<OUT_INPUT_ACCOUNT> OUT_INPUT_ACCOUNTs { get; set; }

    public virtual DbSet<PARAMETTER> PARAMETTERs { get; set; }

    public virtual DbSet<PO> POs { get; set; }

    public virtual DbSet<PO_Result_ThueNhaThau> PO_Result_ThueNhaThaus { get; set; }

    public virtual DbSet<REMAINDER> REMAINDERs { get; set; }

    public virtual DbSet<REQUEST> REQUESTs { get; set; }

    public virtual DbSet<REQUEST_ACCEPT> REQUEST_ACCEPTs { get; set; }

    public virtual DbSet<REQUEST_DETAIL> REQUEST_DETAILs { get; set; }

    public virtual DbSet<REQUEST_DETAIL_QUOATATION> REQUEST_DETAIL_QUOATATIONs { get; set; }

    public virtual DbSet<REQUEST_DETAIL_VENDOR> REQUEST_DETAIL_VENDORs { get; set; }

    public virtual DbSet<RETURN_GOOD> RETURN_GOODs { get; set; }

    public virtual DbSet<ROW> ROWs { get; set; }

    public virtual DbSet<RQ_PO_Detail> RQ_PO_Details { get; set; }

    public virtual DbSet<SPLIT> SPLITs { get; set; }

    public virtual DbSet<TEM> TEMs { get; set; }

    public virtual DbSet<TEM_LUONGSUDUNG> TEM_LUONGSUDUNGs { get; set; }

    public virtual DbSet<TEST> TESTs { get; set; }

    public virtual DbSet<TEn> TEns { get; set; }

    public virtual DbSet<TM_ACCOUNT> TM_ACCOUNTs { get; set; }

    public virtual DbSet<TM_AUTHORITY_MENU> TM_AUTHORITY_MENUs { get; set; }

    public virtual DbSet<TM_AUTHORITY_THEOCHUCNANG> TM_AUTHORITY_THEOCHUCNANGs { get; set; }

    public virtual DbSet<TM_Category> TM_Categories { get; set; }

    public virtual DbSet<TM_GOOD_TYPE> TM_GOOD_TYPEs { get; set; }

    public virtual DbSet<TM_KHO_MOLD> TM_KHO_MOLDs { get; set; }

    public virtual DbSet<TM_LOAIHINHTOKHIum> TM_LOAIHINHTOKHIAs { get; set; }

    public virtual DbSet<TM_MAIL_ACC> TM_MAIL_ACCs { get; set; }

    public virtual DbSet<TM_MASTER_MAIL> TM_MASTER_MAILs { get; set; }

    public virtual DbSet<TM_MENU> TM_MENUs { get; set; }

    public virtual DbSet<TM_MENU_BACKUP> TM_MENU_BACKUPs { get; set; }

    public virtual DbSet<TM_NHAP_XUAT_KHO_MOLD_LOG> TM_NHAP_XUAT_KHO_MOLD_LOGs { get; set; }

    public virtual DbSet<TM_NOTICE> TM_NOTICEs { get; set; }

    public virtual DbSet<TM_PO_CONFIRMED_GOOD_COME> TM_PO_CONFIRMED_GOOD_COMEs { get; set; }

    public virtual DbSet<TM_PO_NHAPKHO_MOLD_STATUS> TM_PO_NHAPKHO_MOLD_STATUSes { get; set; }

    public virtual DbSet<TM_PURPOSE_USING> TM_PURPOSE_USINGs { get; set; }

    public virtual DbSet<TM_QR_CODE> TM_QR_CODEs { get; set; }

    public virtual DbSet<TM_REPORT> TM_REPORTs { get; set; }

    public virtual DbSet<TM_REPORT_HISTORY> TM_REPORT_HISTORies { get; set; }

    public virtual DbSet<TM_TEMP_FORM_ORDER> TM_TEMP_FORM_ORDERs { get; set; }

    public virtual DbSet<TM_TRADE_CUSTOM_TYPE> TM_TRADE_CUSTOM_TYPEs { get; set; }

    public virtual DbSet<TM_USER> TM_USERs { get; set; }

    public virtual DbSet<TM_USER_GROUP_USING> TM_USER_GROUP_USINGs { get; set; }

    public virtual DbSet<TM_USER_TEST> TM_USER_TESTs { get; set; }

    public virtual DbSet<TONTRENLINE> TONTRENLINEs { get; set; }

    public virtual DbSet<USER> USERs { get; set; }

    public virtual DbSet<USER_DEPT> USER_DEPTs { get; set; }

    public virtual DbSet<V2_FORM> V2_FORMs { get; set; }

    public virtual DbSet<V2_FORM_ALL> V2_FORM_ALLs { get; set; }

    public virtual DbSet<V2_FORM_CHITIET> V2_FORM_CHITIETs { get; set; }

    public virtual DbSet<V3_CATAGORY> V3_CATAGORies { get; set; }

    public virtual DbSet<V3_CATAGORY_MAPPING> V3_CATAGORY_MAPPINGs { get; set; }

    public virtual DbSet<V3_CATAGORY_NEW> V3_CATAGORY_NEWs { get; set; }

    public virtual DbSet<V3_EMAIL> V3_EMAILs { get; set; }

    public virtual DbSet<V3_EMAILCONTENT> V3_EMAILCONTENTs { get; set; }

    public virtual DbSet<V3_NOT_BELONG_CATAGORY> V3_NOT_BELONG_CATAGORies { get; set; }

    public virtual DbSet<V3_POCONFIRM> V3_POCONFIRMs { get; set; }

    public virtual DbSet<VERSION> VERSIONs { get; set; }

    public virtual DbSet<VIEWPO_NCC> VIEWPO_NCCs { get; set; }

    public virtual DbSet<VIEW_DEPARTMENT_VITRI> VIEW_DEPARTMENT_VITRIs { get; set; }

    public virtual DbSet<VIEW_DEPT_ESTIMATE_DEADLINE> VIEW_DEPT_ESTIMATE_DEADLINEs { get; set; }

    public virtual DbSet<VIEW_DEPT_REQUEST> VIEW_DEPT_REQUESTs { get; set; }

    public virtual DbSet<VIEW_HISTORY_PO_DETAIL> VIEW_HISTORY_PO_DETAILs { get; set; }

    public virtual DbSet<VIEW_HISTORY_PO_DETAIL_AUTO> VIEW_HISTORY_PO_DETAIL_AUTOs { get; set; }

    public virtual DbSet<VIEW_MATERIAL_REUSE> VIEW_MATERIAL_REUSEs { get; set; }

    public virtual DbSet<VIEW_NHOMHANG> VIEW_NHOMHANGs { get; set; }

    public virtual DbSet<VIEW_PROCESS_PRIVATE> VIEW_PROCESS_PRIVATEs { get; set; }

    public virtual DbSet<VIEW_UPDATETOKHAI> VIEW_UPDATETOKHAIs { get; set; }

    public virtual DbSet<VIEW_USER_DEPT> VIEW_USER_DEPTs { get; set; }

    public virtual DbSet<View_XXXXX> View_XXXXXes { get; set; }

    public virtual DbSet<WF_BAOGIum> WF_BAOGIAs { get; set; }

    public virtual DbSet<WF_CREATEDID> WF_CREATEDIDs { get; set; }

    public virtual DbSet<WF_HISTORY> WF_HISTORies { get; set; }

    public virtual DbSet<WF_PROCESS> WF_PROCESSes { get; set; }

    public virtual DbSet<WF_PROCESS_STEP> WF_PROCESS_STEPs { get; set; }

    public virtual DbSet<WF_WORKFOLLOWLIST> WF_WORKFOLLOWLISTs { get; set; }

    public virtual DbSet<WF_WORKFOLLOWSTEP> WF_WORKFOLLOWSTEPs { get; set; }

    public virtual DbSet<XUAT> XUATs { get; set; }

    public virtual DbSet<XUAT_ACC> XUAT_ACCs { get; set; }

    public virtual DbSet<XUAT_GA> XUAT_GAs { get; set; }

    public virtual DbSet<XUAT_GA_TONG> XUAT_GA_TONGs { get; set; }

    public virtual DbSet<XULYDONHANG> XULYDONHANGs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //=> optionsBuilder.UseSqlServer("Server=apbivnap19;Database=COST_MANAGERMENT;User Id=COST_MANAGEMENT;Password=COST_MANAGEMENT;TrustServerCertificate=true;");
        => optionsBuilder.UseSqlServer("Server=APBIVNDB14;Database=COST_MANAGEMENT;User Id=COST_MANAGEMENT;Password=COST_MANAGEMENT;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ACCOUNT_NAME>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCOUNT_NAME");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Account_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Mucdich).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Name_Jp).HasMaxLength(500);
            entity.Property(e => e.Phongbanchiuchiphi).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT");

            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Phong).HasMaxLength(50);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT_COMMON>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT_COMMON");

            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaTong).HasMaxLength(7);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Ngaynhaokho)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.Phong).HasMaxLength(50);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT_PROPOTIONAL>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT_PROPOTIONAL");

            entity.Property(e => e.MaNguyenLieu).HasMaxLength(7);
            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Ngaynhaokho)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT_SUMTONTRENLINE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT_SUMTONTRENLINE");

            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.TenMatongVn).HasMaxLength(500);
            entity.Property(e => e.Tennhomhang).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT_TONTRENLINE_E>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT_TONTRENLINE_E");

            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Mt).HasMaxLength(7);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT_TONTRENLINE_VITRI>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT_TONTRENLINE_VITRI");

            entity.Property(e => e.Cost).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.NgayCapnhat).HasColumnType("datetime");
            entity.Property(e => e.Nhamay).HasMaxLength(50);
            entity.Property(e => e.UserCapnhat).HasMaxLength(50);
            entity.Property(e => e.Vitri).HasMaxLength(50);
        });

        modelBuilder.Entity<ACCREPORT_TONTRENLINE_XUATKHO>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ACCREPORT_TONTRENLINE_XUATKHO");

            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(7);
            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Thang)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<ACC_NHOMHANG>(entity =>
        {
            entity.HasKey(e => e.Manhomhang);

            entity.ToTable("ACC_NHOMHANG");

            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Id_Nhomhang).ValueGeneratedOnAdd();
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
        });

        modelBuilder.Entity<ACC_NHOMHANG_MATERIAL>(entity =>
        {
            entity.HasKey(e => new { e.Manhomhang, e.Material_Code });

            entity.ToTable("ACC_NHOMHANG_MATERIAL");

            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Id_Nhomhang).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ACC_NHOMVITRI>(entity =>
        {
            entity.HasKey(e => e.Mahangmuctheovitri);

            entity.ToTable("ACC_NHOMVITRI");

            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.Id_Nhomvitri).ValueGeneratedOnAdd();
            entity.Property(e => e.LoaiVitri).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.Tenhangmuctheovitri).HasMaxLength(50);
        });

        modelBuilder.Entity<ACC_NHOMVITRI_DEPARTMENT_VITRI>(entity =>
        {
            entity.HasKey(e => new { e.Mahangmuctheovitri, e.MaChuyen, e.Cost }).HasName("PK_ACC_NHOMVITRI_DEPARTMENT_VITRI_1");

            entity.ToTable("ACC_NHOMVITRI_DEPARTMENT_VITRI");

            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.MaChuyen).HasMaxLength(50);
            entity.Property(e => e.Cost).HasMaxLength(50);
            entity.Property(e => e.Id_Nhom).ValueGeneratedOnAdd();

            entity.HasOne(d => d.MahangmuctheovitriNavigation).WithMany(p => p.ACC_NHOMVITRI_DEPARTMENT_VITRIs)
                .HasForeignKey(d => d.Mahangmuctheovitri)
                .HasConstraintName("FK_ACC_NHOMVITRI_DEPARTMENT_VITRI_ACC_NHOMVITRI");
        });

        modelBuilder.Entity<ACC_REPORT1>(entity =>
        {
            entity.HasKey(e => new { e.Cost_Center, e.Loaihinhtokhai, e.Thang });

            entity.ToTable("ACC_REPORT1");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Loaihinhtokhai).HasMaxLength(50);
            entity.Property(e => e.Id_Report1).ValueGeneratedOnAdd();
            entity.Property(e => e.Ngaycapnhat).HasColumnType("datetime");
            entity.Property(e => e.Usercapnhat).HasMaxLength(50);
        });

        modelBuilder.Entity<ACC_REPORT3>(entity =>
        {
            entity.HasKey(e => new { e.Cost_Center, e.Account_Code, e.Thang });

            entity.ToTable("ACC_REPORT3");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Id_Report3).ValueGeneratedOnAdd();
            entity.Property(e => e.Ngaycapnhat).HasColumnType("datetime");
            entity.Property(e => e.Usercapnhat).HasMaxLength(50);
        });

        modelBuilder.Entity<BaoGia_Confirm_Name_Quotation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_C__3214EC27576140BF");

            entity.ToTable("BaoGia_Confirm_Name_Quotation");

            entity.Property(e => e.CHR_Status)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("Draft");
            entity.Property(e => e.CHR_StatusACC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_StatusShip)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DTM_CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DTM_Send).HasColumnType("datetime");
            entity.Property(e => e.DTM_UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.DTM_UserAcc).HasColumnType("datetime");
            entity.Property(e => e.DTM_UserPUR).HasColumnType("datetime");
            entity.Property(e => e.DTM_UserShip).HasColumnType("datetime");
            entity.Property(e => e.NVCHR_LyDo).HasMaxLength(300);
            entity.Property(e => e.NVCHR_Note).HasMaxLength(500);
            entity.Property(e => e.VCHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_MaHangNoiBo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_TenHaiQuan).HasMaxLength(500);
            entity.Property(e => e.VCHR_TenRecomment).HasMaxLength(1200);
            entity.Property(e => e.VCHR_UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_UserAcc)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_UserPUR)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_UserShip)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BaoGia_Detail_of_Quotation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_D__3214EC2759F0B3DF");

            entity.ToTable("BaoGia_Detail_of_Quotation");

            entity.Property(e => e.BIT_Commit).HasDefaultValue(false);
            entity.Property(e => e.BIT_Select).HasDefaultValue(false);
            entity.Property(e => e.CHR_CodeNCC)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaHangNCC)
                .HasMaxLength(250);
            entity.Property(e => e.CHR_NameEN).HasMaxLength(200);
            entity.Property(e => e.CHR_Status).HasMaxLength(150);
            entity.Property(e => e.CHR_UpdateBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DTM_CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DTM_EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.DTM_EndDate).HasColumnType("datetime");
            entity.Property(e => e.DTM_ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.DTM_LeadTime)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DTM_ShipTime).HasColumnType("datetime");
            entity.Property(e => e.DTM_UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.FL_TaxRate).HasDefaultValue(0.0);
            entity.Property(e => e.INT_NumberEdit).HasDefaultValue(0);
            entity.Property(e => e.NVCHR_DeliveryTerm).HasMaxLength(500);
            entity.Property(e => e.NVCHR_DonVi).HasMaxLength(150);
            entity.Property(e => e.NVCHR_File).HasMaxLength(500);
            entity.Property(e => e.NVCHR_MOQ).HasMaxLength(350);
            entity.Property(e => e.NVCHR_NameNCC).HasMaxLength(250);
            entity.Property(e => e.NVCHR_NhaSanXuat).HasMaxLength(250);
            entity.Property(e => e.NVCHR_Note).HasMaxLength(550);
            entity.Property(e => e.NVCHR_Packing).HasMaxLength(350);
            entity.Property(e => e.NVCHR_PaymentTerm).HasMaxLength(500);
            entity.Property(e => e.NVCHR_ReasonPick).HasMaxLength(500);
            entity.Property(e => e.NVCHR_TenHangHQ).HasMaxLength(1200);
            entity.Property(e => e.NVCHR_Warranty).HasMaxLength(500);
            entity.Property(e => e.VCHR_AnToan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_COCQ)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_CamKet).HasMaxLength(50);
            entity.Property(e => e.VCHR_MSDS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VCHR_Rohs)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BaoGia_History_Approver_of_Quotation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_H__3214EC27630C8564");

            entity.ToTable("BaoGia_History_Approver_of_Quotation");

            entity.Property(e => e.CHR_SectionCodeApprover)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_SectionCodeSend)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_SectionNameApprover).HasMaxLength(150);
            entity.Property(e => e.CHR_SectionNameSend).HasMaxLength(150);
            entity.Property(e => e.CHR_StatusFlag)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_UserApprover)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_UserSendApprover)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DTM_UserApprover).HasColumnType("datetime");
            entity.Property(e => e.DTM_UserSendApprover)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NVCHR_ReturnReason).HasMaxLength(500);
        });

        modelBuilder.Entity<BaoGia_History_Detail_Request>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_H__3214EC279284EF0C");

            entity.ToTable("BaoGia_History_Detail_Request");

            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DTM_CreateBy).HasColumnType("datetime");
        });

        modelBuilder.Entity<BaoGia_History_Request_of_Quotation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_H__3214EC27A4AA9F7D");

            entity.ToTable("BaoGia_History_Request_of_Quotation");

            entity.Property(e => e.CHR_ActionType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Updatedate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NVCHR_LyDo).HasMaxLength(500);
            entity.Property(e => e.NVCHR_UpdateName).HasMaxLength(250);
        });

        modelBuilder.Entity<BaoGia_Master_Approver_Send_Mail>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_M__3214EC276C341E68");

            entity.ToTable("BaoGia_Master_Approver_Send_Mail");

            entity.Property(e => e.CHR_CodeSection)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CHR_NameSection)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.CHR_UserAdid)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NVCHR_Position)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NVCHR_StepName).HasMaxLength(250);
            entity.Property(e => e.NVCHR_UserName).HasMaxLength(250);
        });

        modelBuilder.Entity<BaoGia_NCC>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_N__3214EC2755C33A82");

            entity.ToTable("BaoGia_NCC");

            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaNCC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_UpdateBY)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DTM_CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DTM_UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.NVCHAR_TenNCC).HasMaxLength(300);
            entity.Property(e => e.NVCHR_CodeByNCC).HasMaxLength(300);
            entity.Property(e => e.NVCHR_MakeIn).HasMaxLength(255);
        });

        modelBuilder.Entity<BaoGia_NCC_Category>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_N__3214EC273F00446A");

            entity.ToTable("BaoGia_NCC_Category");

            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaNCC)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Mail).HasMaxLength(200);
            entity.Property(e => e.CHR_PIC).HasMaxLength(200);
            entity.Property(e => e.CHR_Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.DTM_CreateBy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NVCHR_ChungLoai).HasMaxLength(300);
            entity.Property(e => e.NVCHR_SanXuat).HasMaxLength(250);
            entity.Property(e => e.NVCHR_TenNCC).HasMaxLength(255);
        });

        modelBuilder.Entity<BaoGia_Request_of_Quotation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_R__3214EC273E3D596D");

            entity.ToTable("BaoGia_Request_of_Quotation");

            entity.Property(e => e.BIT_LayBaoGia).HasDefaultValue(true);
            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Gap)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaHangNCC).HasMaxLength(350);
            entity.Property(e => e.CHR_MaHangNoiBo)
                .HasMaxLength(350)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MaNCC).HasMaxLength(250);
            entity.Property(e => e.CHR_MaThietBi)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.CHR_NameEN).HasMaxLength(1200);
            entity.Property(e => e.CHR_Phanloai).HasMaxLength(250);
            entity.Property(e => e.CHR_SectionCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_SectionName).HasMaxLength(100);
            entity.Property(e => e.CHR_UserApproval).HasMaxLength(20);
            entity.Property(e => e.DTM_CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DTM_Deadline).HasColumnType("datetime");
            entity.Property(e => e.DTM_KyHan).HasColumnType("datetime");
            entity.Property(e => e.DTM_NgayMuonNhan).HasColumnType("datetime");
            entity.Property(e => e.DTM_UpdateLater).HasColumnType("datetime");
            entity.Property(e => e.ID_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.INT_SoLanUpdate).HasDefaultValue(0);
            entity.Property(e => e.NVCHR_AnToan).HasMaxLength(450);
            entity.Property(e => e.NVCHR_COCQ).HasMaxLength(450);
            entity.Property(e => e.NVCHR_ChatLieu).HasMaxLength(500);
            entity.Property(e => e.NVCHR_ChungLoai).HasMaxLength(350);
            entity.Property(e => e.NVCHR_DonVi).HasMaxLength(250);
            entity.Property(e => e.NVCHR_DongMay).HasMaxLength(550);
            entity.Property(e => e.NVCHR_FileThietKe).HasMaxLength(350);
            entity.Property(e => e.NVCHR_HinhDang).HasMaxLength(550);
            entity.Property(e => e.NVCHR_KichThuoc).HasMaxLength(550);
            entity.Property(e => e.NVCHR_LyDo).HasMaxLength(550);
            entity.Property(e => e.NVCHR_MSDS).HasMaxLength(450);
            entity.Property(e => e.NVCHR_NameVN).HasMaxLength(1200);
            entity.Property(e => e.NVCHR_NhaSanXuat).HasMaxLength(450);
            entity.Property(e => e.NVCHR_Rohs).HasMaxLength(450);
            entity.Property(e => e.NVCHR_TenNCC).HasMaxLength(550);
            entity.Property(e => e.NVCHR_ThanhPhan).HasMaxLength(550);
            entity.Property(e => e.NVCHR_TinhNang).HasMaxLength(550);
            entity.Property(e => e.NVCHR_UserRequest).HasMaxLength(200);
            entity.Property(e => e.NVCHR_ReasonQuotation).HasMaxLength(250);
            entity.Property(e => e.CHR_LinkFile).HasMaxLength(1500);
        });

        modelBuilder.Entity<BaoGia_Status>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_S__3214EC271F976FD2");

            entity.ToTable("BaoGia_Status");

            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Flag)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.CHR_TenStatusEN).HasMaxLength(277);
            entity.Property(e => e.CHR_TenStatusJP).HasMaxLength(277);
            entity.Property(e => e.DTM_CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NVCHR_TenStatus).HasMaxLength(250);
            entity.Property(e => e.VCHR_CodeStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BaoGia_Step>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__BaoGia_S__3214EC2746181382");

            entity.ToTable("BaoGia_Step");

            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Note)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_StepName).HasMaxLength(250);
            entity.Property(e => e.CHR_StepNameEN).HasMaxLength(277);
            entity.Property(e => e.CHR_StepNameJP).HasMaxLength(277);
            entity.Property(e => e.DTM_CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Baocao_ACC_KIEMKE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Baocao_ACC_KIEMKE");

            entity.Property(e => e.MaTong).HasMaxLength(7);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<Baocao_ACC_NHAP_THONGTIN>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Baocao_ACC_NHAP_THONGTIN");

            entity.Property(e => e.MaTong).HasMaxLength(7);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Tennhomhang).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<CATAGORY_MATERIAL_MASTER>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CATAGORY_MATERIAL_MASTER");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Account_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Catagory1).HasMaxLength(500);
            entity.Property(e => e.Catagory2).HasMaxLength(500);
            entity.Property(e => e.Catagory3).HasMaxLength(500);
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.GoodKind).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Material_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
        });

        modelBuilder.Entity<CHECK>(entity =>
        {
            entity.ToTable("CHECK");

            entity.Property(e => e.KindCheck).HasMaxLength(50);
        });

        modelBuilder.Entity<CHIHONG>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CHIHONG");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name).HasMaxLength(200);
            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Brand).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Currency_Real).HasMaxLength(10);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Expr1).HasMaxLength(50);
            entity.Property(e => e.Expr2).HasMaxLength(50);
            entity.Property(e => e.Expr3).HasMaxLength(50);
            entity.Property(e => e.Expr5).HasMaxLength(100);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Guarantee)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Hinhthuc).HasMaxLength(50);
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Id_LichsuXuat).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Last_Update).HasColumnType("datetime");
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.PO).HasMaxLength(20);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(10);
            entity.Property(e => e.Poisition).HasMaxLength(500);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.Sotokhai).HasMaxLength(200);
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
            entity.Property(e => e.Unit_Real).HasMaxLength(50);
            entity.Property(e => e.User_Update).HasMaxLength(50);
            entity.Property(e => e.Vitri).HasMaxLength(100);
        });

        modelBuilder.Entity<CHUNGTU>(entity =>
        {
            entity.HasKey(e => e.MaChungtu);

            entity.ToTable("CHUNGTU");

            entity.Property(e => e.MaChungtu).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Id_Chungtu).ValueGeneratedOnAdd();
            entity.Property(e => e.LOAI).HasMaxLength(50);
            entity.Property(e => e.TenEN).HasMaxLength(1000);
            entity.Property(e => e.TenVN).HasMaxLength(1000);
            entity.Property(e => e.Version).HasMaxLength(50);
        });

        modelBuilder.Entity<CHUNGTU_DANHAN>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CHUNGTU_DANHAN");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Id_Chungtudanhan).ValueGeneratedOnAdd();
            entity.Property(e => e.LOAI).HasMaxLength(50);
            entity.Property(e => e.MaChungtu).HasMaxLength(50);
            entity.Property(e => e.Nguoinhan).HasMaxLength(50);
            entity.Property(e => e.Nguoitao).HasMaxLength(50);
            entity.Property(e => e.Nguoixoa).HasMaxLength(50);
            entity.Property(e => e.Phongnop).HasMaxLength(50);
            entity.Property(e => e.Sonhan).HasMaxLength(50);
            entity.Property(e => e.TenEN).HasMaxLength(1000);
            entity.Property(e => e.TenVN).HasMaxLength(1000);
            entity.Property(e => e.Thogiantao).HasColumnType("datetime");
            entity.Property(e => e.Thoigiannhan).HasColumnType("datetime");
            entity.Property(e => e.Thoigianxoa).HasColumnType("datetime");
            entity.Property(e => e.Tinhtrang).HasMaxLength(50);
            entity.Property(e => e.Version).HasMaxLength(50);
        });

        modelBuilder.Entity<CHUYENTIEN>(entity =>
        {
            entity.HasKey(e => e.MaChuyenTien);

            entity.ToTable("CHUYENTIEN");

            entity.Property(e => e.MaChuyenTien).HasMaxLength(50);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.Id_ChuyenTien).ValueGeneratedOnAdd();
            entity.Property(e => e.LoaiChiPhiChuyen).HasMaxLength(50);
            entity.Property(e => e.Namtaichinh).HasMaxLength(10);
            entity.Property(e => e.Ngaychuyen).HasColumnType("datetime");
            entity.Property(e => e.Nguoichuyen).HasMaxLength(50);
            entity.Property(e => e.Nguoixuly).HasMaxLength(50);
            entity.Property(e => e.PhongChuyen).HasMaxLength(50);
            entity.Property(e => e.Tinhtrang).HasMaxLength(50);
        });

        modelBuilder.Entity<CHUYENTIEN_CHITIET>(entity =>
        {
            entity.HasKey(e => e.Id_Chuyen);

            entity.ToTable("CHUYENTIEN_CHITIET");

            entity.Property(e => e.LoaiChiPhiChuyen).HasMaxLength(50);
            entity.Property(e => e.LoaiChuyen).HasMaxLength(50);
            entity.Property(e => e.MaChuyenTien).HasMaxLength(50);
            entity.Property(e => e.PhongChuyen).HasMaxLength(20);
            entity.Property(e => e.SotaikhoanChuyen).HasMaxLength(50);

            entity.HasOne(d => d.MaChuyenTienNavigation).WithMany(p => p.CHUYENTIEN_CHITIETs)
                .HasForeignKey(d => d.MaChuyenTien)
                .HasConstraintName("FK_CHUYENTIEN_CHITIET_CHUYENTIEN");
        });

        modelBuilder.Entity<COMBOBOX>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("COMBOBOX");

            entity.Property(e => e.Detail).HasMaxLength(500);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Kind).HasMaxLength(50);
            entity.Property(e => e.PO).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<COSTCENTER_MONTH_TOTAL_ESTIMATE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("COSTCENTER_MONTH_TOTAL_ESTIMATE");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name).HasMaxLength(200);
            entity.Property(e => e.Brand).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Declaration).HasMaxLength(50);
            entity.Property(e => e.Good_Code).HasMaxLength(500);
            entity.Property(e => e.Group_Code).HasMaxLength(10);
            entity.Property(e => e.Guarantee)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Id_LichsuXuat).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Kind).HasMaxLength(20);
            entity.Property(e => e.Last_Update).HasColumnType("datetime");
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.PO).HasMaxLength(20);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(10);
            entity.Property(e => e.Poisition).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StatusTotal).HasMaxLength(20);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Real).HasMaxLength(50);
            entity.Property(e => e.User_Update).HasMaxLength(50);
            entity.Property(e => e.Vitri).HasMaxLength(100);
        });

        modelBuilder.Entity<DAY_OFF>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("DAY_OFF");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DEPARTMENT>(entity =>
        {
            entity.HasKey(e => e.Id_Dept).HasName("PK_DEPARTMENT_Id_Dept");

            entity.ToTable("DEPARTMENT");

            entity.Property(e => e.CHR_Section_Code)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.CHR_WAREHOUSE).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Cost_Center_Group).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Name_Jp).HasMaxLength(500);
        });

        modelBuilder.Entity<DEPARTMENT_1>(entity =>
        {
            entity.HasKey(e => e.Cost_Center).HasName("PK_DEPARTMENT");

            entity.ToTable("DEPARTMENT_1");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.CHR_Section_Code)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.CHR_WAREHOUSE).HasMaxLength(50);
            entity.Property(e => e.Cost_Center_Group).HasMaxLength(500);
            entity.Property(e => e.Id_Dept).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Name_Jp).HasMaxLength(500);
        });

        modelBuilder.Entity<DEPARTMENT_VITRI>(entity =>
        {
            entity.HasKey(e => new { e.MaCost, e.MaChuyen });

            entity.ToTable("DEPARTMENT_VITRI");

            entity.Property(e => e.MaCost).HasMaxLength(50);
            entity.Property(e => e.MaChuyen).HasMaxLength(50);
            entity.Property(e => e.Id_Vitri).ValueGeneratedOnAdd();
            entity.Property(e => e.MaMay).HasMaxLength(50);
            entity.Property(e => e.MaPhong).HasMaxLength(50);
            entity.Property(e => e.TenChuyen).HasMaxLength(50);
        });

        modelBuilder.Entity<DONVIQUYDOI>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("DONVIQUYDOI");

            entity.Property(e => e.DonviPO).HasMaxLength(50);
            entity.Property(e => e.DonviRequest).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<EMAIL>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Email");

            entity.ToTable("EMAIL");

            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);

            entity.HasOne(d => d.CHR_USER).WithMany(p => p.EMAILs)
                .HasForeignKey(d => d.CHR_USERID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EMAIL_TM_USER");
        });

        modelBuilder.Entity<ESTIMATE>(entity =>
        {
            entity.HasKey(e => new { e.Cost_Center, e.Month, e.Kind });

            entity.ToTable("ESTIMATE");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Kind).HasMaxLength(50);
            entity.Property(e => e.Id_Est).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Cost_CenterNavigation).WithMany(p => p.ESTIMATEs)
                .HasForeignKey(d => d.Cost_Center)
                .HasConstraintName("FK_ESTIMATE_DEPARTMENT1");
        });

        modelBuilder.Entity<ESTIMATE_CHANGE>(entity =>
        {
            entity.HasKey(e => e.Id_Change);

            entity.ToTable("ESTIMATE_CHANGE");

            entity.HasIndex(e => new { e.Cost_Center, e.Kind, e.NamThang, e.Month }, "IX_ESTIMATE_CHANGE_Cost_Center_Kind_NamThang_Month");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.DateCreation).HasColumnType("datetime");
            entity.Property(e => e.Kind).HasMaxLength(50);
            entity.Property(e => e.NamThang).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.UserChange).HasMaxLength(50);
        });

        modelBuilder.Entity<ESTIMATE_DEADLINE_CHANGE>(entity =>
        {
            entity.HasKey(e => new { e.Cost_Center, e.Date });

            entity.ToTable("ESTIMATE_DEADLINE_CHANGE");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.Id_Change).ValueGeneratedOnAdd();
            entity.Property(e => e.Time).HasMaxLength(50);
            entity.Property(e => e.TimeEnd).HasColumnType("datetime");
            entity.Property(e => e.TimeStart).HasColumnType("datetime");

            entity.HasOne(d => d.Cost_CenterNavigation).WithMany(p => p.ESTIMATE_DEADLINE_CHANGEs)
                .HasForeignKey(d => d.Cost_Center)
                .HasConstraintName("FK_ESTIMATE_DEADLINE_CHANGE_DEPARTMENT1");
        });

        modelBuilder.Entity<ESTIMATE_DEADLINE_CHANGE_DEFAULT>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ESTIMATE_DEADLINE_CHANGE_DEFAULT");

            entity.Property(e => e.Gio).HasMaxLength(50);
            entity.Property(e => e.Ngay).HasMaxLength(50);
            entity.Property(e => e.Thogianhieuchinh).HasMaxLength(50);
        });

        modelBuilder.Entity<EXCHANGE_RATE>(entity =>
        {
            entity.HasKey(e => new { e.DateApply, e.Currency }).HasName("PK_Exchange_Rate");

            entity.ToTable("EXCHANGE_RATE");

            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Rate).HasMaxLength(50);
        });

        modelBuilder.Entity<EXPORT_TEMPLATE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("EXPORT_TEMPLATE");

            entity.Property(e => e.Catagory1).HasMaxLength(500);
            entity.Property(e => e.Catagory2).HasMaxLength(500);
            entity.Property(e => e.Catagory3).HasMaxLength(500);
            entity.Property(e => e.Expr1).HasMaxLength(551);
        });

        modelBuilder.Entity<GET_CODE_REQUEST_BY_MATERIAL_CODE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GET_CODE_REQUEST_BY_MATERIAL_CODE");

            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<GROUP>(entity =>
        {
            entity.HasKey(e => e.Group_Code).HasName("PK_GROUP_1");

            entity.ToTable("GROUP");

            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Group_Name).HasMaxLength(50);
            entity.Property(e => e.Id_Group).ValueGeneratedOnAdd();
            entity.Property(e => e.Note).HasMaxLength(50);
        });

        modelBuilder.Entity<GROUP_MEMBER>(entity =>
        {
            entity.HasKey(e => new { e.Group_Code, e.CHR_USERID }).HasName("PK_GROUP_MEMBER_1");

            entity.ToTable("GROUP_MEMBER");

            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.Group_member_Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.CHR_USER).WithMany(p => p.GROUP_MEMBERs)
                .HasForeignKey(d => d.CHR_USERID)
                .HasConstraintName("FK_GROUP_MEMBER_TM_USER");

            entity.HasOne(d => d.Group_CodeNavigation).WithMany(p => p.GROUP_MEMBERs)
                .HasForeignKey(d => d.Group_Code)
                .HasConstraintName("FK_GROUP_MEMBER_GROUP");
        });

        modelBuilder.Entity<IM_DONVI>(entity =>
        {
            entity.HasKey(e => e.Donvi);

            entity.ToTable("IM_DONVI");

            entity.Property(e => e.Donvi).HasMaxLength(50);
            entity.Property(e => e.Donvi_Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<IM_LOAITHANHTOAN>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("IM_LOAITHANHTOAN");

            entity.Property(e => e.Loaithanhtoan).HasMaxLength(50);
            entity.Property(e => e.Loaithanhtoan_id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<IM_LOG>(entity =>
        {
            entity.HasKey(e => e.Log_Id);

            entity.ToTable("IM_LOG");

            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.Nguoicapnhat).HasMaxLength(50);
            entity.Property(e => e.PO_Detail_Id).HasMaxLength(50);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Thogian).HasColumnType("datetime");
        });

        modelBuilder.Entity<IM_NCC>(entity =>
        {
            entity.HasKey(e => new { e.Ma, e.Group_Code });

            entity.ToTable("IM_NCC");

            entity.Property(e => e.Ma).HasMaxLength(100);
            entity.Property(e => e.Group_Code).HasMaxLength(100);
            entity.Property(e => e.Dieukienthanhtoan).HasMaxLength(50);
            entity.Property(e => e.Hinhthucmotk).HasMaxLength(50);
            entity.Property(e => e.Masothue).HasMaxLength(50);
            entity.Property(e => e.Ncc_Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nhanvienketoan).HasMaxLength(50);
            entity.Property(e => e.Nhanvienkinhdoand).HasMaxLength(50);
        });

        modelBuilder.Entity<IM_NCC_NEW>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_IM_NCC_NEW_1");

            entity.ToTable("IM_NCC_NEW");

            entity.Property(e => e.Ma).HasMaxLength(100);
            entity.Property(e => e.Canphaixacnhanlamthutuchaiquan).HasMaxLength(50);
            entity.Property(e => e.Dieukienthanhtoan).HasMaxLength(50);
            entity.Property(e => e.Hinhthucmotk).HasMaxLength(50);
            entity.Property(e => e.Masothue).HasMaxLength(50);
            entity.Property(e => e.Ncc_Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nhanvienketoan).HasMaxLength(50);
            entity.Property(e => e.Nhanvienkinhdoand).HasMaxLength(50);
            entity.Property(e => e.ShortName).HasMaxLength(250);
            entity.Property(e => e.nguoi_cap_nhat).HasMaxLength(50);
            entity.Property(e => e.nhom).HasMaxLength(50);
        });

        modelBuilder.Entity<IM_PHUONGTHUCVANCHUYEN>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("IM_PHUONGTHUCVANCHUYEN");

            entity.Property(e => e.Phuongthucvanchuyen).HasMaxLength(50);
            entity.Property(e => e.Phuongthucvanchuyen_Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<IM_PO>(entity =>
        {
            entity.HasKey(e => e.SoPO);

            entity.ToTable("IM_PO");

            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Hinhthuc).HasMaxLength(50);
            entity.Property(e => e.Id_PO).ValueGeneratedOnAdd();
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
        });

        modelBuilder.Entity<IM_PO_AUTO>(entity =>
        {
            entity.HasKey(e => e.SoPO);

            entity.ToTable("IM_PO_AUTO");

            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Hinhthuc).HasMaxLength(50);
            entity.Property(e => e.Id_PO).ValueGeneratedOnAdd();
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
        });

        modelBuilder.Entity<IM_PO_DETAIL>(entity =>
        {
            entity.HasKey(e => e.PO_Detail_Id);

            entity.ToTable("IM_PO_DETAIL");

            entity.HasIndex(e => e.SoPO, "IX_IM_PO_DETAIL_SoPO");

            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CHR_SoPO_LASTEST).HasMaxLength(50);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.DTM_UPDATE_TSCD).HasColumnType("datetime");
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Dongia_SoPO_LASTEST).HasColumnType("money");
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.So_TaiSanCoDinh)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
            entity.Property(e => e.chr_USER_UPDATE_TSCD).HasMaxLength(50);

            entity.HasOne(d => d.SoPONavigation).WithMany(p => p.IM_PO_DETAILs)
                .HasForeignKey(d => d.SoPO)
                .HasConstraintName("FK_IM_PO_DETAIL_IM_PO");
        });

        modelBuilder.Entity<IM_PO_DETAIL_AUTO>(entity =>
        {
            entity.HasKey(e => e.PO_Detail_Id);

            entity.ToTable("IM_PO_DETAIL_AUTO");

            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
        });

        modelBuilder.Entity<IM_PO_LYDONEEDNONEED>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("IM_PO_LYDONEEDNONEED");

            entity.Property(e => e.Lydo).HasColumnType("ntext");
            entity.Property(e => e.NguoiXacNhan).HasMaxLength(50);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.ThoiGianXacNhan).HasColumnType("datetime");
            entity.Property(e => e.XacNhan).HasMaxLength(50);
        });

        modelBuilder.Entity<IM_PO_TRANGTHAI>(entity =>
        {
            entity.HasKey(e => e.SoPO);

            entity.ToTable("IM_PO_TRANGTHAI");

            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.NHAPINVOICE_NGAY).HasColumnType("datetime");
            entity.Property(e => e.NHAPINVOICE_USER).HasMaxLength(50);
            entity.Property(e => e.NHAPKHO_NGAY).HasColumnType("datetime");
            entity.Property(e => e.NHAPKHO_USER).HasMaxLength(50);
            entity.Property(e => e.NHAPLUONGVE_NGAY).HasColumnType("datetime");
            entity.Property(e => e.NHAPLUONGVE_USER).HasMaxLength(50);
            entity.Property(e => e.NHAPTOKHAI_NGAY).HasColumnType("datetime");
            entity.Property(e => e.NHAPTOKHAI_USER).HasMaxLength(50);
            entity.Property(e => e.XACNHAN_NEED_NONEED_NGAY).HasColumnType("datetime");
            entity.Property(e => e.XACNHAN_NEED_NONEED_USER).HasMaxLength(50);
            entity.Property(e => e.id_Trangthai).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<KHO>(entity =>
        {
            entity.HasKey(e => new { e.MaNguyenLieu, e.Group_Code, e.Kho1 });

            entity.ToTable("KHO");

            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Kho1)
                .HasMaxLength(50)
                .HasColumnName("Kho");
            entity.Property(e => e.Id_Kho).ValueGeneratedOnAdd();
            entity.Property(e => e.nvchr_note)
                .HasMaxLength(200)
                .IsFixedLength();
        });

        modelBuilder.Entity<KHO_CHITIET>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("KHO_CHITIET");

            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Material_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Material_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
        });

        modelBuilder.Entity<KHO_DONVIQUYDOI>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("KHO_DONVIQUYDOI");

            entity.Property(e => e.DonviPO).HasMaxLength(50);
            entity.Property(e => e.DonviRequest).HasMaxLength(50);
            entity.Property(e => e.Id_Quydoi).ValueGeneratedOnAdd();
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
        });

        modelBuilder.Entity<KHO_KIEMKE>(entity =>
        {
            entity.HasKey(e => new { e.MaNguyenLieu, e.Thang, e.Group_Code, e.Kho });

            entity.ToTable("KHO_KIEMKE");

            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Id_Kiemke).ValueGeneratedOnAdd();
            entity.Property(e => e.NgayCapnhat).HasColumnType("datetime");
            entity.Property(e => e.UserCapnhat).HasMaxLength(50);
        });

        modelBuilder.Entity<KHO_NHAPXUAT>(entity =>
        {
            entity.HasKey(e => e.Id_Lichsu);

            entity.ToTable("KHO_NHAPXUAT");

            entity.HasIndex(e => new { e.MaNguyenLieu, e.Loai, e.Kho }, "IX_KHO_NHAPXUAT_MaNguyenLieu_Loai_Kho");

            entity.HasIndex(e => new { e.MaNguyenLieu, e.Loai, e.Soluong }, "IX_KHO_NHAPXUAT_MaNguyenLieu_Loai_Soluong");

            entity.HasIndex(e => new { e.MaNguyenLieu, e.Loai, e.Soluong, e.Kho }, "IX_KHO_NHAPXUAT_MaNguyenLieu_Loai_Soluong_Kho");

            entity.Property(e => e.Donvi).HasMaxLength(50);
            entity.Property(e => e.DonviPO).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.Loai)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.MaNguoinhap).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.NCC).HasMaxLength(500);
            entity.Property(e => e.Nguoicapnhat).HasMaxLength(50);
            entity.Property(e => e.Phong).HasMaxLength(50);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotaikhoan).HasMaxLength(50);
            entity.Property(e => e.Thoigian).HasColumnType("datetime");
            entity.Property(e => e.Vitri).HasMaxLength(50);
        });

        modelBuilder.Entity<KHO_XOA>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("KHO_XOA");

            entity.Property(e => e.Donvi).HasMaxLength(50);
            entity.Property(e => e.DonviPO).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.Loai)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.MaNguoinhap).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.NCC).HasMaxLength(500);
            entity.Property(e => e.Nguoicapnhat).HasMaxLength(50);
            entity.Property(e => e.Phong).HasMaxLength(50);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotaikhoan).HasMaxLength(50);
            entity.Property(e => e.Thoigian).HasColumnType("datetime");
            entity.Property(e => e.Vitri).HasMaxLength(50);
        });

        modelBuilder.Entity<LOG>(entity =>
        {
            entity.ToTable("LOG");

            entity.Property(e => e.DateCreation).HasColumnType("datetime");
            entity.Property(e => e.Explain).HasMaxLength(1000);
            entity.Property(e => e.Who).HasMaxLength(50);
        });

        modelBuilder.Entity<LOG_REQUEST>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("LOG_REQUEST");

            entity.Property(e => e.Code_Request).HasMaxLength(500);
            entity.Property(e => e.DateCreate).HasColumnType("datetime");
            entity.Property(e => e.User).HasMaxLength(50);
        });

        modelBuilder.Entity<MAILED>(entity =>
        {
            entity.ToTable("MAILED");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Kind).HasMaxLength(50);
            entity.Property(e => e.Remain).HasMaxLength(50);
            entity.Property(e => e.SendDate).HasMaxLength(50);
        });

        modelBuilder.Entity<MATEIAL_REUSE>(entity =>
        {
            entity.HasKey(e => e.Material_Code);

            entity.ToTable("MATEIAL_REUSE");

            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Amount).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Id_reuse).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<MATERIAL>(entity =>
        {
            entity.HasKey(e => e.Material_Code);

            entity.ToTable("MATERIAL");

            entity.HasIndex(e => e.Group_Code, "IX_MATERIAL_Group_Code");

            entity.HasIndex(e => new { e.Group_Code, e.Material_Code }, "IX_MATERIAL_Group_Code_Material_Code");

            entity.HasIndex(e => e.Group_Code, "IX_MATERIAL_Group_Code_included");

            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Account_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Category_EN).HasMaxLength(500);
            entity.Property(e => e.Category_JP).HasMaxLength(500);
            entity.Property(e => e.Category_VN).HasMaxLength(500);
            entity.Property(e => e.Code_Suppiler).HasMaxLength(200);
            entity.Property(e => e.Composition).HasMaxLength(2200);
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Dimension).HasMaxLength(2200);
            entity.Property(e => e.GoodKind).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Id_Material).ValueGeneratedOnAdd();
            entity.Property(e => e.Material1)
                .HasMaxLength(2200)
                .HasColumnName("Material");
            entity.Property(e => e.Material_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Material_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Material_Name_VN);
            entity.Property(e => e.Purpose).HasMaxLength(2200);
            entity.Property(e => e.Shape).HasMaxLength(2200);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
            entity.Property(e => e.UsedFor).HasMaxLength(2200);
            entity.Property(e => e.CHR_MaterialOutSide).HasMaxLength(50);
        });

        modelBuilder.Entity<MATERIAL_ACCOUNT>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MATERIAL_ACCOUNT");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Phongbanchiuchiphi).HasMaxLength(50);
        });

        modelBuilder.Entity<MATERIAL_ACOUNTCODE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MATERIAL_ACOUNTCODE");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Account_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.GoodKind).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Material_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
        });

        modelBuilder.Entity<MATERIAL_IT>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MATERIAL_IT");

            entity.Property(e => e.ChiPhi).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code_Kemtheo).HasMaxLength(50);
        });

        modelBuilder.Entity<MATERIAL_MATONG>(entity =>
        {
            entity.HasKey(e => e.MaTong);

            entity.ToTable("MATERIAL_MATONG");

            entity.Property(e => e.MaTong).HasMaxLength(50);
            entity.Property(e => e.Id_MaTong).ValueGeneratedOnAdd();
            entity.Property(e => e.Khoi).HasMaxLength(50);
        });

        modelBuilder.Entity<Master_RECEIVE_EMAIL_PRICE>(entity =>
        {
            entity.HasKey(e => e.CHR_EMPLOYEE_ID);

            entity.ToTable("Master_RECEIVE_EMAIL_PRICE");

            entity.Property(e => e.CHR_EMPLOYEE_ID)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CHR_EMPLOYEE_ADID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_EMPLOYEE_NAME).HasMaxLength(50);
            entity.Property(e => e.CHR_POSTION_GROUP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_SEC_CODE)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CHR_USER_CREATE)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CHR_USER_UPDATE)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DTM_USER_CREATE).HasColumnType("datetime");
            entity.Property(e => e.DTM_USER_UPDATE).HasColumnType("datetime");
        });

        modelBuilder.Entity<NHAP>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("NHAP");

            entity.Property(e => e.Donvi).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.Loai)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.NCC).HasMaxLength(500);
        });

        modelBuilder.Entity<OD>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("OD");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name).HasMaxLength(200);
            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Brand).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Currency_Real).HasMaxLength(10);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Expr1).HasMaxLength(50);
            entity.Property(e => e.Expr2).HasMaxLength(50);
            entity.Property(e => e.Expr3).HasMaxLength(50);
            entity.Property(e => e.Expr5).HasMaxLength(100);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Guarantee)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Hinhthuc).HasMaxLength(50);
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Id_LichsuXuat).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Last_Update).HasColumnType("datetime");
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.PO).HasMaxLength(20);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(10);
            entity.Property(e => e.Poisition).HasMaxLength(500);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.Sotokhai).HasMaxLength(200);
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
            entity.Property(e => e.Unit_Real).HasMaxLength(50);
            entity.Property(e => e.User_Update).HasMaxLength(50);
            entity.Property(e => e.Vitri).HasMaxLength(100);
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ORDER");

            entity.Property(e => e.Test).HasMaxLength(50);
        });

        modelBuilder.Entity<OUT_INPUT>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OUT_INPUT");

            entity.HasIndex(e => new { e.Cost_Center, e.Declaration }, "IX_OUT_INPUT_Cost_Center_Declaration");

            entity.HasIndex(e => new { e.Cost_Center, e.Loai }, "IX_OUT_INPUT_Cost_Center_Loai");

            entity.Property(e => e.Account_Code).HasMaxLength(100);
            entity.Property(e => e.Account_Name).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(500);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Declaration).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Loai).HasMaxLength(20);
            entity.Property(e => e.ThoigianNhap).HasMaxLength(50);
            entity.Property(e => e.UserNhap).HasMaxLength(50);

            entity.HasOne(d => d.Cost_CenterNavigation).WithMany()
                .HasForeignKey(d => d.Cost_Center)
                .HasConstraintName("FK_OUT_INPUT_DEPARTMENT");
        });

        modelBuilder.Entity<OUT_INPUT_ACCOUNT>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("OUT_INPUT_ACCOUNT");

            entity.Property(e => e.Account_Code).HasMaxLength(100);
            entity.Property(e => e.Account_Name).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(500);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Declaration).HasMaxLength(50);
            entity.Property(e => e.Loai).HasMaxLength(20);
            entity.Property(e => e.ThoigianNhap).HasMaxLength(50);
            entity.Property(e => e.UserNhap).HasMaxLength(50);
        });

        modelBuilder.Entity<PARAMETTER>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PARAMETTER");

            entity.Property(e => e.Kind).HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(50);
        });

        modelBuilder.Entity<PO>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PO");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Loaihinhtokhai).HasMaxLength(50);
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(10);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
        });

        modelBuilder.Entity<PO_Result_ThueNhaThau>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PO_Result_ThueNhaThau");

            entity.Property(e => e.CHR_PO_ER).HasMaxLength(50);
            entity.Property(e => e.CHR_PURPOSE).HasMaxLength(255);
            entity.Property(e => e.CHR_STATUS_IN_OUT).HasMaxLength(50);
            entity.Property(e => e.CHR_STATUS_UPDATE).HasMaxLength(50);
            entity.Property(e => e.DTM_DATE_INSTOCK).HasColumnType("datetime");
            entity.Property(e => e.ID_PO_ER_DETAIL).HasMaxLength(50);
            entity.Property(e => e.PO).HasMaxLength(255);
            entity.Property(e => e.TongUSD_10).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<REMAINDER>(entity =>
        {
            entity.HasKey(e => new { e.Dept, e.AccountCode, e.Month, e.Kind, e.Group_Code });

            entity.ToTable("REMAINDER");

            entity.Property(e => e.Dept).HasMaxLength(50);
            entity.Property(e => e.AccountCode).HasMaxLength(50);
            entity.Property(e => e.Kind).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Id_Remainder).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<REQUEST>(entity =>
        {
            entity.HasKey(e => e.Code_Request).HasName("PK_REQUEST_1");

            entity.ToTable("REQUEST");

            entity.HasIndex(e => new { e.Cost_Center, e.Declaration, e.Status }, "IX_REQUEST_Cost_Center_Declaration_Status");

            entity.HasIndex(e => new { e.Cost_Center, e.Note, e.Create_Date }, "IX_REQUEST_Cost_Center_Note_Create_Date");

            entity.HasIndex(e => new { e.Cost_Center, e.Status }, "IX_REQUEST_Cost_Center_Status");

            entity.HasIndex(e => new { e.Currency, e.Kind, e.Status, e.Declaration }, "IX_REQUEST_Currency_Kind_Status_Declaration");

            entity.HasIndex(e => new { e.Declaration, e.Status }, "IX_REQUEST_Declaration_Status");

            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.CostCenter).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Currency_Real).HasMaxLength(10);
            entity.Property(e => e.Declaration).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(10);
            entity.Property(e => e.Id_Request).ValueGeneratedOnAdd();
            entity.Property(e => e.Kind).HasMaxLength(20);
            entity.Property(e => e.KindofRQ).HasMaxLength(50);
            entity.Property(e => e.Last_Update).HasColumnType("datetime");
            entity.Property(e => e.Loaihinhtokhai).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.Phuongthucvanchuyen).HasMaxLength(50);
            entity.Property(e => e.Place).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.User_Create).HasMaxLength(10);
            entity.Property(e => e.User_Update).HasMaxLength(10);

            entity.HasOne(d => d.Cost_CenterNavigation).WithMany(p => p.REQUESTs)
                .HasForeignKey(d => d.Cost_Center)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_REQUEST_DEPARTMENT");
        });

        modelBuilder.Entity<REQUEST_ACCEPT>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("REQUEST_ACCEPT");

            entity.Property(e => e.Code_Request).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
        });

        modelBuilder.Entity<REQUEST_DETAIL>(entity =>
        {
            entity.HasKey(e => e.Id_RequestDetail);

            entity.ToTable("REQUEST_DETAIL");

            entity.HasIndex(e => e.Account_Code, "IX_REQUEST_DETAIL_Account_Code");

            entity.HasIndex(e => new { e.Account_Code, e.Phongchiuchiphi }, "IX_REQUEST_DETAIL_Account_Code_Phongchiuchiphi");

            entity.HasIndex(e => e.Code_Request, "IX_REQUEST_DETAIL_Code_Request");

            entity.HasIndex(e => e.Code_Request, "IX_REQUEST_DETAIL_Code_Request_Include");

            entity.HasIndex(e => new { e.Code_Request, e.Phongchiuchiphi }, "IX_REQUEST_DETAIL_Code_Request_Phongchiuchiphi");

            entity.HasIndex(e => new { e.Code_Request, e.Phongchiuchiphi }, "IX_REQUEST_DETAIL_Code_Request_Phongchiuchiphi_Included");

            entity.HasIndex(e => e.Id_Request, "IX_REQUEST_DETAIL_Id_Request");

            entity.HasIndex(e => new { e.Id_Request, e.Account_Code }, "IX_REQUEST_DETAIL_Id_Request_Account_Code");

            entity.HasIndex(e => e.Material_Code, "IX_REQUEST_DETAIL_Material_Code");

            entity.HasIndex(e => e.Phongchiuchiphi, "IX_REQUEST_DETAIL_Phongchiuchiphi");

            entity.HasIndex(e => e.Phongchiuchiphi, "IX_REQUEST_DETAIL_Phongchiuchiphi_included");

            entity.HasIndex(e => e.Status, "IX_REQUEST_DETAIL_Status");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name).HasMaxLength(200);
            entity.Property(e => e.Brand).HasMaxLength(500);
            entity.Property(e => e.CHR_USER_UPDATE_MOLD).HasMaxLength(100);
            entity.Property(e => e.Catagory1).HasMaxLength(500);
            entity.Property(e => e.Catagory2).HasMaxLength(500);
            entity.Property(e => e.Catagory3).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.CostElement).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Currency_Real).HasMaxLength(10);
            entity.Property(e => e.DTM_UPDATE_MOLD).HasColumnType("datetime");
            entity.Property(e => e.DeliveryTerm).HasMaxLength(500);
            entity.Property(e => e.Good_Code).HasMaxLength(500);
            entity.Property(e => e.Guarantee)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Id_LichsuXuat).HasMaxLength(50);
            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Last_Update).HasColumnType("datetime");
            entity.Property(e => e.MaHangTem).HasMaxLength(100);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.MethodofShip).HasMaxLength(500);
            entity.Property(e => e.NCHR_GhiChu_1).HasMaxLength(100);
            entity.Property(e => e.NCHR_GhiChu_2).HasMaxLength(100);
            entity.Property(e => e.NCHR_LienLac_SHIP).HasMaxLength(100);
            entity.Property(e => e.PO).HasMaxLength(20);
            entity.Property(e => e.PaymentTerm).HasMaxLength(500);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(10);
            entity.Property(e => e.Poisition).HasMaxLength(500);
            entity.Property(e => e.Register).HasMaxLength(500);
            entity.Property(e => e.Service_Goods).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
            entity.Property(e => e.Unit_Real).HasMaxLength(50);
            entity.Property(e => e.User_Update).HasMaxLength(50);
            entity.Property(e => e.V4DonViCan).HasMaxLength(500);
            entity.Property(e => e.VCHR_NCC_BAOGIA).HasMaxLength(100);
            entity.Property(e => e.VenderAComparingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderAPurchasingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderBComparingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderBPurchasingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderCComparingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderCPurchasingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderDComparingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderDPurchasingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderEComparingUnit).HasMaxLength(50);
            entity.Property(e => e.VenderEPurchasingUnit).HasMaxLength(50);
            entity.Property(e => e.Vitri).HasMaxLength(100);
        });

        modelBuilder.Entity<REQUEST_DETAIL_QUOATATION>(entity =>
        {
            entity.HasKey(e => e.Id_Quotation);

            entity.ToTable("REQUEST_DETAIL_QUOATATION");

            entity.Property(e => e.Code_Request).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.GoodCode).HasMaxLength(50);
            entity.Property(e => e.Pic).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.UploadTime).HasColumnType("datetime");
            entity.Property(e => e.Vendor).HasMaxLength(50);
        });

        modelBuilder.Entity<REQUEST_DETAIL_VENDOR>(entity =>
        {
            entity.HasKey(e => e.Id_Vendor);

            entity.ToTable("REQUEST_DETAIL_VENDOR");

            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.PriceUnit).HasMaxLength(50);
            entity.Property(e => e.TimeInput).HasColumnType("datetime");
            entity.Property(e => e.Vendor).HasMaxLength(50);
            entity.Property(e => e.VendorCode).HasMaxLength(50);
            entity.Property(e => e.WhoInPut).HasMaxLength(50);
        });

        modelBuilder.Entity<RETURN_GOOD>(entity =>
        {
            entity.ToTable("RETURN_GOOD");

            entity.Property(e => e.Ghichu).HasColumnType("ntext");
            entity.Property(e => e.MaSanPham).HasMaxLength(50);
            entity.Property(e => e.NguoiUp).HasMaxLength(50);
            entity.Property(e => e.NoiCatGiu).HasMaxLength(50);
            entity.Property(e => e.PhongBanCoHang).HasMaxLength(50);
            entity.Property(e => e.TenSanPham).HasMaxLength(100);
            entity.Property(e => e.ThoiGianUp).HasColumnType("datetime");
            entity.Property(e => e.TinhNang).HasColumnType("ntext");
            entity.Property(e => e.TinhTrang).HasMaxLength(50);
            entity.Property(e => e.Width).HasMaxLength(50);
        });

        modelBuilder.Entity<ROW>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ROWS");
        });

        modelBuilder.Entity<RQ_PO_Detail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RQ_PO_Detail");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name).HasMaxLength(200);
            entity.Property(e => e.Brand).HasMaxLength(500);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Good_Code).HasMaxLength(500);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Last_Update).HasColumnType("datetime");
            entity.Property(e => e.Loaihinhtokhai).HasMaxLength(50);
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(10);
            entity.Property(e => e.SoPO).HasMaxLength(20);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.User_Update).HasMaxLength(50);
        });

        modelBuilder.Entity<SPLIT>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SPLIT");

            entity.Property(e => e.DateUpdate).HasColumnType("datetime");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<TEM>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TEM");

            entity.Property(e => e.Cot1)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Cot2)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<TEM_LUONGSUDUNG>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TEM_LUONGSUDUNG");

            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.TenMatongVn).HasMaxLength(500);
            entity.Property(e => e.Tennhomhang).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<TEST>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TEST");

            entity.Property(e => e.cl1).HasMaxLength(500);
            entity.Property(e => e.cl2).HasMaxLength(500);
        });

        modelBuilder.Entity<TEn>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TEn");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Account_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.GoodKind).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Material_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
            entity.Property(e => e.cl2).HasMaxLength(500);
        });

        modelBuilder.Entity<TM_ACCOUNT>(entity =>
        {
            entity.HasKey(e => e.Account_Code);

            entity.ToTable("TM_ACCOUNT");

            entity.Property(e => e.Account_Code).HasMaxLength(50);
            entity.Property(e => e.Account_Name_EN).HasMaxLength(500);
            entity.Property(e => e.Account_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Account_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Id_Account).ValueGeneratedOnAdd();
            entity.Property(e => e.LoaiChiPhi).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<TM_AUTHORITY_MENU>(entity =>
        {
            entity.HasKey(e => new { e.CHR_USERID, e.CHR_CODE_MENU });

            entity.ToTable("TM_AUTHORITY_MENU");

            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.CHR_CODE_MENU).HasMaxLength(50);
            entity.Property(e => e.CHR_CRT_USERID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CHR_UPD_USERID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.CHR_CODE_MENUNavigation).WithMany(p => p.TM_AUTHORITY_MENUs)
                .HasForeignKey(d => d.CHR_CODE_MENU)
                .HasConstraintName("FK_TM_AUTHORITY_MENU_TM_MENU");

            entity.HasOne(d => d.CHR_USER).WithMany(p => p.TM_AUTHORITY_MENUs)
                .HasForeignKey(d => d.CHR_USERID)
                .HasConstraintName("FK_TM_AUTHORITY_MENU_TEMP_TM_USER1");
        });

        modelBuilder.Entity<TM_AUTHORITY_THEOCHUCNANG>(entity =>
        {
            entity.HasKey(e => e.CHR_CODE_FUNCTION);

            entity.ToTable("TM_AUTHORITY_THEOCHUCNANG");

            entity.Property(e => e.CHR_CODE_FUNCTION).HasMaxLength(50);
        });

        modelBuilder.Entity<TM_Category>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__TM_Categ__3214EC2731393636");

            entity.ToTable("TM_Category");

            entity.Property(e => e.CHR_CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DTM_CreateBy).HasColumnType("datetime");
            entity.Property(e => e.NVCHR_Category).HasMaxLength(200);
        });

        modelBuilder.Entity<TM_GOOD_TYPE>(entity =>
        {
            entity.HasKey(e => e.ID_CODE_GOOD_TYPE);

            entity.ToTable("TM_GOOD_TYPE");

            entity.Property(e => e.ID_CODE_GOOD_TYPE).ValueGeneratedNever();
            entity.Property(e => e.CHR_GOOD_TYPE_JP).HasMaxLength(200);
            entity.Property(e => e.CHR_GOOD_TYPE_VN).HasMaxLength(200);
            entity.Property(e => e.CHR_USER_CREATE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_USER_UPDATE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DTM_DATE_CREATE).HasColumnType("datetime");
            entity.Property(e => e.DTM_DATE_UPDATE).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_KHO_MOLD>(entity =>
        {
            entity.HasKey(e => new { e.ID_PO_ER_DETAIL, e.CHR_PO_ER, e.CHR_GOOD_CODE_BOOK, e.INT_PHANLOAI_HANGHOA });

            entity.ToTable("TM_KHO_MOLD");

            entity.Property(e => e.CHR_PO_ER).HasMaxLength(50);
            entity.Property(e => e.CHR_GOOD_CODE_BOOK).HasMaxLength(50);
            entity.Property(e => e.CHR_GOOD_NAME).HasMaxLength(200);
            entity.Property(e => e.CHR_KHO).HasMaxLength(50);
            entity.Property(e => e.CHR_PROJECT_CODE).HasMaxLength(100);
        });

        modelBuilder.Entity<TM_LOAIHINHTOKHIum>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TM_LOAIHINHTOKHIA");

            entity.Property(e => e.Condition).HasMaxLength(50);
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
            entity.Property(e => e.Kind).HasMaxLength(500);
            entity.Property(e => e.SoTk).HasMaxLength(50);
            entity.Property(e => e.TEnTk).HasMaxLength(500);
            entity.Property(e => e.Value).HasMaxLength(500);
        });

        modelBuilder.Entity<TM_MAIL_ACC>(entity =>
        {
            entity.HasKey(e => e.CHR_MANV);

            entity.ToTable("TM_MAIL_ACC");

            entity.Property(e => e.CHR_MANV)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CHR_ADID)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CHR_MAIL).HasMaxLength(100);
            entity.Property(e => e.CHR_TEN_NV).HasMaxLength(100);
        });

        modelBuilder.Entity<TM_MASTER_MAIL>(entity =>
        {
            entity.ToTable("TM_MASTER_MAIL");

            entity.Property(e => e.CHR_BCC).HasMaxLength(500);
            entity.Property(e => e.CHR_CC).HasMaxLength(500);
            entity.Property(e => e.CHR_FROM).HasMaxLength(100);
            entity.Property(e => e.CHR_NAME).HasMaxLength(500);
            entity.Property(e => e.CHR_SUBJECT).HasMaxLength(500);
            entity.Property(e => e.CHR_TO).HasMaxLength(500);
        });

        modelBuilder.Entity<TM_MENU>(entity =>
        {
            entity.HasKey(e => e.CHR_CODE_MENU).HasName("PK_TM_MENU_1");

            entity.ToTable("TM_MENU");

            entity.Property(e => e.CHR_CODE_MENU).HasMaxLength(50);
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.NVCHR_MENU).HasMaxLength(500);
        });

        modelBuilder.Entity<TM_MENU_BACKUP>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TM_MENU_BACKUP");

            entity.Property(e => e.CHR_CODE_MENU).HasMaxLength(50);
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.NVCHR_MENU).HasMaxLength(500);
        });

        modelBuilder.Entity<TM_NHAP_XUAT_KHO_MOLD_LOG>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__TM_NHAP___3214EC279B6BA2D8");

            entity.ToTable("TM_NHAP_XUAT_KHO_MOLD_LOG");

            entity.Property(e => e.CHR_DONVI).HasMaxLength(50);
            entity.Property(e => e.CHR_GOOD_CODE_BOOK).HasMaxLength(50);
            entity.Property(e => e.CHR_GOOD_NAME).HasMaxLength(255);
            entity.Property(e => e.CHR_KHO).HasMaxLength(50);
            entity.Property(e => e.CHR_NOTE).HasMaxLength(255);
            entity.Property(e => e.CHR_PO_ER).HasMaxLength(50);
            entity.Property(e => e.CHR_PROJECT_CODE).HasMaxLength(50);
            entity.Property(e => e.CHR_RQ_CODE).HasMaxLength(50);
            entity.Property(e => e.CHR_STATUS_IN_OUT).HasMaxLength(50);
            entity.Property(e => e.CHR_STATUS_IN_STOCK)
                .HasMaxLength(50)
                .HasComment("Mục đích xuất: Xuất dumg, xuất sparepart");
            entity.Property(e => e.CHR_STATUS_UPDATE).HasMaxLength(50);
            entity.Property(e => e.CHR_USERID_RECEIVED).HasMaxLength(50);
            entity.Property(e => e.CHR_USERNAME_RECEIVED).HasMaxLength(50);
            entity.Property(e => e.CHR_USER_EXPORT).HasMaxLength(50);
            entity.Property(e => e.CHR_USER_INSTOCK).HasMaxLength(50);
            entity.Property(e => e.DTM_DATE_EXPORT).HasColumnType("datetime");
            entity.Property(e => e.DTM_DATE_INSTOCK).HasColumnType("datetime");
            entity.Property(e => e.DTM_DATE_RECEIVED).HasColumnType("datetime");
            entity.Property(e => e.DTM_DATE_UPDATE).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_NOTICE>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TM_NOTICE");

            entity.Property(e => e.Dateupdate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_PO_CONFIRMED_GOOD_COME>(entity =>
        {
            entity.HasKey(e => e.CHR_PO);

            entity.ToTable("TM_PO_CONFIRMED_GOOD_COME");

            entity.Property(e => e.CHR_PO).HasMaxLength(50);
            entity.Property(e => e.CHR_USER_CONFIRM).HasMaxLength(200);
            entity.Property(e => e.DTM_DATE_CONFIRM).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_PO_NHAPKHO_MOLD_STATUS>(entity =>
        {
            entity.HasKey(e => new { e.ID_PO_ER_DETAIL, e.CHR_PO_ER });

            entity.ToTable("TM_PO_NHAPKHO_MOLD_STATUS");

            entity.Property(e => e.CHR_PO_ER).HasMaxLength(50);
            entity.Property(e => e.CHR_ACOUNT).HasMaxLength(50);
            entity.Property(e => e.CHR_DONVI).HasMaxLength(50);
            entity.Property(e => e.CHR_GOOD_CODE).HasMaxLength(50);
            entity.Property(e => e.CHR_GOOD_NAME).HasMaxLength(255);
            entity.Property(e => e.CHR_MA_HANG_HOA).HasMaxLength(50);
            entity.Property(e => e.CHR_PROJECT_CODE).HasMaxLength(50);
            entity.Property(e => e.CHR_RQ_CODE).HasMaxLength(50);
            entity.Property(e => e.CHR_SEC_CODE).HasMaxLength(50);
            entity.Property(e => e.CHR_USER_CONFIRM).HasMaxLength(200);
            entity.Property(e => e.DTM_DATE_CONFIRM).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_PURPOSE_USING>(entity =>
        {
            entity.HasKey(e => e.INT_PURPOSE_CODE);

            entity.ToTable("TM_PURPOSE_USING");

            entity.Property(e => e.INT_PURPOSE_CODE).ValueGeneratedNever();
            entity.Property(e => e.CHR_PURPOSE_TYPE_JP).HasMaxLength(200);
            entity.Property(e => e.CHR_PURPOSE_TYPE_VN).HasMaxLength(200);
            entity.Property(e => e.CHR_USER_CREATE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_USER_UPDATE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DTM_DATE_CREATE).HasColumnType("datetime");
            entity.Property(e => e.DTM_DATE_UPDATE).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_QR_CODE>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__TM_QR_CO__3214EC275BB16D8F");

            entity.ToTable("TM_QR_CODE");

            entity.Property(e => e.CHR_GOOD_NAME).HasMaxLength(200);
            entity.Property(e => e.CHR_NUM_CAV).HasMaxLength(50);
            entity.Property(e => e.CHR_PO).HasMaxLength(200);
            entity.Property(e => e.CHR_PROJECT_CODE).HasMaxLength(200);
            entity.Property(e => e.CHR_USER_PRINT).HasMaxLength(100);
        });

        modelBuilder.Entity<TM_REPORT>(entity =>
        {
            entity.HasKey(e => e.INT_MAYEUCAU);

            entity.ToTable("TM_REPORT");

            entity.Property(e => e.CHR_ADID_ACC).HasMaxLength(15);
            entity.Property(e => e.CHR_ADID_NGUOIYEUCAU).HasMaxLength(50);
            entity.Property(e => e.CHR_ADID_QLSC).HasMaxLength(15);
            entity.Property(e => e.CHR_ADID_QLTC).HasMaxLength(15);
            entity.Property(e => e.CHR_EXCEL).HasMaxLength(200);
            entity.Property(e => e.CHR_GHICHU).HasMaxLength(200);
            entity.Property(e => e.CHR_LINK_PDF).HasMaxLength(200);
            entity.Property(e => e.CHR_LYDO_TUCHOI).HasMaxLength(200);
            entity.Property(e => e.CHR_STT_ACC).HasMaxLength(2);
            entity.Property(e => e.CHR_STT_NGUOIYEUCAU).HasMaxLength(2);
            entity.Property(e => e.CHR_STT_QLSC).HasMaxLength(2);
            entity.Property(e => e.CHR_STT_QLTC).HasMaxLength(2);
            entity.Property(e => e.CHR_TENYEUCAU).HasMaxLength(200);
            entity.Property(e => e.DTM_CHOP_ACC).HasColumnType("datetime");
            entity.Property(e => e.DTM_CHOP_QLSC).HasColumnType("datetime");
            entity.Property(e => e.DTM_CHOP_QLTC).HasColumnType("datetime");
            entity.Property(e => e.DTM_NGAYTAODON).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_REPORT_HISTORY>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TM_REPORT_HISTORY");

            entity.Property(e => e.CHR_ADID_XULY).HasMaxLength(50);
            entity.Property(e => e.CHR_VITRI_XULY).HasMaxLength(50);
            entity.Property(e => e.DTM_TIME_XULY).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_TEMP_FORM_ORDER>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TM_TEMP_FORM_ORDER");

            entity.Property(e => e.CHR_DONGIA).HasMaxLength(200);
            entity.Property(e => e.CHR_DONVI).HasMaxLength(200);
            entity.Property(e => e.CHR_GHICHU).HasMaxLength(200);
            entity.Property(e => e.CHR_HAIQUAN).HasMaxLength(200);
            entity.Property(e => e.CHR_HANGHOA).HasMaxLength(200);
            entity.Property(e => e.CHR_HANGSX).HasMaxLength(200);
            entity.Property(e => e.CHR_MADUAN).HasMaxLength(200);
            entity.Property(e => e.CHR_MASP).HasMaxLength(200);
            entity.Property(e => e.CHR_MUCDICHCHUNG).HasMaxLength(200);
            entity.Property(e => e.CHR_MUCDICHSD).HasMaxLength(200);
            entity.Property(e => e.CHR_SOLUONG).HasMaxLength(200);
            entity.Property(e => e.CHR_STK).HasMaxLength(200);
            entity.Property(e => e.CHR_TEN_TIENGANH).HasMaxLength(200);
            entity.Property(e => e.CHR_TEN_TIENGVIET).HasMaxLength(200);
            entity.Property(e => e.CHR_USER_SAVE).HasMaxLength(50);
        });

        modelBuilder.Entity<TM_TRADE_CUSTOM_TYPE>(entity =>
        {
            entity.HasKey(e => e.CHR_TRADE_TYPE);

            entity.ToTable("TM_TRADE_CUSTOM_TYPE");

            entity.Property(e => e.CHR_TRADE_TYPE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CHR_TRADE_TYPE_JP).HasMaxLength(200);
            entity.Property(e => e.CHR_TRADE_TYPE_VN).HasMaxLength(200);
            entity.Property(e => e.CHR_USER_CREATE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CHR_USER_UPDATE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DTM_DATE_CREATE).HasColumnType("datetime");
            entity.Property(e => e.DTM_DATE_UPDATE).HasColumnType("datetime");
        });

        modelBuilder.Entity<TM_USER>(entity =>
        {
            entity.HasKey(e => e.CHR_USERID);

            entity.ToTable("TM_USER");

            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.CHR_ADID_GROUPUSER)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHR_CRT_USERID).HasMaxLength(50);
            entity.Property(e => e.CHR_EMPLOYEE_ID)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.CHR_SECTION)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DTM_CREATE).HasColumnType("datetime");
            entity.Property(e => e.DTM_LAST_LOGIN).HasColumnType("datetime");
            entity.Property(e => e.FULLNAME).HasMaxLength(500);
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
            entity.Property(e => e.INT_LOCK).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.INT_LOCK_DAY).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Lancuoicungdangnhap).HasColumnType("datetime");
            entity.Property(e => e.VCHR_PASSWORD).HasMaxLength(50);
            entity.Property(e => e.dia_chi_mail).HasMaxLength(50);
            entity.Property(e => e.phong_ban).HasMaxLength(50);
            entity.Property(e => e.thoi_gian_cap_nhat).HasColumnType("datetime");

            entity.HasMany(d => d.CHR_CODE_FUNCTIONs).WithMany(p => p.CHR_USERs)
                .UsingEntity<Dictionary<string, object>>(
                    "TM_AUTHORITY_FUNCTION",
                    r => r.HasOne<TM_AUTHORITY_THEOCHUCNANG>().WithMany()
                        .HasForeignKey("CHR_CODE_FUNCTION")
                        .HasConstraintName("FK_TM_AUTHORITY_FUNCTION_TM_AUTHORITY_THEOCHUCNANG"),
                    l => l.HasOne<TM_USER>().WithMany()
                        .HasForeignKey("CHR_USERID")
                        .HasConstraintName("FK_TM_AUTHORITY_FUNCTION_TM_USER"),
                    j =>
                    {
                        j.HasKey("CHR_USERID", "CHR_CODE_FUNCTION");
                        j.ToTable("TM_AUTHORITY_FUNCTION");
                        j.IndexerProperty<string>("CHR_USERID").HasMaxLength(50);
                        j.IndexerProperty<string>("CHR_CODE_FUNCTION").HasMaxLength(50);
                    });
        });

        modelBuilder.Entity<TM_USER_GROUP_USING>(entity =>
        {
            entity.HasKey(e => e.CHR_USERID);

            entity.ToTable("TM_USER_GROUP_USING");

            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.CHR_CODE_GROUP_NAME).HasMaxLength(200);
            entity.Property(e => e.CHR_GROUP_NAME).HasMaxLength(200);
        });

        modelBuilder.Entity<TM_USER_TEST>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TM_USER_TEST");

            entity.Property(e => e.DtmLast).HasColumnType("datetime");
            entity.Property(e => e.UserID)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TONTRENLINE>(entity =>
        {
            entity.HasKey(e => new { e.MaNguyenLieu, e.Thang, e.Cost, e.Vitri, e.Nhamay }).HasName("PK_TONTRENLINE_1");

            entity.ToTable("TONTRENLINE");

            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Cost).HasMaxLength(50);
            entity.Property(e => e.Vitri).HasMaxLength(50);
            entity.Property(e => e.Nhamay).HasMaxLength(50);
            entity.Property(e => e.Id_TonLine).ValueGeneratedOnAdd();
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.NgayCapnhat).HasColumnType("datetime");
            entity.Property(e => e.UserCapnhat).HasMaxLength(50);
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("USER");

            entity.Property(e => e.CHR_CRT_USERID).HasMaxLength(50);
            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.FULLNAME).HasMaxLength(500);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.INT_LOCK).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.VCHR_PASSWORD).HasMaxLength(50);
        });

        modelBuilder.Entity<USER_DEPT>(entity =>
        {
            entity.HasKey(e => new { e.CHR_USERID, e.Cost_Center }).HasName("PK_USER_DEPT_1");

            entity.ToTable("USER_DEPT");

            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Id_User_Dept).ValueGeneratedOnAdd();

            entity.HasOne(d => d.CHR_USER).WithMany(p => p.USER_DEPTs)
                .HasForeignKey(d => d.CHR_USERID)
                .HasConstraintName("FK_USER_DEPT_TM_USER");

            entity.HasOne(d => d.Cost_CenterNavigation).WithMany(p => p.USER_DEPTs)
                .HasForeignKey(d => d.Cost_Center)
                .HasConstraintName("FK_USER_DEPT_DEPARTMENT");
        });

        modelBuilder.Entity<V2_FORM>(entity =>
        {
            entity.HasKey(e => e.MaDon);

            entity.ToTable("V2_FORM");

            entity.Property(e => e.MaDon).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Id_Madon).ValueGeneratedOnAdd();
            entity.Property(e => e.LoaiChiPhi).HasMaxLength(50);
            entity.Property(e => e.LoaiDon).HasMaxLength(50);
            entity.Property(e => e.LoaiTien).HasMaxLength(50);
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.Ngaycapnhat).HasColumnType("datetime");
            entity.Property(e => e.NguoiTao).HasMaxLength(50);
            entity.Property(e => e.NguoiYeuCau).HasMaxLength(50);
            entity.Property(e => e.Nguoicapnhat).HasMaxLength(50);
            entity.Property(e => e.TenPhong).HasMaxLength(500);
            entity.Property(e => e.TinhTrang).HasMaxLength(50);

            entity.HasOne(d => d.Cost_CenterNavigation).WithMany(p => p.V2_FORMs)
                .HasForeignKey(d => d.Cost_Center)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_V2_FORM_DEPARTMENT");
        });

        modelBuilder.Entity<V2_FORM_ALL>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V2_FORM_ALL");

            entity.Property(e => e.ActualEstimate).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Dieukhoanthanhtoan).HasMaxLength(500);
            entity.Property(e => e.Kydichvu).HasMaxLength(50);
            entity.Property(e => e.LoaiChiPhi).HasMaxLength(50);
            entity.Property(e => e.LoaiDon).HasMaxLength(50);
            entity.Property(e => e.LoaiTien).HasMaxLength(50);
            entity.Property(e => e.MaDon).HasMaxLength(50);
            entity.Property(e => e.Ncc).HasMaxLength(500);
            entity.Property(e => e.OverseaDomestic).HasMaxLength(50);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(50);
            entity.Property(e => e.Sotaikhoan).HasMaxLength(50);
            entity.Property(e => e.TenPhong).HasMaxLength(50);
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(500);
            entity.Property(e => e.TinhTrang).HasMaxLength(50);
        });

        modelBuilder.Entity<V2_FORM_CHITIET>(entity =>
        {
            entity.HasKey(e => e.MaDonChiTiet);

            entity.ToTable("V2_FORM_CHITIET");

            entity.HasIndex(e => new { e.LoaiChiPhi, e.Phongchiuchiphi }, "IX_V2_FORM_CHITIET_LoaiChiPhi_Phongchiuchiphi");

            entity.HasIndex(e => new { e.LoaiChiPhi, e.Phongchiuchiphi, e.MaDon }, "IX_V2_FORM_CHITIET_LoaiChiPhi_Phongchiuchiphi_MaDon");

            entity.HasIndex(e => new { e.Sotaikhoan, e.Phongchiuchiphi }, "IX_V2_FORM_CHITIET_Sotaikhoan_Phongchiuchiphi");

            entity.Property(e => e.ActualEstimate).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Dieukhoanthanhtoan).HasMaxLength(500);
            entity.Property(e => e.Kydichvu).HasMaxLength(50);
            entity.Property(e => e.LoaiChiPhi).HasMaxLength(50);
            entity.Property(e => e.LoaiDon).HasMaxLength(50);
            entity.Property(e => e.LoaiTien).HasMaxLength(50);
            entity.Property(e => e.MaDon).HasMaxLength(50);
            entity.Property(e => e.Ncc).HasMaxLength(500);
            entity.Property(e => e.OverseaDomestic).HasMaxLength(50);
            entity.Property(e => e.Phongchiuchiphi).HasMaxLength(50);
            entity.Property(e => e.Sotaikhoan).HasMaxLength(50);
            entity.Property(e => e.TenPhong).HasMaxLength(50);
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(500);
            entity.Property(e => e.TinhTrang).HasMaxLength(50);

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.V2_FORM_CHITIETs)
                .HasForeignKey(d => d.MaDon)
                .HasConstraintName("FK_V2_FORM_CHITIET_V2_FORM1");
        });

        modelBuilder.Entity<V3_CATAGORY>(entity =>
        {
            entity.HasKey(e => e.Id_Catagory).HasName("PK_V3_CATAGORY_1");

            entity.ToTable("V3_CATAGORY");

            entity.Property(e => e.Catagory1).HasMaxLength(500);
            entity.Property(e => e.Catagory2).HasMaxLength(500);
            entity.Property(e => e.Catagory3).HasMaxLength(500);
        });

        modelBuilder.Entity<V3_CATAGORY_MAPPING>(entity =>
        {
            entity.HasKey(e => new { e.Id_Catagory, e.Material_Code });

            entity.ToTable("V3_CATAGORY_MAPPING");

            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Id_Catagorymap).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<V3_CATAGORY_NEW>(entity =>
        {
            entity.HasKey(e => e.Id_Catagory).HasName("PK_V3_CATAGORY");

            entity.ToTable("V3_CATAGORY_NEW");

            entity.Property(e => e.Category1EN).HasMaxLength(500);
            entity.Property(e => e.Category1VN).HasMaxLength(500);
            entity.Property(e => e.Category2EN).HasMaxLength(500);
            entity.Property(e => e.Category2VN).HasMaxLength(500);
            entity.Property(e => e.Category3EN).HasMaxLength(500);
            entity.Property(e => e.Category3VN).HasMaxLength(500);
            entity.Property(e => e.CategoryCode).HasMaxLength(50);
        });

        modelBuilder.Entity<V3_EMAIL>(entity =>
        {
            entity.HasKey(e => e.Id_Email).HasName("PK_V3_EMAIL_1");

            entity.ToTable("V3_EMAIL");

            entity.Property(e => e.CHR_CRT_USERID).HasMaxLength(500);
            entity.Property(e => e.CHR_USERID).HasMaxLength(500);
            entity.Property(e => e.Department).HasMaxLength(50);
            entity.Property(e => e.FULLNAME).HasMaxLength(500);
        });

        modelBuilder.Entity<V3_EMAILCONTENT>(entity =>
        {
            entity.ToTable("V3_EMAILCONTENT");

            entity.Property(e => e.ContentRow).HasMaxLength(4000);
        });

        modelBuilder.Entity<V3_NOT_BELONG_CATAGORY>(entity =>
        {
            entity.ToTable("V3_NOT_BELONG_CATAGORY");

            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.TenEn).HasMaxLength(500);
            entity.Property(e => e.TenJp).HasMaxLength(500);
            entity.Property(e => e.TenVn).HasMaxLength(500);
        });

        modelBuilder.Entity<V3_POCONFIRM>(entity =>
        {
            entity.ToTable("V3_POCONFIRM");

            entity.Property(e => e.DateConfirm).HasColumnType("datetime");
            entity.Property(e => e.PO).HasMaxLength(50);
            entity.Property(e => e.Q1).HasMaxLength(50);
            entity.Property(e => e.Q2).HasMaxLength(50);
            entity.Property(e => e.Q3).HasMaxLength(50);
            entity.Property(e => e.UserConfirm).HasMaxLength(50);
        });

        modelBuilder.Entity<VERSION>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("VERSION");

            entity.Property(e => e.Detail).HasMaxLength(500);
            entity.Property(e => e.Version1)
                .HasMaxLength(50)
                .HasColumnName("Version");
        });

        modelBuilder.Entity<VIEWPO_NCC>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEWPO_NCC");

            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Expr1).HasMaxLength(50);
            entity.Property(e => e.Expr2).HasMaxLength(50);
            entity.Property(e => e.Expr3).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Hinhthuc).HasMaxLength(50);
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.Ngaytao).HasColumnType("datetime");
            entity.Property(e => e.Nguoixacnhan).HasMaxLength(50);
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.Sotokhai).HasMaxLength(200);
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Thoigianxacnhan).HasColumnType("datetime");
            entity.Property(e => e.TinhtrangPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPO).HasMaxLength(50);
            entity.Property(e => e.TinhtranghaiquanPONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.TinhtranghaiquanPONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
        });

        modelBuilder.Entity<VIEW_DEPARTMENT_VITRI>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_DEPARTMENT_VITRI");

            entity.Property(e => e.MaChuyen).HasMaxLength(50);
            entity.Property(e => e.MaCost).HasMaxLength(50);
            entity.Property(e => e.MaMay).HasMaxLength(50);
            entity.Property(e => e.MaPhong).HasMaxLength(50);
            entity.Property(e => e.Mahangmuctheovitri).HasMaxLength(50);
            entity.Property(e => e.TenChuyen).HasMaxLength(50);
        });

        modelBuilder.Entity<VIEW_DEPT_ESTIMATE_DEADLINE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_DEPT_ESTIMATE_DEADLINE");

            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Time).HasMaxLength(50);
            entity.Property(e => e.TimeEnd).HasColumnType("datetime");
            entity.Property(e => e.TimeStart).HasColumnType("datetime");
        });

        modelBuilder.Entity<VIEW_DEPT_REQUEST>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_DEPT_REQUEST");

            entity.Property(e => e.Account_Code).HasMaxLength(500);
            entity.Property(e => e.Account_Name).HasMaxLength(500);
            entity.Property(e => e.Aim).HasMaxLength(50);
            entity.Property(e => e.Amount).HasMaxLength(50);
            entity.Property(e => e.Amount_Real).HasMaxLength(50);
            entity.Property(e => e.Brand).HasMaxLength(500);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Cost_Center_Group).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.Dealine_Real).HasMaxLength(50);
            entity.Property(e => e.Expr2).HasMaxLength(500);
            entity.Property(e => e.Guarantee).HasMaxLength(1000);
            entity.Property(e => e.Last_Update)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Material_Code).HasMaxLength(100);
            entity.Property(e => e.Material_Name_ENJP).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Name_Jp).HasMaxLength(500);
            entity.Property(e => e.PO).HasColumnType("datetime");
            entity.Property(e => e.Price).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(500);
            entity.Property(e => e.Total).HasMaxLength(50);
            entity.Property(e => e.Total_Real).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
            entity.Property(e => e.Unit_Real).HasMaxLength(500);
            entity.Property(e => e.User_Update).HasMaxLength(50);
        });

        modelBuilder.Entity<VIEW_HISTORY_PO_DETAIL>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_HISTORY_PO_DETAIL");

            entity.Property(e => e.Issue_PO_Date).HasColumnName("Issue PO Date");
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.UnitPriceUSD).HasColumnType("money");
        });

        modelBuilder.Entity<VIEW_HISTORY_PO_DETAIL_AUTO>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_HISTORY_PO_DETAIL_AUTO");

            entity.Property(e => e.Issue_PO_Date).HasColumnName("Issue PO Date");
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.UnitPriceUSD).HasColumnType("money");
        });

        modelBuilder.Entity<VIEW_MATERIAL_REUSE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_MATERIAL_REUSE");

            entity.Property(e => e.Amount).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Unit_Note).HasMaxLength(500);
        });

        modelBuilder.Entity<VIEW_NHOMHANG>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_NHOMHANG");

            entity.Property(e => e.Manhomhang).HasMaxLength(50);
            entity.Property(e => e.Material_Code).HasMaxLength(50);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
        });

        modelBuilder.Entity<VIEW_PROCESS_PRIVATE>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_PROCESS_PRIVATE");

            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Create_User).HasMaxLength(50);
            entity.Property(e => e.Id_Process).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Process_Date).HasColumnType("datetime");
            entity.Property(e => e.Process_User).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<VIEW_UPDATETOKHAI>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_UPDATETOKHAI");

            entity.Property(e => e.Benxacnhantruoc)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Danhmuc).HasMaxLength(50);
            entity.Property(e => e.DoisangUSD).HasColumnType("money");
            entity.Property(e => e.Dongia).HasColumnType("money");
            entity.Property(e => e.Id_LichsuNhap).HasMaxLength(50);
            entity.Property(e => e.Invoice).HasMaxLength(50);
            entity.Property(e => e.InvoiceNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNguoinhap).HasMaxLength(50);
            entity.Property(e => e.InvoicePO).HasMaxLength(50);
            entity.Property(e => e.InvoicePODenghithanhtoan).HasMaxLength(50);
            entity.Property(e => e.InvoicePONgaynhap).HasColumnType("datetime");
            entity.Property(e => e.InvoicePONguoinhap).HasMaxLength(50);
            entity.Property(e => e.Kiemtratk).HasMaxLength(50);
            entity.Property(e => e.Loaitien).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoKhonhap).HasMaxLength(50);
            entity.Property(e => e.LuongvekhoNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvekhoNguoinhap).HasMaxLength(50);
            entity.Property(e => e.LuongvethucteNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.LuongvethucteNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Maphongyeucau).HasMaxLength(50);
            entity.Property(e => e.Ngaydangkytk).HasColumnType("datetime");
            entity.Property(e => e.SoPO).HasMaxLength(50);
            entity.Property(e => e.Sotien).HasColumnType("money");
            entity.Property(e => e.SotokhaiNgaynhap).HasColumnType("datetime");
            entity.Property(e => e.SotokhaiNguoinhap).HasMaxLength(50);
            entity.Property(e => e.Tinhtrangtokhai).HasMaxLength(50);
            entity.Property(e => e.Tygia).HasColumnType("money");
        });

        modelBuilder.Entity<VIEW_USER_DEPT>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_USER_DEPT");

            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(500);
        });

        modelBuilder.Entity<View_XXXXX>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_XXXXX");

            entity.Property(e => e.CHR_CODE_MENU).HasMaxLength(50);
            entity.Property(e => e.CHR_CRT_USERID).HasMaxLength(50);
            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.DTM_CREATE).HasColumnType("datetime");
            entity.Property(e => e.Expr1)
                .HasMaxLength(22)
                .IsUnicode(false);
            entity.Property(e => e.Expr2).HasMaxLength(50);
            entity.Property(e => e.Expr3)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.Expr4)
                .HasMaxLength(1)
                .IsUnicode(false);
        });

        modelBuilder.Entity<WF_BAOGIum>(entity =>
        {
            entity.HasKey(e => e.Id_Baogia);

            entity.ToTable("WF_BAOGIA");

            entity.Property(e => e.DonVi).HasMaxLength(50);
            entity.Property(e => e.GhiChu).HasColumnType("ntext");
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.MaHang).HasMaxLength(50);
            entity.Property(e => e.MaHangTem).HasMaxLength(50);
            entity.Property(e => e.MaRequest).HasMaxLength(100);
            entity.Property(e => e.NCC).HasMaxLength(50);
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.NguoiTao).HasMaxLength(50);
        });

        modelBuilder.Entity<WF_CREATEDID>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("WF_CREATEDID");

            entity.Property(e => e.CREATEDID).HasMaxLength(50);
        });

        modelBuilder.Entity<WF_HISTORY>(entity =>
        {
            entity.HasKey(e => e.Id_History);

            entity.ToTable("WF_HISTORY");

            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Create_User).HasMaxLength(50);
            entity.Property(e => e.Create_UserName).HasMaxLength(500);
            entity.Property(e => e.Id_Process).HasMaxLength(50);

            entity.HasOne(d => d.Id_ProcessNavigation).WithMany(p => p.WF_HISTORies)
                .HasForeignKey(d => d.Id_Process)
                .HasConstraintName("FK_WF_HISTORY_WF_PROCESS");
        });

        modelBuilder.Entity<WF_PROCESS>(entity =>
        {
            entity.HasKey(e => e.Id_Process);

            entity.ToTable("WF_PROCESS");

            entity.Property(e => e.Id_Process).HasMaxLength(50);
            entity.Property(e => e.Code_Request).HasMaxLength(100);
            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Create_User).HasMaxLength(50);
            entity.Property(e => e.Create_UserName).HasMaxLength(500);
            entity.Property(e => e.Id_WF).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Code_RequestNavigation).WithMany(p => p.WF_PROCESSes)
                .HasForeignKey(d => d.Code_Request)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_WF_PROCESS_REQUEST");

            entity.HasOne(d => d.Id_WFNavigation).WithMany(p => p.WF_PROCESSes)
                .HasForeignKey(d => d.Id_WF)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_WF_PROCESS_WF_WORKFOLLOWLIST");
        });

        modelBuilder.Entity<WF_PROCESS_STEP>(entity =>
        {
            entity.HasKey(e => e.Id_Process_Step);

            entity.ToTable("WF_PROCESS_STEP");

            entity.Property(e => e.Id_Process).HasMaxLength(50);
            entity.Property(e => e.Pic).HasMaxLength(50);
            entity.Property(e => e.PicName).HasMaxLength(500);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Process_Date).HasColumnType("datetime");
            entity.Property(e => e.Process_User).HasMaxLength(50);
            entity.Property(e => e.Process_UserName).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.SubPic).HasMaxLength(50);
            entity.Property(e => e.SubPicName).HasMaxLength(500);
            entity.Property(e => e.SubPosition).HasMaxLength(50);
            entity.Property(e => e.SubRole).HasMaxLength(100);

            entity.HasOne(d => d.Id_ProcessNavigation).WithMany(p => p.WF_PROCESS_STEPs)
                .HasForeignKey(d => d.Id_Process)
                .HasConstraintName("FK_WF_PROCESS_STEP_WF_PROCESS");
        });

        modelBuilder.Entity<WF_WORKFOLLOWLIST>(entity =>
        {
            entity.HasKey(e => e.Id_WF).HasName("PK_WF_WORKFOLLOW");

            entity.ToTable("WF_WORKFOLLOWLIST");

            entity.Property(e => e.Id_WF).HasMaxLength(50);
            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Create_User).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.WF_Name).HasMaxLength(500);
        });

        modelBuilder.Entity<WF_WORKFOLLOWSTEP>(entity =>
        {
            entity.HasKey(e => new { e.Id_WF, e.Position, e.Step });

            entity.ToTable("WF_WORKFOLLOWSTEP");

            entity.Property(e => e.Id_WF).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Create_Date).HasColumnType("datetime");
            entity.Property(e => e.Create_User).HasMaxLength(50);
            entity.Property(e => e.Id_Step).ValueGeneratedOnAdd();
            entity.Property(e => e.Refuse_Step)
                .HasMaxLength(50)
                .HasComment("Được phép trả về những bước nào");
            entity.Property(e => e.Reuse_Email)
                .HasMaxLength(50)
                .HasComment("Những  bước nhận được email thông báo nếu Refuse");
            entity.Property(e => e.note).HasMaxLength(500);

            entity.HasOne(d => d.Id_WFNavigation).WithMany(p => p.WF_WORKFOLLOWSTEPs)
                .HasForeignKey(d => d.Id_WF)
                .HasConstraintName("FK_WF_WORKFOLLOWSTEP_WF_WORKFOLLOWLIST1");
        });

        modelBuilder.Entity<XUAT>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("XUAT");

            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaChuyen).HasMaxLength(50);
            entity.Property(e => e.MaCost).HasMaxLength(50);
            entity.Property(e => e.MaMay).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.MaPhong).HasMaxLength(50);
            entity.Property(e => e.TenChuyen).HasMaxLength(50);
        });

        modelBuilder.Entity<XUAT_ACC>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("XUAT_ACC");

            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaChuyen).HasMaxLength(50);
            entity.Property(e => e.MaMay).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.MaPhong).HasMaxLength(50);
            entity.Property(e => e.Phong).HasMaxLength(50);
            entity.Property(e => e.TenChuyen).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<XUAT_GA>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("XUAT_GA");

            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaChuyen).HasMaxLength(50);
            entity.Property(e => e.MaCost).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Material_Name_JP).HasMaxLength(500);
            entity.Property(e => e.Material_Name_VN).HasMaxLength(500);
            entity.Property(e => e.Sotaikhoan).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<XUAT_GA_TONG>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("XUAT_GA_TONG");

            entity.Property(e => e.Kho).HasMaxLength(50);
            entity.Property(e => e.Khoi).HasMaxLength(50);
            entity.Property(e => e.MaCost).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<XULYDONHANG>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("XULYDONHANG");

            entity.Property(e => e.CHR_CODE_MENU).HasMaxLength(50);
            entity.Property(e => e.CHR_USERID).HasMaxLength(50);
            entity.Property(e => e.Cost_Center).HasMaxLength(50);
            entity.Property(e => e.Group_Code).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class BaoGia_WorkflowStepDTO
    {
        public int StepID { get; set; }

        public int StageID { get; set; }

        public string StepCode { get; set; } = null!;

        public string StepName { get; set; } = null!;

        public string? StepNameEN { get; set; }

        public string? Description { get; set; }

        public int? DefaultDurationHours { get; set; }

        public int? StepOrder { get; set; }

        public bool IsActive { get; set; }

    }
}

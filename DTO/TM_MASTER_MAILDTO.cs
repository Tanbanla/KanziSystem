using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class TM_MASTER_MAILDTO
{
    public int ID { get; set; }

    public string? CHR_NAME { get; set; }

    public string? CHR_SUBJECT { get; set; }

    public string? CHR_FROM { get; set; }

    public string? CHR_TO { get; set; }

    public string? CHR_CC { get; set; }

    public string? CHR_BCC { get; set; }

    public string? CHR_BODY { get; set; }
}

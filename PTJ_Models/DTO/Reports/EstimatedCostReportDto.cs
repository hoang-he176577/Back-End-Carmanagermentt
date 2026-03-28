using System.Collections.Generic;

namespace Models.DTO.Reports;

public sealed class EstimatedCostReportDto
{
    public List<EstimatedCostBranchSummaryDto> Branches { get; set; } = new();
    public EstimatedCostSummaryDto Total { get; set; } = new();
}

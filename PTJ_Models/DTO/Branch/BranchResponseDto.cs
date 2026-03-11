using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Branch
{
    public class BranchResponseDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

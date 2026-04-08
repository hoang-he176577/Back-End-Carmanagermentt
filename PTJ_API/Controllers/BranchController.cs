using Microsoft.AspNetCore.Mvc;
using Models.DTO.Branch;
using Service.Services.VehicleAssets.Interfaces;

namespace API.Controllers
{
    public class BranchController : BaseController
    {
        private readonly IBranchService _service;

        public BranchController(IBranchService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BranchCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return HandleCreated(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(BranchUpdateDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return HandleSuccess("Deleted successfully");
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var result = await _service.DropdownAsync();
            return HandleResult(result);
        }
    }
}

using HospitalSchedulingApp.Common;
using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dal.Repositories;
using HospitalSchedulingApp.Dtos.Shift.Requests;
using HospitalSchedulingApp.Dtos.Shift.Response;
using HospitalSchedulingApp.Services.Interfaces;

namespace HospitalSchedulingApp.Services
{
    public class PlannedShiftService : IPlannedShiftService
    {
        private readonly IRepository<PlannedShift> _plannedShiftRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<ShiftType> _shiftTypeRepo;
        private readonly IRepository<ShiftStatus> _shiftStatusRepo;
        private readonly IRepository<Staff> _staffRepo;

        public PlannedShiftService(
            IRepository<PlannedShift> plannedShiftRepo,
            IRepository<Department> departmentRepo,
            IRepository<ShiftType> shiftTypeRepo,
            IRepository<ShiftStatus> shiftStatusRepo,
            IRepository<Staff> staffRepo)
        {
            _plannedShiftRepo = plannedShiftRepo;
            _departmentRepo = departmentRepo;
            _shiftTypeRepo = shiftTypeRepo;
            _shiftStatusRepo = shiftStatusRepo;
            _staffRepo = staffRepo;
        }

        // Calendar UI Call
        public async Task<List<PlannedShiftDto>> FetchPlannedShiftsAsync(DateTime startDate, DateTime endDate)
        {
            var shifts = await _plannedShiftRepo.GetAllAsync();
            var departments = await _departmentRepo.GetAllAsync();
            var shiftTypes = await _shiftTypeRepo.GetAllAsync();
            var staff = await _staffRepo.GetAllAsync();

            var filteredShifts = shifts
                .Where(s => s.ShiftDate >= startDate && s.ShiftDate <= endDate)
                .OrderBy(s => s.ShiftDate)
                .ToList();

            var dtos = filteredShifts.Select(shift =>
            {
                var staffMember = shift.AssignedStaffId.HasValue
                    ? staff.FirstOrDefault(s => s.StaffId == shift.AssignedStaffId.Value)
                    : null;

                return new PlannedShiftDto
                {
                    PlannedShiftId = shift.PlannedShiftId,
                    ShiftDate = shift.ShiftDate,
                    SlotNumber = shift.SlotNumber,
                    ShiftTypeName = shiftTypes.FirstOrDefault(st => st.ShiftTypeId ==(int) shift.ShiftTypeId)?.ShiftTypeName ?? string.Empty,
                    AssignedStaffFullName = staffMember?.StaffName ?? string.Empty,
                    ShiftDeparmentName = departments.FirstOrDefault(d => d.DepartmentId == shift.DepartmentId)?.DepartmentName ?? string.Empty
                };
            }).ToList();

            return dtos;
        }

        // Chat Interface call
        public async Task<List<PlannedShiftDetailDto>> FetchDetailedPlannedShiftsAsync(DateTime startDate, DateTime endDate)
        {
            var shifts = await _plannedShiftRepo.GetAllAsync();
            var departments = await _departmentRepo.GetAllAsync();
            var shiftTypes = await _shiftTypeRepo.GetAllAsync();
            var shiftStatuses = await _shiftStatusRepo.GetAllAsync();
            var staff = await _staffRepo.GetAllAsync();

            var filteredShifts = shifts
                .Where(s => s.ShiftDate >= startDate && s.ShiftDate <= endDate)
                .OrderBy(s => s.ShiftDate)
                .ToList();

            var dtos = filteredShifts.Select(shift =>
            {
                var assignedStaff = shift.AssignedStaffId.HasValue
                    ? staff.FirstOrDefault(s => s.StaffId == shift.AssignedStaffId.Value)
                    : null;

                return new PlannedShiftDetailDto
                {
                    PlannedShiftId = shift.PlannedShiftId,

                    // Core values
                    ShiftDate = shift.ShiftDate,
                    SlotNumber = shift.SlotNumber,

                    // IDs
                    ShiftTypeId =(int) shift.ShiftTypeId,
                    DepartmentId = shift.DepartmentId,
                    ShiftStatusId = (int)shift.ShiftStatusId,
                    AssignedStaffId = shift.AssignedStaffId,

                    // Resolved names
                    ShiftTypeName = shiftTypes.FirstOrDefault(st => st.ShiftTypeId == (int)shift.ShiftTypeId)?.ShiftTypeName ?? string.Empty,
                    ShiftDeparmentName = departments.FirstOrDefault(d => d.DepartmentId == shift.DepartmentId)?.DepartmentName ?? string.Empty,
                    ShiftStatusName = shiftStatuses.FirstOrDefault(ss => ss.ShiftStatusId == (int)shift.ShiftStatusId)?.ShiftStatusName ?? string.Empty,
                    AssignedStaffFullName = assignedStaff?.StaffName ?? string.Empty,
                    AssignedStaffDepartmentName = assignedStaff != null
                        ? departments.FirstOrDefault(d => d.DepartmentId == assignedStaff.StaffDepartmentId)?.DepartmentName ?? string.Empty
                        : string.Empty
                };
            }).ToList();


            return dtos;
        }


        public async Task<List<PlannedShiftDetailDto>> FetchFilteredPlannedShiftsAsync(ShiftFilterDto filter)
        {
            var shifts = await _plannedShiftRepo.GetAllAsync();
            var departments = await _departmentRepo.GetAllAsync();
            var shiftTypes = await _shiftTypeRepo.GetAllAsync();
            var shiftStatuses = await _shiftStatusRepo.GetAllAsync();
            var staff = await _staffRepo.GetAllAsync();

            // Filter by date range
            if (filter.FromDate.HasValue)
                shifts = shifts.Where(s => s.ShiftDate >= filter.FromDate.Value).ToList();

            if (filter.ToDate.HasValue)
                shifts = shifts.Where(s => s.ShiftDate <= filter.ToDate.Value).ToList();

            // Filter by department name
            if (!string.IsNullOrWhiteSpace(filter.DepartmentName))
            {
                var deptIds = departments
                    .Where(d => d.DepartmentName.Equals(filter.DepartmentName, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.DepartmentId)
                    .ToHashSet();

                shifts = shifts.Where(s => deptIds.Contains(s.DepartmentId)).ToList();
            }

            // Filter by shift type
            if (!string.IsNullOrWhiteSpace(filter.ShiftTypeName))
            {
                var shiftTypeIds = shiftTypes
                    .Where(st => st.ShiftTypeName.Equals(filter.ShiftTypeName, StringComparison.OrdinalIgnoreCase))
                    .Select(st => st.ShiftTypeId)
                    .ToHashSet();

                shifts = shifts.Where(s => shiftTypeIds.Contains((int)s.ShiftTypeId)).ToList();
            }

            // Filter by shift status
            if (!string.IsNullOrWhiteSpace(filter.ShiftStatusName))
            {
                var statusIds = shiftStatuses
                    .Where(ss => ss.ShiftStatusName.Equals(filter.ShiftStatusName, StringComparison.OrdinalIgnoreCase))
                    .Select(ss => ss.ShiftStatusId)
                    .ToHashSet();

                shifts = shifts.Where(s => statusIds.Contains((int)s.ShiftStatusId)).ToList();
            }

            // Filter by staff name (partial match)
            if (!string.IsNullOrWhiteSpace(filter.StaffName))
            {
                var matchedStaff = staff
                    .Where(s => s.StaffName.Contains(filter.StaffName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var matchedIds = matchedStaff.Select(s => s.StaffId).ToHashSet();
                shifts = shifts.Where(s => s.AssignedStaffId.HasValue && matchedIds.Contains(s.AssignedStaffId.Value)).ToList();
            }

            // Map to DTOs
            var dtos = shifts.Select(shift =>
            {
                var assignedStaff = shift.AssignedStaffId.HasValue
                    ? staff.FirstOrDefault(s => s.StaffId == shift.AssignedStaffId.Value)
                    : null;

                return new PlannedShiftDetailDto
                {
                    PlannedShiftId = shift.PlannedShiftId,
                    ShiftDate = shift.ShiftDate,
                    SlotNumber = shift.SlotNumber,
                    ShiftTypeId =(int) shift.ShiftTypeId,
                    DepartmentId = shift.DepartmentId,
                    ShiftStatusId =(int) shift.ShiftStatusId,
                    AssignedStaffId = shift.AssignedStaffId,

                    ShiftTypeName = shiftTypes.FirstOrDefault(st => st.ShiftTypeId ==(int) shift.ShiftTypeId)?.ShiftTypeName ?? string.Empty,
                    ShiftDeparmentName = departments.FirstOrDefault(d => d.DepartmentId == shift.DepartmentId)?.DepartmentName ?? string.Empty,
                    ShiftStatusName = shiftStatuses.FirstOrDefault(ss => ss.ShiftStatusId ==(int) shift.ShiftStatusId)?.ShiftStatusName ?? string.Empty,
                    AssignedStaffFullName = assignedStaff?.StaffName ?? string.Empty,
                    AssignedStaffDepartmentName = assignedStaff != null
                        ? departments.FirstOrDefault(d => d.DepartmentId == assignedStaff.StaffDepartmentId)?.DepartmentName ?? string.Empty
                        : string.Empty
                };
            }).OrderBy(s => s.ShiftDate).ToList();

            return dtos;
        }

    }

}

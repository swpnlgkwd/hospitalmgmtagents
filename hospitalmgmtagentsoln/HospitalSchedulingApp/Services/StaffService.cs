using HospitalSchedulingApp.Common;
using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dal.Repositories;
using HospitalSchedulingApp.Dtos.Staff;
using HospitalSchedulingApp.Dtos.Staff.Requests;
using HospitalSchedulingApp.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HospitalSchedulingApp.Services
{
    public class StaffService : IStaffService
    {
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Staff> _staffRepo;
        private readonly IRepository<PlannedShift> _shiftRepo;
        private readonly IRepository<LeaveRequests> _leaveRepo;
        private readonly IRepository<NurseAvailability> _availabilityRepo;
        private readonly IShiftTypeService _shiftTypeService;


        public StaffService(
                 IRepository<Department> departmentRepo,
                 IRepository<Role> roleRepo,
                 IRepository<Staff> staffRepo,
                 IRepository<PlannedShift> shiftRepo,
                 IRepository<LeaveRequests> leaveRepo,
                 IRepository<NurseAvailability> availabilityRepo,
                 IRepository<ShiftType> shiftTypeRepo,
                 IShiftTypeService shiftTypeService)
        {
            _departmentRepo = departmentRepo;
            _roleRepo = roleRepo;
            _staffRepo = staffRepo;
            _shiftRepo = shiftRepo;
            _leaveRepo = leaveRepo;
            _availabilityRepo = availabilityRepo;
            _shiftTypeService = shiftTypeService;
        }

        public async Task<List<StaffDto?>> FetchActiveStaffByNamePatternAsync(string namePart)
        {
            if (string.IsNullOrWhiteSpace(namePart))
                return new List<StaffDto?>();

            var staffList = await _staffRepo.GetAllAsync();
            var roles = await _roleRepo.GetAllAsync();
            var departments = await _departmentRepo.GetAllAsync();

            var filtered = staffList
                .Where(s => s.IsActive && s.StaffName.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.StaffName)
                .Take(10)
                .Select(s => new StaffDto
                {
                    StaffId = s.StaffId,
                    StaffName = s.StaffName,
                    RoleId = s.RoleId,
                    RoleName = roles.FirstOrDefault(r => r.RoleId == s.RoleId)?.RoleName ?? string.Empty,
                    StaffDepartmentId = s.StaffDepartmentId,
                    StaffDepartmentName = departments.FirstOrDefault(d => d.DepartmentId == s.StaffDepartmentId)?.DepartmentName ?? string.Empty
                })
                .ToList();


            return filtered;
        }


        public async Task<List<StaffDto?>> SearchAvailableStaffAsync(AvailableStaffFilterDto filter)
        {
            var staffList = await _staffRepo.GetAllAsync();
            var shifts = await _shiftRepo.GetAllAsync();
            var leaveRequests = await _leaveRepo.GetAllAsync();
            var availabilities = await _availabilityRepo.GetAllAsync();
            var departments = await _departmentRepo.GetAllAsync();

            ShiftTypes? resolvedShiftTypeId = null;
            if (!string.IsNullOrWhiteSpace(filter.ShiftType))
            {
                var shiftType = await _shiftTypeService.FetchShiftInfoByShiftName(filter.ShiftType);
                resolvedShiftTypeId = (ShiftTypes)shiftType.ShiftTypeId;
            }

            var departmentMap = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var dateRange = Enumerable.Range(0, (filter.EndDate.DayNumber - filter.StartDate.DayNumber + 1))
                                      .Select(offset => filter.StartDate.AddDays(offset))
                                      .ToList();

            var availableStaff = staffList
                .Where(s => s.IsActive)
                .Where(s => dateRange.All(date =>
                {
                    var shiftDate = date.ToDateTime(TimeOnly.MinValue);

                    // Check availability and leave
                    var isAvailable = !availabilities.Any(a =>
                                        a.StaffId == s.StaffId &&
                                        a.AvailableDate == shiftDate &&
                                        !a.IsAvailable);

                    var isOnLeave = leaveRequests.Any(l =>
                                        l.StaffId == s.StaffId &&
                                        l.LeaveStatusId == LeaveRequestStatuses.Approved &&
                                        shiftDate >= l.LeaveStart &&
                                        shiftDate <= l.LeaveEnd);

                    // Check if already assigned a shift of same type (if ShiftType provided)
                    var isOccupied = resolvedShiftTypeId.HasValue &&
                                     shifts.Any(ps =>
                                         ps.AssignedStaffId == s.StaffId &&
                                         ps.ShiftDate == shiftDate &&
                                         ps.ShiftTypeId == resolvedShiftTypeId.Value);

                    return isAvailable && !isOnLeave && !isOccupied;
                }))
                .Select(s => new
                {
                    Staff = s,
                    DepartmentMatch = string.IsNullOrEmpty(filter.Department) ||
                                      (departmentMap.TryGetValue(s.StaffDepartmentId, out var deptName) &&
                                       deptName.Equals(filter.Department, StringComparison.OrdinalIgnoreCase))
                })
                .OrderByDescending(s => s.DepartmentMatch)
                .ThenBy(s => s.Staff.StaffName)
                .Select(s =>
                {
                    var staff = s.Staff;
                    departmentMap.TryGetValue(staff.StaffDepartmentId, out var deptName);

                    return new StaffDto
                    {
                        StaffId = staff.StaffId,
                        StaffName = staff.StaffName,
                        RoleId = staff.RoleId,
                        RoleName = string.Empty, // Optional if you removed Role usage
                        StaffDepartmentId = staff.StaffDepartmentId,
                        StaffDepartmentName = deptName ?? string.Empty
                    };
                })
                .ToList();

            return availableStaff;
        }

    }
}

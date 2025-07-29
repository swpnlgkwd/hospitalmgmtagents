using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dal.Repositories;
using HospitalSchedulingApp.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalSchedulingApp.Services
{
    /// <summary>
    /// Service for retrieving shift type information.
    /// </summary>
    public class ShiftTypeService : IShiftTypeService
    {
        private readonly IRepository<ShiftType> _shiftTypeRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTypeService"/> class.
        /// </summary>
        /// <param name="shiftTypeRepo">Repository for shift types.</param>
        public ShiftTypeService(IRepository<ShiftType> shiftTypeRepo)
        {
            _shiftTypeRepo = shiftTypeRepo;
        }

        /// <summary>
        /// Fetches a shift type entity by its name (case-insensitive match).
        /// </summary>
        /// <param name="shiftTypePart">The shift type name to match (e.g., "morning").</param>
        /// <returns>The matching <see cref="ShiftType"/> entity, or null if not found.</returns>
        public async Task<ShiftType?> FetchShiftInfoByShiftName(string shiftTypePart)
        {
            if (string.IsNullOrWhiteSpace(shiftTypePart))
                return null;

            var allShiftTypes = await _shiftTypeRepo.GetAllAsync();

            return allShiftTypes
                .FirstOrDefault(s =>
                    s.ShiftTypeName.Contains(shiftTypePart, StringComparison.OrdinalIgnoreCase));
        }

    }
}

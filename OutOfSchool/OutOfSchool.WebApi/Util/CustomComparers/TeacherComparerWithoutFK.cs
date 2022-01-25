using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Util.CustomComparers
{
    public class TeacherComparerWithoutFK : IEqualityComparer<Teacher>
    {
        public bool Equals(Teacher x, Teacher y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Id == y.Id && x.FirstName == y.FirstName && x.LastName == y.LastName && x.MiddleName == y.MiddleName && x.DateOfBirth == y.DateOfBirth && x.Description == y.Description && x.AvatarImageId == y.AvatarImageId;
        }

        public int GetHashCode(Teacher obj)
        {
            return HashCode.Combine(obj.Id, obj.FirstName, obj.LastName, obj.MiddleName, obj.DateOfBirth, obj.Description, obj.AvatarImageId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;

namespace УчётУспеваемостиООШ.Services
{
    public static class CacheService
    {
        private static List<Class>? _cachedClasses;
        private static List<Teacher>? _cachedTeachers;
        private static readonly object _classLock = new object();
        private static readonly object _teacherLock = new object();

        public static List<Class> GetClasses(SchoolContext context)
        {
            lock (_classLock)
            {
                if (_cachedClasses == null)
                {
                    _cachedClasses = context.Classes.ToList();
                    Logger.Info("Classes loaded into cache", "Cache");
                }
                return _cachedClasses;
            }
        }

        public static List<Teacher> GetTeachers(SchoolContext context)
        {
            lock (_teacherLock)
            {
                if (_cachedTeachers == null)
                {
                    _cachedTeachers = context.Teachers.ToList();
                    Logger.Info("Teachers loaded into cache", "Cache");
                }
                return _cachedTeachers;
            }
        }

        public static void InvalidateClasses()
        {
            lock (_classLock)
            {
                _cachedClasses = null;
                Logger.Info("Classes cache invalidated", "Cache");
            }
        }

        public static void InvalidateTeachers()
        {
            lock (_teacherLock)
            {
                _cachedTeachers = null;
                Logger.Info("Teachers cache invalidated", "Cache");
            }
        }

        public static void InvalidateAll()
        {
            InvalidateClasses();
            InvalidateTeachers();
            Logger.Info("All caches invalidated", "Cache");
        }
    }
}
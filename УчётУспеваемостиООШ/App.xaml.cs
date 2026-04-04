using System;
using System.Linq;
using System.Windows;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public partial class App : Application
    {
        private DateTime _startTime;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _startTime = DateTime.Now;

            Logger.Info("Application started", "App");

            try
            {
                using (var context = new SchoolContext())
                {
                    //context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    Logger.Metrics("DB connected", "success");

                    Logger.Metrics("Records in Students", context.Students.Count().ToString());
                    Logger.Metrics("Records in Teachers", context.Teachers.Count().ToString());
                    Logger.Metrics("Records in Subjects", context.Subjects.Count().ToString());
                    Logger.Metrics("Records in Grades", context.Grades.Count().ToString());
                    Logger.Metrics("Records in Attendance", context.Attendances.Count().ToString());
                    Logger.Metrics("Records in Classes", context.Classes.Count().ToString());
                    Logger.Metrics("Records in Users", context.Users.Count().ToString());

                    // В методе OnStartup после EnsureCreated()
                    if (context.Users.Any())
                    {
                        var users = context.Users.ToList();
                        bool needUpdate = false;

                        foreach (var user in users)
                        {
                            if (string.IsNullOrEmpty(user.Salt) && !string.IsNullOrEmpty(user.PasswordHash))
                            {
                                var (hash, salt) = PasswordHasher.MigratePlainPassword(user.PasswordHash);
                                user.PasswordHash = hash;
                                user.Salt = salt;
                                needUpdate = true;
                            }
                        }

                        if (needUpdate)
                        {
                            context.SaveChanges();
                            Logger.Info("Existing passwords migrated to hashed format", "Security");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Database initialization error", ex, "DB");
            }

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true && loginWindow.CurrentUser != null)
            {
                Logger.Info($"User {loginWindow.CurrentUser.Username} logged in", "Security");
                Logger.Metrics("User logged in", loginWindow.CurrentUser.Username);

                var menuWindow = new Menu(loginWindow.CurrentUser);
                MainWindow = menuWindow;
                menuWindow.Show();

                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                Logger.Info("Login cancelled", "Security");
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var uptime = DateTime.Now - _startTime;

            // ПРАВИЛЬНЫЙ ФОРМАТ
            Logger.Metrics("Application uptime", $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s");

            Logger.Info("Application closed", "App");
            base.OnExit(e);
        }
    }
}
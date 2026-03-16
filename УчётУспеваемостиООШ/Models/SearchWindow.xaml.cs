using System;
using System.Linq;
using System.Windows;
using УчётУспеваемостиООШ.Models;

namespace УчётУспеваемостиООШ
{
    public partial class SearchWindow : Window
    {
        public SearchCriteria Criteria { get; private set; }

        public SearchWindow()
        {
            InitializeComponent();
            LoadClasses();
        }

        private void LoadClasses()
        {
            using (var context = new Data.SchoolContext())
            {
                var classes = context.Classes
                    .Select(c => c.ClassName)
                    .OrderBy(c => c)
                    .ToList();

                cmbClass.ItemsSource = classes;
                cmbClass.SelectedIndex = -1;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Criteria = new SearchCriteria
            {
                LastName = string.IsNullOrWhiteSpace(txtLastName.Text) ? null : txtLastName.Text.Trim(),
                FirstName = string.IsNullOrWhiteSpace(txtFirstName.Text) ? null : txtFirstName.Text.Trim(),
                ClassName = cmbClass.SelectedItem?.ToString(),
                IsActive = true
            };

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
using System.Windows;
using System.Windows.Threading;

namespace УчётУспеваемостиООШ
{
    public partial class ProgressWindow : Window
    {
        private string _baseTitle;

        public ProgressWindow(string title)
        {
            InitializeComponent();
            _baseTitle = title;
            txtStatus.Text = title;
        }

        public void UpdateProgress(int percent)
        {

            Dispatcher.Invoke(() =>
            {
                progressBar.Value = percent;
                txtStatus.Text = $"{_baseTitle}: {percent}%";

                this.UpdateLayout();
            });
        }

        public void SetMaxValue(int max)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Maximum = max;
            });
        }

        public void IncrementProgress(int step)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Value += step;
                int percent = (int)(progressBar.Value / progressBar.Maximum * 100);
                txtStatus.Text = $"{_baseTitle}: {percent}%";
                this.UpdateLayout();
            });
        }

        public void SetProgress(int current, int total)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Maximum = total;
                progressBar.Value = current;
                int percent = (int)((double)current / total * 100);
                txtStatus.Text = $"{_baseTitle}: {percent}%";
                this.UpdateLayout();
            });
        }

        public void CloseWindow()
        {
            Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }
    }
}
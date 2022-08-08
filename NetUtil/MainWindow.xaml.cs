using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NetUtil.Checker;
using System.Text.RegularExpressions;


namespace NetUtil
{
    /// <summary>
    /// Logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region pirvate members

        /// <summary>
        /// Port checker
        /// </summary>
        CheckPort _checkPort = null;

        #endregion

        #region C'tor

        /// <summary>
        /// C'tor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region HMI events

        /// <summary>
        /// Combo box port TIA selection
        /// </summary>
        private void cmbBx_portTiaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.cmbBxPortTiaSelection.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            var si = selectedItem.Content;
            if (si == null) return;
            this.txtBx_CheckPort.Text = GetPortFromComboBox(si.ToString());
        }

        /// <summary>
        /// Combo box port Zenon selection
        /// </summary>
        private void cmbBx_portZenonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.cmbBxPortZenonSelection.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            var si = selectedItem.Content;
            if (si == null) return;
            this.txtBx_CheckPort.Text = GetPortFromComboBox(si.ToString());
        }

        /// <summary>
        /// Combo box port Common selection
        /// </summary>
        private void cmbBx_portCommonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.cmbBxPortCommonSelection.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            var si = selectedItem.Content;
            if (si == null) return;
            this.txtBx_CheckPort.Text = GetPortFromComboBox(si.ToString());
        }

        /// <summary>
        /// Button port checker start
        /// </summary>
        private void btnCheckPortPing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = this.txtBx_CheckPortIP.Text;
                string port = this.txtBx_CheckPort.Text;
                this.txtBlock_CheckPortResult.Text = "Checking ...";

                if (_checkPort == null)
                {
                    _checkPort = new CheckPort();
                    _checkPort.NewCheckResult += HandlePortCheckerResultEvent;
                }

                if (!_checkPort.IsRunning) _checkPort.StartCheck(ip, Int32.Parse(port));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }   
        }

        /// <summary>
        /// Button cancle
        /// </summary>
        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_checkPort != null)
                {
                    if(_checkPort.IsRunning)
                    {
                        _checkPort.Cancle();
                        this.txtBlock_CheckPortResult.Inlines.Add("Cancled");
                    }                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Textbox ort previe text input event
        /// </summary>
        // private void txtBx_CheckPortPrevTextInput(object sender, DataObjectPastingEventArgs e)
        private void txtBx_CheckPortPrevTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        #endregion

        #region private methods

        /// <summary>
        /// Port checker result event
        /// </summary>
        string GetPortFromComboBox(string cmb)
        {
            try
            {
                if (cmb == null) return "0";

                var list = cmb.ToString().Split(':');
                if (list.Length <= 1) return "0";

                string s = list.ElementAt(1).Trim();
                return list.ElementAt(1).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "0";
            }
        }
        #endregion

        #region Result events

        /// <summary>
        /// Port checker result event
        /// </summary>
        void HandlePortCheckerResultEvent(object sender, CheckPortEventArgs e)
        {
            Console.WriteLine(e.Host, e.Elapsed);
            if (e.IsFirstResult) this.txtBlock_CheckPortResult.Text = "";

            Run runHost = new Run(e.Host);
            runHost.FontWeight = FontWeights.Bold;
            this.txtBlock_CheckPortResult.Inlines.Add(runHost);

            this.txtBlock_CheckPortResult.Inlines.Add(" port:");

            Run runPort = new Run(e.Port.ToString());
            runPort.FontWeight = FontWeights.Bold;
            this.txtBlock_CheckPortResult.Inlines.Add(runPort);

            this.txtBlock_CheckPortResult.Inlines.Add(" ");

            if (e.Connected)
            {
                Run run = new Run("OPEN");
                run.Foreground = Brushes.Green;
                run.FontWeight = FontWeights.Bold;
                this.txtBlock_CheckPortResult.Inlines.Add(run);
                this.txtBlock_CheckPortResult.Inlines.Add(" ");
            }
            else
            {
                Run run = new Run("CLOSED");
                run.Foreground = Brushes.Red;
                run.FontWeight = FontWeights.Bold;
                this.txtBlock_CheckPortResult.Inlines.Add(run);
                this.txtBlock_CheckPortResult.Inlines.Add(" ");
            }

            int ms = 0;
            try
            {
                ms = Convert.ToInt32(e.Elapsed.TotalMilliseconds);
            }
            catch (Exception)
            {
            }

            this.txtBlock_CheckPortResult.Inlines.Add(ms.ToString());
            this.txtBlock_CheckPortResult.Inlines.Add("ms");
            this.txtBlock_CheckPortResult.Inlines.Add("\r\n");
        }

        #endregion
    }
}

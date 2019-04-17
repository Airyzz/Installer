using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.IO;
using IWshRuntimeLibrary;
using System.Diagnostics;

namespace Installer
{
    public partial class MainWindow : Window
    {

        MainMenu mainMenu = new MainMenu();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExitButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void UsernameTextbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
           Task.Run(() => LoginAsync());
        }


        private async Task LoginAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        string result = webClient.DownloadString("https://pastebin.com/raw/V8ZjRVfw");
                    }
                }
                catch
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        WelcomeText.Text = "Error:";
                        DescText.Text = "Unable to download data required";
                        NameText.Text = ":(";
                        LinearGradientBrush ErrorGb = new LinearGradientBrush();
                        ErrorGb.StartPoint = new Point(1, 0);
                        ErrorGb.EndPoint = new Point(1, 1);
                        // Create and add Gradient stops

                        GradientStop blueGS = new GradientStop();
                        blueGS.Color = Color.FromArgb(122, 100, 20, 20);
                        blueGS.Offset = 0.0;
                        ErrorGb.GradientStops.Add(blueGS);                

                        GradientStop orangeGS = new GradientStop();
                        orangeGS.Color = Color.FromArgb(204, 147, 18, 18);
                        orangeGS.Offset = 1;
                        ErrorGb.GradientStops.Add(orangeGS);

                        GradientRectangle.Fill = ErrorGb;
                    }));
                    return;
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Name = UsernameTextbox.Text;
                    DoubleAnimation animation = new DoubleAnimation()
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromSeconds(0.1))
                    };
                    this.BeginAnimation(Window.OpacityProperty, animation);
                    mainMenu.Left = this.PointToScreen(new Point(0, 0)).X;
                    mainMenu.Top = this.PointToScreen(new Point(0, 0)).Y;
                    mainMenu.Show();
                }));
            });
        }

        private void UpdateButton_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcut = startUpFolderPath + "\\" + "Team A Updater" + ".lnk";
            Console.WriteLine("Shortcut: " + shortcut);
            FileInfo fi = new FileInfo(shortcut);
            toggleStartup.IsChecked = fi.Exists;
        }

        private void ToggleStartup_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)toggleStartup.IsChecked)
            {
                WshShell wshShell = new WshShell();

                IWshRuntimeLibrary.IWshShortcut shortcut;
                string startUpFolderPath =
                  Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                // Create the shortcut
                shortcut =
                  (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                    startUpFolderPath + "\\" +
                    "Team A Updater" + ".lnk");

                shortcut.TargetPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Updater.exe";
                shortcut.Arguments = "-hidden";
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                shortcut.Description = "Launch My Application";
                // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
                shortcut.Save();
            }
            else
            {
                string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcut = startUpFolderPath + "\\" + "Team A Updater" + ".lnk";
                FileInfo fi = new FileInfo(shortcut);
                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
        }

        private void UpdateButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Checking for Updates");
            Process.Start(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Updater.exe");
        }
    }
}

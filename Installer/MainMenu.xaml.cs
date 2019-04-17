using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Installer
{
    /// <summary>
    /// Data to bind to each button
    /// </summary>
    /// 
    public class data
    {
        public string version;
        public string url;
        public string name;
        public string password;
        public string toolname;

        public data(string _version, string _url, string _name, string _password, string _toolname)
        {
            version = _version;
            url = _url;
            name = _name;
            password = _password;
            toolname = _toolname;
        }
    }


    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    /// 
    public partial class MainMenu : Window
    {

        string PastebinURL = @"https://pastebin.com/raw/V8ZjRVfw";

        public enum webpageindexes
        {
            Name, ImageUrl, FileUrl, VersionNumber, password
        }

        public Dictionary<string, string> ToolVersions = new Dictionary<string, string> { };

        public MainMenu()
        {
            InitializeComponent();

            //Check if there is an update for the installer
            string InstallerVersionPB = @"https://pastebin.com/raw/RUGwsdwd";
            string version = "1.0.1";
            var ver = string.Empty;
            using (var webClient = new System.Net.WebClient())
            {
                ver = webClient.DownloadString(InstallerVersionPB);
            }
            if(ver != version)
            {
                MessageBoxResult result = MessageBox.Show("There is an update available, would you like to download it?", "Update", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("http://airyz.xyz");
                }
            }


            //Attempt to read currently installed tool versions
            try
            {
                ToolVersions = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("VersionLog"));
            }
            catch
            {

            }

            try
            {
                ProgressBar.Opacity = 0;
                ProgressText.Opacity = 0;

                //Get currently available tools
                var result = string.Empty;
                using (var webClient = new System.Net.WebClient())
                {
                    result = webClient.DownloadString(PastebinURL);
                }

                string[] entries = result.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                //Resize the Stack Panel to fit each button
                AppStackPanel.Height = ((entries.Length) * BaseButton.Height) - (20 * entries.Length);

                //Add a button for each tool
                foreach (string s in entries)
                {
                    Console.WriteLine(s);
                    string[] Entry = s.Split('|');
                    Console.WriteLine(Entry[(int)webpageindexes.Name]);
                    Button newButton = new Button { DataContext = BaseButton.DataContext };
                    newButton.Height = BaseButton.Height;
                    newButton.Content = Entry[(int)webpageindexes.Name];

                    //Get Image
                    var bitmapImage = new BitmapImage();
                    ImageBrush uniformBrush = new ImageBrush();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(Entry[(int)webpageindexes.ImageUrl]);
                    bitmapImage.EndInit();
                    uniformBrush.ImageSource = bitmapImage;
                    uniformBrush.Stretch = Stretch.UniformToFill;
                    newButton.Background = uniformBrush;

                    newButton.BorderBrush = null;
                    newButton.Visibility = Visibility.Visible;
                    newButton.Margin = new Thickness(-10, -10, -10, -10);

                    newButton.Name = Entry[(int)webpageindexes.Name].Replace(' ', '_') + "_" + Entry[(int)webpageindexes.VersionNumber].Replace('.', '_');

                    //Bind Data to the button
                    newButton.DataContext = new data(Entry[(int)webpageindexes.VersionNumber], Entry[(int)webpageindexes.FileUrl], Entry[(int)webpageindexes.Name], Entry[(int)webpageindexes.password], Entry[(int)webpageindexes.Name]);
                    newButton.FontSize = 32;
                    newButton.ToolTip = Entry[(int)webpageindexes.FileUrl];
                    newButton.Click += BaseButton_Click;
                    newButton.MouseEnter += BaseButton_Hover;
                    newButton.MouseLeave += BaseButton_HoverOff;
                    AppStackPanel.Children.Add(newButton);
                }
            }
            catch
            {

            }

        }

        string _url;
        string _filename;
        string _password;
        string _version;
        string _dest;
        string _toolname;

        bool downloading = false;

        private void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            //Initialise Download
            if (!downloading)
            {
                Directory.CreateDirectory($@"Downloads\{(string)((Button)sender).Name}");
                _url = ((data)((Button)sender).DataContext).url;
                _filename = (string)((Button)sender).Name;
                _password = ((data)((Button)sender).DataContext).password;
                _version = ((data)((Button)sender).DataContext).version;
                _toolname = ((data)((Button)sender).DataContext).toolname;
                downloading = true;
                startDownload(_url, $@"Downloads\{(string)((Button)sender).Name}\{_filename}.zip");
            }
            else
            {
                MessageBox.Show("Please download only one tool at a time"); 
            }
        }

        private void BaseButton_Hover(object sender, RoutedEventArgs e)
        {
            //((Button)sender).Content = "Download";
        }

        private void BaseButton_HoverOff(object sender, RoutedEventArgs e)
        {
            //((Button)sender).Content = ((Button)sender).Name.Replace('_', ' ');
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Rectangle_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void startDownload(string url, string destination)
        {
            _dest = destination;
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, 500);

            // Create a DoubleAnimation to fade the not selected option control
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0.0,
                To = 1,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, a) => ProgressBar.Opacity = 1;
            animation.Completed += (s, a) => ProgressText.Opacity = 1;

            ProgressBar.BeginAnimation(UIElement.OpacityProperty, animation);
            ProgressText.BeginAnimation(UIElement.OpacityProperty, animation);

            //Check for download progress
            Thread thread = new Thread(() =>
            {
                try
                {
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(url), destination);
                }
                catch(Exception e)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ProgressText.Content = $"Something went wrong: {e.GetType().ToString()} {e.Message.ToString()}";
                        SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(100, 207, 69, 69));
                        ProgressBar.Foreground = sb;
                        ProgressBar.BorderBrush = sb;
                    }));
                }
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //Update the progress bar with download information
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(100, 179, 157, 219));
                ProgressBar.Foreground = sb;
                ProgressBar.BorderBrush = sb;
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                ProgressText.Content = $"Downloaded {(e.BytesReceived / 1000.0f)} kb of {e.TotalBytesToReceive / 1000.0f}kb\t{Math.Round(percentage, 2)}%";
                ProgressBar.Value = int.Parse(Math.Truncate(percentage * 100).ToString());
            }));
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Install file from download
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Error == null)
                {
                    ProgressText.Content = "Download Completed... Extracting...";
                    try
                    {
                        //Set Up extractor
                        using (ZipFile archive = new ZipFile(_dest))
                        {
                            if (_password != "none")
                            {
                                archive.Password = _password;
                            }
                            archive.Encryption = EncryptionAlgorithm.PkzipWeak; // the default: you might need to select the proper value here

                            //Delete any pre existing files (only .exe and .dll so that caches and presets are not deleted)
                            DirectoryInfo di = new DirectoryInfo($@"Installations\{_toolname}\");
                            if(!di.Exists)
                            {
                                di.Create();
                            }
                            FileInfo[] files = di.GetFiles("*.exe", SearchOption.AllDirectories)
                                                 .Where(p => p.Extension == ".exe").ToArray();
                            foreach (FileInfo file in files)
                                try
                                {
                                    file.Attributes = FileAttributes.Normal;
                                    File.Delete(file.FullName);
                                }
                                catch { }

                            di = new DirectoryInfo($@"Installations\{_toolname}\");
                            files = di.GetFiles("*.dll", SearchOption.AllDirectories).Where(p => p.Extension == ".dll").ToArray();
                            foreach (FileInfo file in files)
                                try
                                {
                                    file.Attributes = FileAttributes.Normal;
                                    File.Delete(file.FullName);
                                }
                                catch { }

                            //Extract files to install folder
                            archive.ExtractAll($@"Installations\{_toolname}\", ExtractExistingFileAction.OverwriteSilently);
                            archive.StatusMessageTextWriter = Console.Out;
                            ProgressText.Content = "Done!";

                            //Open Install Folder
                            Process.Start($@"Installations\{_toolname}\");
                            if(ToolVersions.ContainsKey(_toolname))
                            {
                                ToolVersions[_toolname] = _version;
                            }
                            else
                            {
                                ToolVersions.Add(_toolname, _version);
                            }

                            File.WriteAllText("VersionLog", Newtonsoft.Json.JsonConvert.SerializeObject(ToolVersions));
                        }
                    }
                    //Error Handling
                    catch (Ionic.Zip.BadPasswordException)
                    {
                        ProgressText.Content = "Error: Invalid Zip Password...";
                        SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(100, 207, 69, 69));
                        ProgressBar.Foreground = sb;
                        ProgressBar.BorderBrush = sb;
                    }
                    catch (Exception error)
                    {
                        ProgressText.Content = $"Something went wrong: {error.GetType().ToString()} {error.Message.ToString()}";
                        SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(100, 207, 69, 69));
                        ProgressBar.Foreground = sb;
                        ProgressBar.BorderBrush = sb;
                    }

                    //UI Animation
                    Storyboard storyboard = new Storyboard();
                    TimeSpan duration = new TimeSpan(0, 0, 10);

                    DoubleAnimation animation = new DoubleAnimation
                    {

                        From = 1,
                        To = 0.0,
                        Duration = new Duration(duration),
                        FillBehavior = FillBehavior.Stop
                    };

                    animation.Completed += (s, a) => ProgressBar.Opacity = 0;
                    animation.Completed += (s, a) => ProgressText.Opacity = 0;

                    ProgressBar.BeginAnimation(UIElement.OpacityProperty, animation);
                    ProgressText.BeginAnimation(UIElement.OpacityProperty, animation);
                }
                else
                {
                    ProgressText.Content = $"Something went wrong: {e.GetType().ToString()} {e.Error.Message.ToString()}";
                    SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(100, 207, 69, 69));
                    ProgressBar.Foreground = sb;
                    ProgressBar.BorderBrush = sb;
                }
                downloading = false;
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }

    public static class BlurElementExtension
    {
        /// <summary>
        /// Turning blur on
        /// </summary>
        /// <param name="element">bluring element</param>
        /// <param name="blurRadius">blur radius</param>
        /// <param name="duration">blur animation duration</param>
        /// <param name="beginTime">blur animation delay</param>
        public static void BlurApply(this UIElement element,
            double blurRadius, TimeSpan duration, TimeSpan beginTime)
        {
            BlurEffect blur = new BlurEffect() { Radius = 0 };
            DoubleAnimation blurEnable = new DoubleAnimation(0, blurRadius, duration)
            { BeginTime = beginTime };
            element.Effect = blur;
            blur.BeginAnimation(BlurEffect.RadiusProperty, blurEnable);
        }
        /// <summary>
        /// Turning blur off
        /// </summary>
        /// <param name="element">bluring element</param>
        /// <param name="duration">blur animation duration</param>
        /// <param name="beginTime">blur animation delay</param>
        public static void BlurDisable(this UIElement element, TimeSpan duration, TimeSpan beginTime)
        {
            BlurEffect blur = element.Effect as BlurEffect;
            if (blur == null || blur.Radius == 0)
            {
                return;
            }
            DoubleAnimation blurDisable = new DoubleAnimation(blur.Radius, 0, duration) { BeginTime = beginTime };
            blur.BeginAnimation(BlurEffect.RadiusProperty, blurDisable);
        }
    }
}

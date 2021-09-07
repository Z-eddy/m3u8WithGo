using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

namespace M3U8Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 调用dll,显示控制台
#if DEBUG
        [DllImport("Kernel32.dll")]
        public static extern bool AllocConsole();
#endif

        /***********Property***********/
        private HashSet<string> curUrls = new HashSet<string>();
        private SynchronizationContext mainThreadSynContext;

        /***********Function***********/
        public MainWindow()
        {
            InitializeComponent();

            // 记录主线程
            mainThreadSynContext = SynchronizationContext.Current;

#if DEBUG
            // 显示控制台
            AllocConsole();
#endif
        }

        private void on_deleteListItem(object arg)
        {
            var item = (ZrListItem)arg;
            curUrls.Remove(item.Filename);
            curStatus.Text = item.Filename + "完成下载";
            // 移除此项
            urlList.Items.Remove(arg);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 没有填写url/filename则返回
            if (urlText.Text.Length == 0 || filenameText.Text.Length == 0)
            {
                string msg = "文件名/url地址为空";
                Console.WriteLine(msg);
                curStatus.Text = msg;
                return;
            }

            // 文件名不重复
            if (!curUrls.Add(filenameText.Text))
            {
                string msg = "文件名已存在";
                Console.WriteLine(msg);
                curStatus.Text = msg;
                return;
            }

            /********添加项*********/
            ZrListItem item = new ZrListItem(filenameText.Text, urlText.Text);
            urlList.Items.Add(item);
            // 添加完成后移除内容
            urlText.Clear();
            filenameText.Clear();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var idx = urlList.SelectedIndex;
            // 未选中时返回
            if (idx == -1)
            {
                return;
            }
            var item = urlList.SelectedItem;
            urlList.Items.Remove(item);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in urlList.Items)
            {
                // 检查参数是否正常
                var isOk = item is ZrListItem;
                if (!isOk)
                {
                    var msg = "object can't convert to ZrListItem";
                    Console.WriteLine(msg);
                    curStatus.Text = msg;
                    return;
                }

                // 启动线程池,开始下载任务
                ThreadPool.QueueUserWorkItem(downM3U8, item);
            }
        }

        private void downM3U8(object arg)
        {
            var item = (ZrListItem)arg;

            Process goApp = new Process();
            // 调用go的程序名
            goApp.StartInfo.FileName = "m3u8GoAssist.exe";
            // 无窗口
            goApp.StartInfo.CreateNoWindow = true;
            // 不启用shell
            goApp.StartInfo.UseShellExecute = false;
            // 重定向
            goApp.StartInfo.RedirectStandardOutput = true;
            goApp.StartInfo.RedirectStandardError = true;
            goApp.StartInfo.RedirectStandardInput = true;

            // 参数输入
            goApp.StartInfo.Arguments = " -u=\"" + item.Url + "\" -n=" + item.Filename + " -c=256";

            // 启动
            goApp.Start();

            // 等待完成
            goApp.StandardOutput.ReadToEnd();
            goApp.WaitForExit();
            goApp.Close();

            mainThreadSynContext.Post(new SendOrPostCallback(on_deleteListItem), arg);
        }
    }
}

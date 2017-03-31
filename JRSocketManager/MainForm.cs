using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JRSocketManager
{
    public partial class MainForm : Form
    {
        public delegate void LogDelegate(string log);
        public BackgroundWorker bgWorkerListener;
        public BackgroundWorker bgWorkerStartVideo;
        public BackgroundWorker bgWorkerStopVideo;
        public BackgroundWorker bgWorkerService;

        public string message;
        public string servicePath;//带后缀
        public string serviceName;//不带后缀
        public string ip;
        public int port;
        public string courseId;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            servicePath = ConfigurationManager.AppSettings["servicePath"];
            serviceName = ConfigurationManager.AppSettings["serviceName"];
            ip = ConfigurationManager.AppSettings["ipAddress"];
            courseId = ConfigurationManager.AppSettings["courseId"];
            int.TryParse(ConfigurationManager.AppSettings["port"], out port);


            this.labelWSName.Text = serviceName;
            this.textBoxIP.Text = ip;
            this.textBoxPort.Text = port.ToString();
            this.textBoxCourseId.Text = courseId;

            ReloadMenu();
        }

        private void buttonIMStart_Click(object sender, EventArgs e)
        {
            bgWorkerListener = new BackgroundWorker();
            bgWorkerListener.WorkerReportsProgress = true;
            bgWorkerListener.WorkerSupportsCancellation = true;
            bgWorkerListener.DoWork += BgWorkerListener_DoWork;
            bgWorkerListener.RunWorkerAsync();
        }

        private void buttonIMStop_Click(object sender, EventArgs e)
        {

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            bgWorkerStartVideo = new BackgroundWorker();
            bgWorkerStartVideo.WorkerReportsProgress = true;
            bgWorkerStartVideo.WorkerSupportsCancellation = true;
            bgWorkerStartVideo.DoWork += BgWorkerStartVideo_DoWork;
            bgWorkerStartVideo.RunWorkerAsync();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            bgWorkerStopVideo = new BackgroundWorker();
            bgWorkerStopVideo.WorkerReportsProgress = true;
            bgWorkerStopVideo.WorkerSupportsCancellation = true;
            bgWorkerStopVideo.DoWork += BgWorkerStopVideo_DoWork; ;
            bgWorkerStopVideo.RunWorkerAsync();
        }

        private void BgWorkerListener_DoWork(object sender, DoWorkEventArgs e)
        {
            JRSocketListener.socketInfo += JRSocketListener_socketInfo;
            JRSocketListener.socketOp += JRSocketListener_socketOp;
            JRSocketListener.StartListening();
        }

        private void BgWorkerStartVideo_DoWork(object sender, DoWorkEventArgs e)
        {
            JRSocketClient.socketInfo += JRSocketListener_socketInfo;
            JRSocketClient.StartVideo(courseId);
        }

        private void BgWorkerStopVideo_DoWork(object sender, DoWorkEventArgs e)
        {
            JRSocketClient.socketInfo += JRSocketListener_socketInfo;
            JRSocketClient.StopVideo(courseId);
        }

        private void JRSocketListener_socketInfo(string info)
        {
            logInfo(info);
        }

        private void JRSocketListener_socketOp(JRCommand cmd)
        {
            switch (cmd.key)
            {
                case JROP.start:
                    FFMPEGExtendLib.startCatchVideo("10086");
                    break;
                case JROP.stop:
                    FFMPEGExtendLib.stopCatchVideo("10086");
                    break;

                default:
                    break;
            }
        }

        private void logInfo(string log)
        {
            if (this.textBoxLog.InvokeRequired)
            {
                LogDelegate logDelegate = new LogDelegate(logInfo);
                this.Invoke(logDelegate, new object[] { log });
            }
            else
            {
                this.textBoxLog.Text =  log + "\r\n" + this.textBoxLog.Text;
            }
        }




        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void 开始服务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelWSStatus.Text = "";
            message = "";
            if (ServiceHelper.Start_Service(serviceName, out message))
            {
                停止服务ToolStripMenuItem.Enabled = true;
                开始服务ToolStripMenuItem.Enabled = false;
                labelWSStatus.Text = "服务执行中...";
            }
            else
            {
                labelWSStatus.Text = "启动服务失败";
            }
        }

        private void 停止服务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelWSStatus.Text = "";
            message = "";
            if (ServiceHelper.Stop_Service(serviceName, out message))
            {
                labelWSStatus.Text = "服务未启动";
                开始服务ToolStripMenuItem.Enabled = true;
                停止服务ToolStripMenuItem.Enabled = false;
            }
            else
            {
                labelWSStatus.Text = "停止服务失败";
            }
        }

        private void 安装ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelWSStatus.Text = "";
            message = "";
            if (ServiceHelper.Install_Service(servicePath, serviceName, out message))
            {
                labelWSStatus.Text = "安装服务成功";
                卸载ToolStripMenuItem.Enabled = true;
                开始服务ToolStripMenuItem.Enabled = true;
                停止服务ToolStripMenuItem.Enabled = false;
                安装ToolStripMenuItem.Enabled = false;
            }
            else
            {
                labelWSStatus.Text = "安装服务失败";
                MessageBox.Show(message);
            }
        }

        private void 卸载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelWSStatus.Text = "";
            message = "";
            if (ServiceHelper.Uninstall_Service(servicePath, serviceName, out message))
            {
                labelWSStatus.Text = "卸载服务成功";
                安装ToolStripMenuItem.Enabled = true;
                开始服务ToolStripMenuItem.Enabled = false;
                停止服务ToolStripMenuItem.Enabled = false;
                卸载ToolStripMenuItem.Enabled = false;
            }
            else
            {
                labelWSStatus.Text = "卸载服务失败";
                MessageBox.Show(message);
            }
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadMenu();
        }

        private void ReloadMenu()
        {
            if (ServiceHelper.ServiceIsExisted(serviceName))
            {
                this.安装ToolStripMenuItem.Enabled = false;
                if (ServiceHelper.ServiceIsBusy(serviceName))
                {
                    开始服务ToolStripMenuItem.Enabled = false;
                    this.labelWSStatus.Text = "服务执行中...";
                }
                else
                {
                    this.停止服务ToolStripMenuItem.Enabled = false;
                    this.labelWSStatus.Text = "服务未启动";
                }
            }
            else
            {
                this.卸载ToolStripMenuItem.Enabled = false;
                this.开始服务ToolStripMenuItem.Enabled = false;
                this.停止服务ToolStripMenuItem.Enabled = false;
                this.labelWSStatus.Text = "未安装服务";
            }
        }

        private void textBoxIP_TextChanged(object sender, EventArgs e)
        {
            ip = this.textBoxIP.Text.Trim();
            JRSocketClient.ConfigPara(ip, port);
        }

        private void textBoxPort_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(this.textBoxPort.Text.Trim(), out port);
            JRSocketClient.ConfigPara(ip, port);
        }

        private void textBoxCourseId_TextChanged(object sender, EventArgs e)
        {
            courseId = this.textBoxCourseId.Text.Trim();
        }
    }
}

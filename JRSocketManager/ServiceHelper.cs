using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;

namespace JRSocketManager
{
    class ServiceHelper
    {
        /// <summary>
        /// 判断服务是否存在
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ServiceIsExisted(string ServiceName)
        {
            bool _bool = false;
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == ServiceName)
                {
                    _bool = true;
                    if (s.Status == ServiceControllerStatus.Running)
                    {

                    }
                }
            }
            return _bool;
        }

        /// <summary>
        /// 判断服务是否启动
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public static bool ServiceIsBusy(string ServiceName)
        {
            bool _bool = false;
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == ServiceName && s.Status == ServiceControllerStatus.Running)
                {
                    _bool = true;
                }
            }
            return _bool;
        }

        /// <summary>
        /// 开始服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool Start_Service(string serviceName, out string message)
        {
            message = "";
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == serviceName && s.Status == ServiceControllerStatus.Stopped)
                {
                    s.Start();
                    message = "启动服务成功";
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool Stop_Service(string serviceName, out string message)
        {
            message = "";
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == serviceName && s.Status == ServiceControllerStatus.Running)
                {
                    s.Stop();
                    message = "停止服务成功";
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 安装windows service
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public static bool Install_Service(string FileServiceName, string ServiceName, out string message)
        {
            try
            {
                if (!ServiceIsExisted(ServiceName))
                {
                    string[] cmdline = { };
                    TransactedInstaller transactedInstaller = new TransactedInstaller();
                    AssemblyInstaller assemblyInstaller = new AssemblyInstaller(FileServiceName, cmdline);
                    transactedInstaller.Installers.Add(assemblyInstaller);
                    transactedInstaller.Install(new Hashtable());
                    message = "服务安装成功";
                    return true;
                }
                else
                {
                    message = "服务已存在";
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 卸载windows service
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public static bool Uninstall_Service(string FileServiceName, string ServiceName, out string message)
        {
            try
            {
                if (ServiceIsExisted(ServiceName))
                {
                    string[] cmdline = { };
                    string serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;

                    TransactedInstaller transactedInstaller = new TransactedInstaller();
                    AssemblyInstaller assemblyInstaller = new AssemblyInstaller(FileServiceName, cmdline);
                    transactedInstaller.Installers.Add(assemblyInstaller);
                    transactedInstaller.Uninstall(null);
                    message = "服务卸载成功";
                    return true;
                }
                else
                {
                    message = "服务不存在";
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return false;
            }
        }
    }
}

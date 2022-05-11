using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceDemo
{
    class LogInfoWorker : BackgroundService
    {
        /// <summary>
        /// 日志字段
        /// </summary>
        private readonly ILogger<LogInfoWorker> logger;

        /// <summary>
        /// 应用程序生命周期字段
        /// </summary>
        private readonly IHostApplicationLifetime hostApplicationLifetime;

        /// <summary>
        /// 日志输出目录
        /// </summary>
        private readonly string LogPath = $"{AppDomain.CurrentDomain.BaseDirectory}LogInfo.log";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="hostApplicationLifetime"></param>
        public LogInfoWorker(ILogger<LogInfoWorker> logger, IHostApplicationLifetime hostApplicationLifetime) => 
            (this.logger, this.hostApplicationLifetime) = (logger,hostApplicationLifetime);

        /// <summary>
        /// 服务启动时执行的操作
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            FileOperation(FileMode.OpenOrCreate, "StartAsync", "Service started.");
            logger.LogInformation($"{DateTime.Now} : Service has been requested to start.");

            await base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// 服务运行时执行的操作
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Task task = ServiceRunningMethod(stoppingToken);
                await Task.WhenAll(task);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                FileOperation(FileMode.Append, "ExecuteAsync", ex.ToString());
            }
            finally
            {
                BeforeStopMethod();

                //手动停止服务
                hostApplicationLifetime.StopApplication();
            }           
        }

        /// <summary>
        /// 服务停止时执行的操作
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"{DateTime.Now} : Service has been requested to stop.");
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// 运行时执行的方法
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private Task ServiceRunningMethod(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //记录日志
                    logger.LogInformation($"{DateTime.Now} : Service is running.");
                    FileOperation(FileMode.Append, "ServiceRunningMethod", "Service is running.");
                    Thread.Sleep(2000);
                }              
            }, stoppingToken);
        }

        /// <summary>
        /// 服务结束前的操作
        /// </summary>
        private void BeforeStopMethod()
        {
            logger.LogInformation($"{DateTime.Now} : Service is stopping.");

            Thread.Sleep(3000);
            logger.LogInformation($"{DateTime.Now} : Service stopped.");
            //记录服务已停止
            FileOperation(FileMode.Append, "ServiceRunningMethod", "Service stopped.");
        }

        /// <summary>
        /// 文件操作
        /// </summary>
        /// <param name="fileMode">操作类型</param>
        /// <param name="methodName">调用此方法的方法</param>
        /// <param name="message">写入的消息</param>
        private void FileOperation(FileMode fileMode, string methodName, string message)
        {
            FileStream fs = new FileStream(LogPath, fileMode, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine($"{DateTime.Now} : [{methodName}] {message}");
            sw.Close();
            fs.Close();
        }
    }
}


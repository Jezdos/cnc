using HslCommunication;
using HslCommunication.CNC.Fanuc;
using log4net;
using System.Text.Json;
using transport_common;

namespace transport_cnc
{
    public class CncClient(long deviceId, string server, int port, string path) : IConnectLifeCycle, IDevice
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(CncClient));

        private FanucSeries0i? fanuc;

        public readonly long _deviceId = deviceId;
        public readonly string _server = server;
        public readonly int _port = port;
        public readonly string _path = path;



        protected override Task InitAsync()
        {
            if (fanuc != null) fanuc.ConnectClose();
            fanuc = new FanucSeries0i(_server, _port);
            SetProgramPath();
            return Task.CompletedTask;
        }

        protected override async Task ConnectAsync()
        {
            OperateResult connect = await fanuc.ConnectServerAsync();
            if (connect.IsSuccess) base.ChangeStatus(ConnectStatus.CONNECTED);
        }

        //private readonly SemaphoreSlim ReconnectLock = new(1, 1); // 对象锁
        //private async Task AttemptConnectWithRetry()
        //{
        //    if (fanuc == null || Disposed) return;
        //    if (ReconnectLock.Wait(TimeSpan.Zero)) {
        //        base.ChangeStatus(ConnectStatus.CONNECTING);
        //        Task.Run(async () => {
        //            while (!Disposed)
        //            {
        //                try
        //                {
        //                    if (fanuc != null)
        //                    {
        //                        OperateResult connect = await fanuc.ConnectServerAsync();
        //                        if (connect.IsSuccess) base.ChangeStatus(ConnectStatus.CONNECTED);
        //                    }
        //                    if (Status == ConnectStatus.CONNECTED) return;
        //                }
        //                catch (Exception ex)
        //                {
        //                    logger.DebugFormat("Retry in 5 seconds. Error: {0}", ex);
        //                }
        //                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // 非阻塞延迟
        //            }
        //        });
        //    }
        //    await Task.CompletedTask;
        //}

        public override Task DisconnectAsync()
        {
            if (fanuc is not null) fanuc.ConnectClose();
            return Task.CompletedTask;
        }

        public override void Dispose() { }

        public async Task<string> Collect()
        {
            Dictionary<string, object> dataMap = new Dictionary<string, object>();
            if (fanuc is not null)
            {
                OperateResult<FanucSysInfo> read1 = fanuc.ReadSysInfo();    // 系统信息
                if (read1.IsSuccess)
                {
                    //dataMap.Add("CncType", read1.Content.CncType);
                    //dataMap.Add("Version", read1.Content.Version);
                    //dataMap.Add("Axes", read1.Content.Axes);
                    //dataMap.Add("TypeCode", read1.Content.TypeCode);
                    //dataMap.Add("MtType", read1.Content.MtType);
                    //dataMap.Add("Series", read1.Content.Series);
                    if(Status != ConnectStatus.CONNECTED) base.ChangeStatus(ConnectStatus.CONNECTED);
                    dataMap.Add("SysInfo", read1.Content);
                }
                else
                {
                    base.ChangeStatus(ConnectStatus.CONNECTING);
                }
                OperateResult<SysStatusInfo> read3 = fanuc.ReadSysStatusInfo(); // 系统状态
                if (read3.IsSuccess)
                {
                    //dataMap.Add("RunStatus", read3.Content.RunStatus.ToString());
                    //dataMap.Add("Edit", read3.Content.Edit);
                    //dataMap.Add("Motion", read3.Content.Motion);
                    //dataMap.Add("Alarm", read3.Content.Alarm);
                    //dataMap.Add("Dummy", read3.Content.Dummy);
                    //dataMap.Add("Emergency", read3.Content.Emergency);
                    //dataMap.Add("MSTB", read3.Content.MSTB);
                    //dataMap.Add("TMMode", read3.Content.TMMode);
                    //dataMap.Add("WorkMode", read3.Content.WorkMode);
                    dataMap.Add("SysStatusInfo", read3.Content);
                }
                OperateResult<SysAlarm[]> read4 = fanuc.ReadSystemAlarm();   // 警报信息
                if (read4.IsSuccess)
                {
                    dataMap.Add("SystemAlarm", read4.Content);
                }
                OperateResult<SysAllCoors> read5 = fanuc.ReadSysAllCoors(); // 坐标数据
                if (read5.IsSuccess)
                {
                    dataMap.Add("SysAllCoors", read5.Content);
                }
                OperateResult<double, double> read7 = fanuc.ReadSpindleSpeedAndFeedRate(); //主轴转速 给进倍率
                if (read7.IsSuccess)
                {
                    dataMap.Add("SpindleSpeed", read7.Content1);
                    dataMap.Add("SpindleFeedRate", read7.Content2);
                }
                OperateResult<double[]> read8 = fanuc.ReadFanucAxisLoad(); //伺服负载
                if (read8.IsSuccess)
                {
                    dataMap.Add("FanucAxisLoad", read8.Content);
                }
                OperateResult<CutterInfo[]> read9 = fanuc.ReadCutterInfos(); // 刀具补偿
                if (read9.IsSuccess)
                {
                    dataMap.Add("CutterInfos", read9.Content);
                }
                OperateResult<string> read10 = fanuc.ReadCurrentForegroundDir(); // 程序路径
                if (read10.IsSuccess)
                {
                    dataMap.Add("CurrentForegroundDir", read10.Content);
                }
                OperateResult<double[]> read11 = fanuc.ReadDeviceWorkPiecesSize(); // 工件尺寸
                if (read11.IsSuccess)
                {
                    dataMap.Add("DeviceWorkPiecesSize", read11.Content);
                }
                OperateResult<int> read12 = fanuc.ReadAlarmStatus(); //警报状态信息
                if (read12.IsSuccess)
                {
                    dataMap.Add("AlarmStatus", read12.Content);
                }
                OperateResult<long> read13 = fanuc.ReadTimeData(0); //读取开机时间
                if (read13.IsSuccess)
                {
                    long time = read13.Content;
                    dataMap.Add("Time_Boot", $"{time / 3600} H {time % 3600 / 60} M {time % 3600 % 60} S");
                }
                OperateResult<long> read14 = fanuc.ReadTimeData(1); //读取运行时间
                if (read14.IsSuccess)
                {
                    long time = read14.Content;
                    dataMap.Add("Time_Run", $"{time / 3600} H {time % 3600 / 60} M {time % 3600 % 60} S");
                }
                OperateResult<long> read15 = fanuc.ReadTimeData(2); //读取切割时间
                if (read15.IsSuccess)
                {
                    long time = read15.Content;
                    dataMap.Add("Time_Cut", $"{time / 3600} H {time % 3600 / 60} M {time % 3600 % 60} S");
                }
                OperateResult<long> read16 = fanuc.ReadTimeData(3); //读取循环时间
                if (read16.IsSuccess)
                {
                    long time = read16.Content;
                    dataMap.Add("Time_Cycle", $"{time / 3600} H {time % 3600 / 60} M {time % 3600 % 60} S");
                }
                OperateResult<long> read17 = fanuc.ReadTimeData(3); //读取空闲时间
                if (read17.IsSuccess)
                {
                    long time = read17.Content;
                    dataMap.Add("Time_Free", $"{time / 3600} H {time % 3600 / 60} M {time % 3600 % 60} S");
                }
                OperateResult<string> read18 = fanuc.ReadCurrentProgram(); //当前程序名
                if (read18.IsSuccess)
                {
                    dataMap.Add("CurrentProgram", read18.Content);
                }
                OperateResult<int> read19 = fanuc.ReadProgramNumber(); //当前程序号
                if (read19.IsSuccess)
                {
                    dataMap.Add("ProgramNumber", read19.Content);
                }
                OperateResult<int> read20 = fanuc.ReadCutterNumber();   // 当前的刀号
                if (read20.IsSuccess)
                {
                    dataMap.Add("CurrentUsedCutterNumber", read20.Content);
                }
                OperateResult<DateTime> read21 = fanuc.ReadCurrentDateTime(); // 机床时间
                if (read21.IsSuccess)
                {
                    dataMap.Add("CurrentDateTime", read21.Content);
                }
                OperateResult<int> read22 = fanuc.ReadCurrentProduceCount(); // 已加工数量
                if (read22.IsSuccess)
                {
                    dataMap.Add("CurrentProduceCount", read22.Content);
                }
                OperateResult<int> read23 = fanuc.ReadExpectProduceCount(); //总加工数量
                if (read23.IsSuccess)
                {
                    dataMap.Add("ExpectProduceCount", read23.Content);
                }
                OperateResult<string[]> read24 = fanuc.ReadAxisNames(); //轴信息
                if (read24.IsSuccess)
                {
                    dataMap.Add("AxisNames", read24.Content);
                }
                OperateResult<string[]> read25 = fanuc.ReadSpindleNames(); //主轴信息
                if (read25.IsSuccess)
                {
                    dataMap.Add("SpindleNames", read25.Content);
                }
                OperateResult<double> read26 = fanuc.ReadSpindleLoad();   // 主轴负载
                if (read26.IsSuccess)
                {
                    dataMap.Add("SpindleLoad", read26.Content);
                }
            }

            string json = JsonSerializer.Serialize(dataMap);
            return await Task.FromResult(json);
        }

        public async Task<bool> WriteProcessFile(ushort programNumber, string fileName)
        {
            OperateResult<int> readResult = await fanuc.ReadProgramNumberAsync(); //当前程序号
            if (readResult.IsSuccess)
            {
                if (readResult.Content == programNumber) return true;
            }
            // 先尝试删除指定程序，可能显示未知错误
            DeleteFile(fileName);
            if (await WriteProgram(fileName))
            {
                if (this.SetCurrentProgram(programNumber)) return true;
            }
            return false;
        }

        private bool DeleteFile(string fileName)
        {
            OperateResult result = fanuc.DeleteFile(_path + fileName);
            if (result.IsSuccess) return true;
            return false;
        }

        private void SetProgramPath()
        {
            fanuc.OperatePath = short.Parse(_path);
        }

        private bool SetCurrentProgram(ushort programNum)
        {
            OperateResult result = fanuc.SetCurrentProgram(programNum);
            if (result.IsSuccess) return true;
            return false;
        }

        private async Task<bool> WriteProgram(string fileName)
        {
            OperateResult result = await fanuc.WriteProgramFileAsync(fileName, 512, _path);
            if (result.IsSuccess) return true;
            return false;
        }

        private bool StartProcess(string fileName)
        {
            OperateResult result = fanuc.StartProcessing();
            if (result.IsSuccess) return true;
            return false;
        }

        public string Name => "CNC";

        public long DeviceId => _deviceId;
    }
}

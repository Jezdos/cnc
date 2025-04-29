using HslCommunication;
using HslCommunication.CNC.Fanuc;
using log4net;
using System.Text.Json;
using transport_common;

namespace transport_cnc
{
    public class CncClient(long deviceId, string server, int port, string path) : IDevice
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(CncClient));

        private FanucSeries0i? fanuc;

        public readonly long _deviceId = deviceId;
        public readonly string _server = server;
        public readonly int _port = port;
        public readonly string _path = path;


        #region IConnectLifeCycle

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

        protected override Task DisconnectAsync()
        {
            if (fanuc is not null) fanuc.ConnectClose();
            return Task.CompletedTask;
        }

        public override void Dispose() { }

        #endregion

        #region IDevice

        public override string DeviceName => "CNC";

        public override async Task<string> Collect()
        {
            Dictionary<string, object> dataMap = new Dictionary<string, object>();
            if (fanuc is not null)
            {
                OperateResult<FanucSysInfo> read1 = fanuc.ReadSysInfo();    // 系统信息
                if (read1.IsSuccess)
                {
                    if(Status != ConnectStatus.CONNECTED) base.ChangeStatus(ConnectStatus.CONNECTED);
                    dataMap.Add("SysInfo", read1.Content);

                    var tasks = new List<Task>  {
                        Task.Run(() => ProcessResult(dataMap, "SysStatusInfo", fanuc.ReadSysStatusInfo())), // 系统状态
                        Task.Run(() => ProcessResult(dataMap, "SystemAlarm", fanuc.ReadSystemAlarm())), // 警报信息
                        Task.Run(() => ProcessResult(dataMap, "SysAllCoors", fanuc.ReadSysAllCoors())), // 坐标数据
                        Task.Run(() => ProcessResult(dataMap, "SpindleSpeed", "SpindleFeedRate", fanuc.ReadSpindleSpeedAndFeedRate())), // /主轴转速 给进倍率
                        Task.Run(() => ProcessResult(dataMap, "FanucAxisLoad", fanuc.ReadFanucAxisLoad())), // 伺服负载
                        Task.Run(() => ProcessResult(dataMap, "CutterInfos", fanuc.ReadCutterInfos())), // 刀具补偿
                        Task.Run(() => ProcessResult(dataMap, "CurrentForegroundDir", fanuc.ReadCurrentForegroundDir())), // 程序路径
                        Task.Run(() => ProcessResult(dataMap, "DeviceWorkPiecesSize", fanuc.ReadDeviceWorkPiecesSize())), // 工件尺寸
                        Task.Run(() => ProcessResult(dataMap, "AlarmStatus", fanuc.ReadAlarmStatus())), // 警报状态信息
                        Task.Run(() => ProcessResult(dataMap, "Time_Boot", fanuc.ReadTimeData(0))), // 读取开机时间
                        Task.Run(() => ProcessResult(dataMap, "Time_Run", fanuc.ReadTimeData(1))), // 读取运行时间
                        Task.Run(() => ProcessResult(dataMap, "Time_Cut", fanuc.ReadTimeData(2))), // 读取切割时间
                        Task.Run(() => ProcessResult(dataMap, "Time_Cycle", fanuc.ReadTimeData(3))), // 读取循环时间
                        Task.Run(() => ProcessResult(dataMap, "Time_Free", fanuc.ReadTimeData(4))), // 读取空闲时间
                        Task.Run(() => ProcessResult(dataMap, "CurrentProgram", fanuc.ReadCurrentProgram())), // 当前程序名
                        Task.Run(() => ProcessResult(dataMap, "ProgramNumber", fanuc.ReadProgramNumber())), // 当前程序号
                        Task.Run(() => ProcessResult(dataMap, "CurrentUsedCutterNumber", fanuc.ReadCutterNumber())), // 当前的刀号
                        Task.Run(() => ProcessResult(dataMap, "CurrentDateTime", fanuc.ReadCurrentDateTime())), // 机床时间
                        Task.Run(() => ProcessResult(dataMap, "CurrentProduceCount", fanuc.ReadCurrentProduceCount())), // 已加工数量
                        Task.Run(() => ProcessResult(dataMap, "ExpectProduceCount", fanuc.ReadExpectProduceCount())), // 预期的总加工数量
                        Task.Run(() => ProcessResult(dataMap, "AxisNames", fanuc.ReadAxisNames())), // 轴信息
                        Task.Run(() => ProcessResult(dataMap, "SpindleNames", fanuc.ReadSpindleNames())), // 主轴信息
                        Task.Run(() => ProcessResult(dataMap, "SpindleLoad", fanuc.ReadSpindleLoad())), // 主轴信息
                    };

                    await Task.WhenAll(tasks);
                }
                else
                {
                    base.ChangeStatus(ConnectStatus.CONNECTING);
                }
            }

            string json = JsonSerializer.Serialize(dataMap);
            return await Task.FromResult(json);
        }

        #endregion

        #region function

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

        public override long GetKey()
        {
            return _deviceId;
        }

        // 统一处理结果的辅助方法
        private void ProcessResult<T>(Dictionary<string, object> dataMap, string key, OperateResult<T> result)
        {
            if (result.IsSuccess && result.Content is not null)
            {
                dataMap.Add(key, result.Content);
            }
        }

        private void ProcessResult<T1, T2>(Dictionary<string, object> dataMap, string key1, string key2, OperateResult<T1, T2> result)
        {
            if (result.IsSuccess)
            {
                if(result.Content1 is not null) dataMap.Add(key1, result.Content1);
                if(result.Content2 is not null) dataMap.Add(key1, result.Content2);
            }
        }

        #endregion


    }
}

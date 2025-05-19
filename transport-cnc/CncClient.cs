namespace transport_cnc
{
    using HslCommunication;
    using HslCommunication.CNC.Fanuc;
    using log4net;
    using transport_common;
    using transport_common.Command;

    /// <summary>
    /// Defines the <see cref="CncClient" />
    /// </summary>
    public class CncClient(long deviceId, string server, int port, string path, int interval) : IDevice(interval)
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(CncClient));

        private FanucSeries0i? fanuc;

        public readonly long _deviceId = deviceId;

        public readonly string _server = server;

        public readonly int _port = port;

        public readonly string _path = path;

        /// <summary>
        /// The InitAsync
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        protected override Task InitAsync()
        {
            if (fanuc != null) fanuc.ConnectServer();
            fanuc = new FanucSeries0i(_server, _port);
            // 设置默认路径
            // SetProgramPath();
            // 注册方法
            RegisterCommandRouter();
            return Task.CompletedTask;
        }

        /// <summary>
        /// The ConnectAsync
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        protected override async Task ConnectAsync()
        {
            OperateResult connect = await fanuc.ConnectServerAsync();
            if (connect.IsSuccess) base.ChangeStatus(ConnectStatus.CONNECTED);
        }

        /// <summary>
        /// The DisconnectAsync
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        protected override Task DisconnectAsync()
        {
            if (fanuc is not null) fanuc.ConnectClose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Gets the DeviceName
        /// </summary>
        public override string DeviceName => "CNC";

        /// <summary>
        /// The PollAsync
        /// </summary>
        /// <returns>The <see cref="Task{Dictionary{string, object}}"/></returns>
        protected override async Task<Dictionary<string, object>> PollAsync()
        {
            Dictionary<string, object> dataMap = new Dictionary<string, object>();
            if (fanuc is not null)
            {
                OperateResult<FanucSysInfo> read1 = fanuc.ReadSysInfo();    // 系统信息
                if (read1.IsSuccess)
                {
                    if (Status != ConnectStatus.CONNECTED) base.ChangeStatus(ConnectStatus.CONNECTED);
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
            return dataMap;
        }

        /// <summary>
        /// The WriteProcessFile
        /// </summary>
        /// <param name="programNumber">The programNumber<see cref="ushort"/></param>
        /// <param name="fileName">The fileName<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
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

        /// <summary>
        /// The DeleteFile
        /// </summary>
        /// <param name="fileName">The fileName<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool DeleteFile(string fileName)
        {
            OperateResult result = fanuc.DeleteFile(_path + fileName);
            if (result.IsSuccess) return true;
            return false;
        }

        /// <summary>
        /// The SetProgramPath
        /// </summary>
        private void SetProgramPath()
        {
            fanuc.OperatePath = short.Parse(_path);
        }

        /// <summary>
        /// The SetCurrentProgram
        /// </summary>
        /// <param name="programNum">The programNum<see cref="ushort"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool SetCurrentProgram(ushort programNum)
        {
            OperateResult result = fanuc.SetCurrentProgram(programNum);
            if (result.IsSuccess) return true;
            return false;
        }

        /// <summary>
        /// The WriteProgram
        /// </summary>
        /// <param name="fileName">The fileName<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        private async Task<bool> WriteProgram(string fileName)
        {
            OperateResult result = await fanuc.WriteProgramFileAsync(fileName, 512, _path);
            if (result.IsSuccess) return true;
            return false;
        }

        /// <summary>
        /// The StartProcess
        /// </summary>
        /// <param name="fileName">The fileName<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool StartProcess(string fileName)
        {
            OperateResult result = fanuc.StartProcessing();
            if (result.IsSuccess) return true;
            return false;
        }

        /// <summary>
        /// The GetKey
        /// </summary>
        /// <returns>The <see cref="long"/></returns>
        public override long GetKey()
        {
            return _deviceId;
        }

        // 统一处理结果的辅助方法

        /// <summary>
        /// The ProcessResult
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataMap">The dataMap<see cref="Dictionary{string, object}"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="result">The result<see cref="OperateResult{T}"/></param>
        private void ProcessResult<T>(Dictionary<string, object> dataMap, string key, OperateResult<T> result)
        {
            if (result.IsSuccess && result.Content is not null)
            {
                dataMap.Add(key, result.Content);
            }
        }

        /// <summary>
        /// The ProcessResult
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dataMap">The dataMap<see cref="Dictionary{string, object}"/></param>
        /// <param name="key1">The key1<see cref="string"/></param>
        /// <param name="key2">The key2<see cref="string"/></param>
        /// <param name="result">The result<see cref="OperateResult{T1, T2}"/></param>
        private void ProcessResult<T1, T2>(Dictionary<string, object> dataMap, string key1, string key2, OperateResult<T1, T2> result)
        {
            if (result.IsSuccess)
            {
                if (result.Content1 is not null) dataMap.Add(key1, result.Content1);
                if (result.Content2 is not null) dataMap.Add(key1, result.Content2);
            }
        }

        /// <summary>
        /// The RegisterCommandRouter
        /// </summary>
        protected override void RegisterCommandRouter()
        {
            base.RegisterHandler("STATUS_INFO", async parameters =>
            {
                // 实际设备控制逻辑
                object data = await Poll();
                return new CommandResponse { Data = data };
            });

            base.RegisterHandler("RESIGN_PROCESS", async parameters =>
            {
                // 实际设备控制逻辑
                return new CommandResponse { StatusCode = 500, Message = "ERROR Method!" };
            });
        }
    }
}

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using WinRT.Interop;

namespace SharpAppSync.Capture
{
    internal class CaptureData
    {
        public CaptureData(CanvasBitmap image) {            
            this.Image = image;
        }
        public CanvasBitmap Image { get; init; }
    }

    /// <summary>
    /// 画面取得コアクラス
    /// </summary>
    internal class GraphicCaptureCore : IDisposable
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GraphicCaptureCore()
        {
            
        }

        /// <summary>
        /// 取得フレーム
        /// </summary>
        public IObservable<CaptureData> CaptureStream
        {
            get
            {
                return this.CaptureSub;
            }
        }
        /// <summary>
        /// キャプチャのSubject
        /// </summary>
        private Subject<CaptureData> CaptureSub = new Subject<CaptureData>();
        

        #region メンバ変数
        /// <summary>
        /// DirextXデバイス
        /// </summary>
        private CanvasDevice? Dev { get; set; } = null;
        public CanvasDevice Device
        {
            get
            {
                if (this.Dev == null)
                {
                    throw new NullReferenceException();
                }
                return this.Dev;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private GraphicsCaptureItem? CaptureItem { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        private Direct3D11CaptureFramePool? FramePool { get; set; } = null;

        /// <summary>
        /// 取得セッション
        /// </summary>
        private GraphicsCaptureSession? CaptureSession { get; set; } = null;


        #endregion

        /// <summary>
        /// Captureする画面の選択
        /// </summary>
        /// <param name="hwnd">親画面ハンドル</param>
        /// <returns></returns>
        public async Task<GraphicsCaptureItem> SelectCaptureWindow(IntPtr hwnd)
        {
            //Capture画面の選択させアイテムの取得
            GraphicsCapturePicker picker = new GraphicsCapturePicker();
            InitializeWithWindow.Initialize(picker, hwnd);
            return await picker.PickSingleItemAsync();
        }

        /// <summary>
        /// Captureの初期化
        /// </summary>
        /// <param name="item">対象画面のGraphicsCaptureItem</param>
        public void Init(GraphicsCaptureItem item)
        {
            this.Dispose();

            this.CaptureItem = item;

            //デバイス作成
            this.Dev = new CanvasDevice();

            //フレイムプール作成
            this.FramePool = Direct3D11CaptureFramePool.Create(this.Dev, Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized, 2, item.Size);
            
        }        

        /// <summary>
        /// キャプチャ開始
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void StartCapture()
        {
            if (this.FramePool == null)
            {
                throw new Exception("No initialized");
            }
            if (this.CaptureSession != null)
            {
                this.StopCapture();
            }

            //取得イベントの設定
            this.FramePool.FrameArrived += FramePool_FrameArrived;

            this.CaptureSession = this.FramePool.CreateCaptureSession(this.CaptureItem);            
            this.CaptureSession.StartCapture();
        }

        /// <summary>
        /// キャプチャの停止
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void StopCapture()
        {
            if (this.FramePool == null)
            {
                throw new Exception("No initialized");
            }

            this.FramePool.FrameArrived -= FramePool_FrameArrived;

            //終了
            this.CaptureSession?.Dispose();
            this.CaptureSession = null;
        }

        /// <summary>
        /// 開放処理
        /// </summary>
        public void Dispose()
        {
            this.CaptureSession?.Dispose();
            this.CaptureSession = null;
            this.FramePool?.Dispose();
            this.FramePool = null;
            this.Dev?.Dispose();
            this.Dev = null;
        }


        /// <summary>
        /// 新しいフレームが来た時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FramePool_FrameArrived(Direct3D11CaptureFramePool sender, object args)
        {


            using (Direct3D11CaptureFrame? cf = sender.TryGetNextFrame())
            {
                if (cf == null)
                {
                    return;
                }
                
                //キャプチャ表示作成
                CanvasBitmap bitmap = CanvasBitmap.CreateFromDirect3D11Surface(this.Dev, cf.Surface);


                CaptureData data = new CaptureData(bitmap);
                this.CaptureSub.OnNext(data);
            }
        }
    }
}

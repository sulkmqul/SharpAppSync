using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.DirectX;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Capture;
using Windows.UI.Composition;
using WinRT.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SharpAppSync.Capture
{
    /// <summary>
    /// 画面キャプチャ用のControl
    /// </summary>
    public sealed partial class DxAppCaptureCotrol : UserControl
    {
        public DxAppCaptureCotrol()
        {
            InitializeComponent();

        }

        public bool StretchImage
        {
            get
            {
                if (this.Brush == null)
                {
                    return false;
                }

                if (this.Brush.Stretch == Microsoft.UI.Composition.CompositionStretch.Uniform)
                {
                    return true;
                }
                return false;
            }
            set
            {
                if (this.Brush == null)
                {
                    return;
                }
                if (value == true)
                {
                    this.Brush.Stretch = Microsoft.UI.Composition.CompositionStretch.Uniform;
                }
                else
                {
                    this.Brush.Stretch = Microsoft.UI.Composition.CompositionStretch.None;
                }
            }
        }


        #region メンバ変数
        /// <summary>
        /// 画面取得処理の本体
        /// </summary>
        private GraphicCaptureCore Core = new GraphicCaptureCore();

        /// <summary>
        /// 切り抜き矩形
        /// </summary>
        private Rect? CropRect = null;

        /// <summary>
        /// キャプチャアイテム
        /// </summary>
        GraphicsCaptureItem? CaptureItem = null;
        /// <summary>
        /// 表示用Device
        /// </summary>
        Microsoft.UI.Composition.CompositionGraphicsDevice? GraphicDevice = null;
        /// <summary>
        /// 表示用surface
        /// </summary>
        Microsoft.UI.Composition.CompositionDrawingSurface? Surface = null;
        /// <summary>
        /// Image描画用Visual
        /// </summary>
        Microsoft.UI.Composition.SpriteVisual? SpVisual = null;

        /// <summary>
        /// Image描画用Brush
        /// </summary>
        Microsoft.UI.Composition.CompositionSurfaceBrush? Brush = null;
        #endregion

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Init()
        {
            
        }

        
        /// <summary>
        /// キャプチャの開始
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public async Task InitCapture(IntPtr hwnd)
        {
            //既存のクリア
            this.Release();

            //画面の取得
            this.CaptureItem = await this.Core.SelectCaptureWindow(hwnd);
            if (this.CaptureItem == null)
            {
                return;
            }

            //初期化
            this.Core.Init(this.CaptureItem);

            //表示用の領域を作成
            Microsoft.UI.Composition.Visual a = ElementCompositionPreview.GetElementVisual(this);
            this.GraphicDevice = CanvasComposition.CreateCompositionGraphicsDevice(a.Compositor, this.Core.Device);
                        
            this.SpVisual = a.Compositor.CreateSpriteVisual();
            this.SpVisual.RelativeSizeAdjustment = System.Numerics.Vector2.One;
            ElementCompositionPreview.SetElementChildVisual(this, this.SpVisual);

            this.SetCropRect(this.CropRect);
            //this.Surface = this.GraphicDevice.CreateDrawingSurface(new Size(this.CaptureItem.Size.Width, this.CaptureItem.Size.Height), Microsoft.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            //this.Brush = a.Compositor.CreateSurfaceBrush(this.Surface);
            //this.Brush.HorizontalAlignmentRatio = 0.5f;
            //this.Brush.VerticalAlignmentRatio = 0.5f;
            //this.Brush.Stretch = Microsoft.UI.Composition.CompositionStretch.Uniform;
            //this.SpVisual.Brush = this.Brush;

            
            

            //描画処理
            this.Core.CaptureStream.Subscribe( x =>
            {
                this.RenderCaptureImage(x);

            });

            ///キャプチャ開始
            this.Core.StartCapture();            
        }

        /// <summary>
        /// キャプチャ停止
        /// </summary>
        public void StopCapture()
        {
            this.Core.StopCapture();
        }

        /// <summary>
        /// 開放処理
        /// </summary>
        public void Release()
        {
            this.SpVisual?.Dispose();
            this.SpVisual = null;
            this.Surface?.Dispose();
            this.Surface = null;
            this.GraphicDevice?.Dispose();
            this.GraphicDevice = null;

            this.Core.Dispose();
        }

        /// <summary>
        /// 切り抜き矩形の設定
        /// </summary>
        /// <param name="rect"></param>
        public void SetCropRect(Rect? rect)
        {
            this.CropRect = rect;
            if (this.CaptureItem == null)
            {
                return;
            }

            //サイズが変わったのでCrop表示用に作り替え
            this.DispatcherQueue.TryEnqueue(() =>
            {

                Size size = new Size(this.CaptureItem.Size.Width, this.CaptureItem.Size.Height);
                if (rect != null)
                {
                    size = new Size(rect.Value.Width, rect.Value.Height);
                }
                Microsoft.UI.Composition.Visual a = ElementCompositionPreview.GetElementVisual(this);
                this.Surface = this.GraphicDevice.CreateDrawingSurface(size, Microsoft.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
                this.Brush = a.Compositor.CreateSurfaceBrush(this.Surface);
                this.Brush.HorizontalAlignmentRatio = 0.5f;
                this.Brush.VerticalAlignmentRatio = 0.5f;
                this.Brush.Stretch = Microsoft.UI.Composition.CompositionStretch.Uniform;
                this.SpVisual.Brush = this.Brush;
            });

        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        /// <summary>
        /// 画面の表示
        /// </summary>
        /// <param name="data"></param>
        private void RenderCaptureImage(CaptureData data)
        {
            if (this.Surface == null)
            {
                return;
            }

            using (var session = CanvasComposition.CreateDrawingSession(this.Surface))
            {   
                session.Clear(Colors.Transparent);
                if (this.CropRect == null)
                {
                    session.DrawImage(data.Image);
                }
                else
                {
                    //session.DrawImage(data.Image, new Rect(0, 0, this.ActualWidth, this.ActualHeight), this.CropRect.Value);
                    session.DrawImage(data.Image, new Rect(0, 0, this.CropRect.Value.Width, this.CropRect.Value.Height), this.CropRect.Value);
                }
            }
        }

        
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //画面の大きさが変更された時
        private void AppCaptureCotrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //System.Diagnostics.Trace.WriteLine($"AppCaptureCotrol_SizeChanged {this.RenderSize}");        
        }
    }
}

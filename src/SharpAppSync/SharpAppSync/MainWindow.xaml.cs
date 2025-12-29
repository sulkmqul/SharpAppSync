using Microsoft.Graphics.Canvas;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
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
using Windows.UI.Popups;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SharpAppSync
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 切り抜き設定
        /// </summary>
        private CropSettingWindow? CropWindow = new CropSettingWindow();

        /// <summary>
        /// 画面取得の初期化
        /// </summary>
        /// <returns></returns>
        private async Task InitCapture()
        {
            //キャプチャの開始
            await this.CaptureControl.InitCapture(WindowNative.GetWindowHandle(this));

        }


        /// <summary>
        /// 通常と最大化の切り替え
        /// </summary>
        /// <param name="f">true=最大化 false=通常Window表示</param>
        private async Task ChangeWindowMode(bool f)
        {
            OverlappedPresenter? op = AppWindow.Presenter as OverlappedPresenter;
            if(op == null)
            {
                return;
            }

            bool windowflag = !f;

            //画面モード設定
            op.IsResizable = windowflag;
            op.IsAlwaysOnTop = windowflag;
            op.SetBorderAndTitleBar(windowflag, windowflag);

            if (f == true)
            {
                //最大化                
                op.Maximize();
                return;
            }

            //通常へ
            op.Restore();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        /// <summary>
        /// RootContentsの読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            this.CropWindow?.CropRectStream.Subscribe(x =>
            {
                this.CaptureControl.SetCropRect(x);
            });

            AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 640));
            

        }

        /// <summary>
        /// 画面が閉じられる処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Window_Closed(object sender, WindowEventArgs args)
        {
            this.CropWindow?.Close();
            this.CaptureControl.Release();
        }

        /// <summary>
        /// キャプチャ開始ボタンが押された時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuCaptureStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //画面キャプチャの初期化
                await this.InitCapture();
            }
            catch (Exception ex)
            {
                //メッセージ表示
                MessageDialog diag = new MessageDialog($"Capture Init Failed {ex.Message}", "");
                InitializeWithWindow.Initialize(diag, WindowNative.GetWindowHandle(this));
                await diag.ShowAsync();

            }
        }

        /// <summary>
        /// キャプチャ停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuCaptureStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CaptureControl.StopCapture();
            }
            catch (Exception ex)
            {
                //メッセージ表示
                MessageDialog diag = new MessageDialog($"Capture Stop Failed {ex.Message}", "");
                InitializeWithWindow.Initialize(diag, WindowNative.GetWindowHandle(this));
                await diag.ShowAsync();

            }
        }

        /// <summary>
        /// 画像のストレッチ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuImageStretch_Click(object sender, RoutedEventArgs e)
        {
            var item = (ToggleMenuFlyoutItem)sender;
            this.CaptureControl.StretchImage = item.IsChecked;
        }
        
        /// <summary>
        /// 切り抜きメニュー選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuImageCrop_Click(object sender, RoutedEventArgs e)
        {            
            this.CropWindow?.Activate();
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 最大化最小化切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuWindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            var item = (ToggleMenuFlyoutItem)sender;
            await this.ChangeWindowMode(item.IsChecked);
        }
    }
}

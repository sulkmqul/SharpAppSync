using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Reactive.Bindings;
using System.Reactive.Subjects;
using System.Reactive.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SharpAppSync;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CropSettingWindow : Window
{
    public CropSettingWindow()
    {
        InitializeComponent();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(400, 200));

        //値が変更されたら検知して取得
        Observable.CombineLatest(this.CropPosX, this.CropPosY, this.CropWidth, this.CropHeight)
            .Throttle(TimeSpan.FromMilliseconds(300)).Subscribe(x =>
        {            
            
            try
            {
                Rect rc = new Rect(Convert.ToDouble(x[0]), Convert.ToDouble(x[1]), Convert.ToDouble(x[2]), Convert.ToDouble(x[3]));
                this.CropRextSub.OnNext(rc);
            }
            catch
            {
                this.CropRextSub.OnNext(null);
            }
            
        });

    }
    /// <summary>
    /// 設定矩形取得
    /// </summary>
    public IObservable<Rect?> CropRectStream => this.CropRextSub;


    /// <summary>
    /// 矩形処理
    /// </summary>
    private Subject<Rect?> CropRextSub = new Subject<Rect?>();

    private ReactiveProperty<string> CropPosX { get; } = new ReactiveProperty<string>("");
    private ReactiveProperty<string> CropPosY { get; } = new ReactiveProperty<string>("");
    private ReactiveProperty<string> CropWidth { get; } = new ReactiveProperty<string>("");
    private ReactiveProperty<string> CropHeight { get; } = new ReactiveProperty<string>("");

    private void ButtonClear_Click(object sender, RoutedEventArgs e)
    {
        this.CropPosX.Value = "";
        this.CropPosY.Value = "";
        this.CropWidth.Value = "";
        this.CropHeight.Value = "";        
    }
}

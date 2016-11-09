﻿using System.IO;
using System.Windows;
using TabML.Core;
using TabML.Editor.Rendering;
using TabML.Editor.Tablature;
using TabML.Parser;
using TabML.Parser.Document;
using Bar = TabML.Editor.Tablature.Bar;

namespace TabML.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Browser.BrowserSettings.FileAccessFromFileUrls = CefSharp.CefState.Enabled;
            this.Browser.Address = Path.GetFullPath("../../../../TabMLWebRenderer/index.html");
        }

        private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                this.RenderTablature();
            }
        }

        private void RenderTablature()
        {
            var renderer = new PrimitiveRenderer(this.Browser);
            var tablature = TabMLParser.TryParse(File.ReadAllText(@"..\..\..\..\..\Documentations\samples\my home town.txt"));
            var style = new TablatureStyle();
            new TablatureRenderer(renderer, style, tablature).Render(new Point(style.Padding.Left, style.Padding.Top),
                                                                     new Size(
                                                                         800 - style.Padding.Left - style.Padding.Right,
                                                                         1200 - style.Padding.Top - style.Padding.Bottom));
        }

        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e)
        {

        }
    }
}

using System;
using The49.Maui.ContextMenu;

namespace DemoTest.Pages;

public partial class DefaultScrollViewTestPage : ContentPage
{
    public DefaultScrollViewTestPage()
    {
        InitializeComponent();
        var index = 0;
        emptyView = new Grid() { HeightRequest = 500 };
        rootLayout.Children.Add(emptyView);
        while (index < 15)
        {
            index++;
            //var botCell = new Image();
            //botCell.Source = "dotnet_bot.png";
            //var baiduCell = new Image();
            //baiduCell.Source = "https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png";

            var youdaoCell = new Image() { HeightRequest = 50 };
            youdaoCell.Source = "https://ydlunacommon-cdn.nosdn.127.net/cb776e6995f1c703706cf8c4c39a7520.png";

            //rootLayout.Children.Add(botCell);
            //rootLayout.Children.Add(baiduCell);
            rootLayout.Children.Add(youdaoCell);
            var text = new Label() { Margin = new Thickness(5), HeightRequest = 50, FontSize = 28, HorizontalTextAlignment = TextAlignment.Center, Text = index.ToString(), BackgroundColor = Colors.Gray };
            rootLayout.Children.Add(text);
            var drag = new DragGestureRecognizer();
            drag.CanDrag = true;
          
            drag.DragStarting += Drag_DragStarting;
            var drop = new DropGestureRecognizer();
            drop.Drop += Drop_Drop;
            drop.DragOver += Drop_DragOver;
            drop.DragLeave += Drop_DragLeave;
            drop.AllowDrop = true;
            text.GestureRecognizers.Add(drag);
            text.GestureRecognizers.Add(drop);

            var template = new DataTemplate(() =>
            {
                var menu = new Menu();
                menu.Children = new System.Collections.ObjectModel.ObservableCollection<MenuElement>()
                {
                    new The49.Maui.ContextMenu.Action()
                    {
                        Title = "Delete",
                    }
                };
                return menu;
            });
            ContextMenu.SetMenu(youdaoCell, template);
        }

        ChangeY.Clicked += ChangeY_Clicked;
        RemoveView.Clicked += RemoveView_Clicked;
        AddView.Clicked += AddView_Clicked;
    }

    private void AddView_Clicked(object sender, EventArgs e)
    {
        var youdaoCell = new Image() { HeightRequest = 50 };
        youdaoCell.Source = "https://ydlunacommon-cdn.nosdn.127.net/cb776e6995f1c703706cf8c4c39a7520.png";

        rootLayout.Children.Add(youdaoCell);
        var text = new Label() { Margin = new Thickness(5), HeightRequest = 50, FontSize = 28, HorizontalTextAlignment = TextAlignment.Center, Text = (rootLayout.Children.Count/2 + 1).ToString(), BackgroundColor = Colors.Gray };
        rootLayout.Children.Add(text);
    }

    private void RemoveView_Clicked(object sender, EventArgs e)
    {
        rootLayout.RemoveAt(3);
    }

    bool add = false;
    private void ChangeY_Clicked(object sender, EventArgs e)
    {
        if (add)
        {
            emptyView.HeightRequest = 500;
            add = false;
        }
        else
        {
            emptyView.HeightRequest = 300;
            add = true;
        }

        ;
    }

    View drag;
    private Grid emptyView;

    private void Drag_DragStarting(object sender, DragStartingEventArgs e)
    {
        drag = (sender as DragGestureRecognizer).Parent as View;
    }

    View drop;
    private void Drop_DragOver(object sender, DragEventArgs e)
    {
        drop = (sender as DropGestureRecognizer).Parent as View;
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void Drop_DragLeave(object sender, DragEventArgs e)
    {
        
    }

    private void Drop_Drop(object sender, DropEventArgs e)
    {
        (drag.Parent as Layout).Remove(drag);
        var target = (sender as DropGestureRecognizer).Parent as View;
        var targetIndex = (target.Parent as Layout).IndexOf(target);
        (target.Parent as Layout).Insert(targetIndex, drop);
    }
}
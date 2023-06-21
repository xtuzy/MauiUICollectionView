using Bogus;
using MauiUICollectionView;
using MauiUICollectionView.Layouts;
using Microsoft.Maui.Controls.Shapes;
using SharpConstraintLayout.Maui.Widget;
using Yang.Maui.Helper.Image;
using MAUICollectionView = MauiUICollectionView.MAUICollectionView;
namespace DemoTest.Pages;

public partial class DefaultTestPage : ContentPage
{
    internal static ViewModel viewModel;
    public DefaultTestPage()
    {
        viewModel = new ViewModel();

        InitializeComponent();
        var tableView = new MAUICollectionView();
        content.Content = tableView;
        tableView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
        tableView.Source = new Source(viewModel);
        tableView.ItemsLayout = new CollectionViewListLayout(tableView)
        {
        };

        //ѡ��Item
        var click = new TapGestureRecognizer();
        click.Tapped += (s, e) =>
        {
            var p = e.GetPosition(tableView);
#if IOS
            var indexPath = tableView.ItemsLayout.IndexPathForRowAtPointOfContentView(p.Value);
#else
            var indexPath = tableView.ItemsLayout.IndexPathForVisibaleRowAtPointOfCollectionView(p.Value);
#endif
            if (indexPath != null)
                tableView.SelectRowAtIndexPath(indexPath, false, ScrollPosition.None);
        };
        tableView.Content.GestureRecognizers.Add(click);

        //Header
        var headerButton = new Button() { Text = "Header GoTo20", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        headerButton.Clicked += (s, e) =>
        {
            tableView.ScrollToRowAtIndexPath(NSIndexPath.FromRowSection(20, 0), ScrollPosition.Top, true);
            Console.WriteLine("Clicked Header");
        };
        var headerView = new MAUICollectionViewViewHolder(headerButton, "Header");
        tableView.HeaderView = headerView;

        //Footer
        var footerButton = new Button() { Text = "Footer GoTo20", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        footerButton.Clicked += (s, e) =>
        {
            tableView.ScrollToRowAtIndexPath(NSIndexPath.FromRowSection(20, 0), ScrollPosition.Top, true);
            Console.WriteLine("Clicked Footer");
        };
        tableView.FooterView = new MAUICollectionViewViewHolder(footerButton, "Footer");

        tableView.BackgroundView = new Grid() { BackgroundColor = Colors.LightPink };

        this.Loaded += (sender, e) =>
        {
            Console.WriteLine("Loaded");
        };
        this.Appearing += (sender, e) =>
        {
            tableView.ReAppear();//�л�Pageʱ����Item���ɼ�, ��Ҫ���¼���
            Console.WriteLine("Appearing");
        };

        void r()
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                tableView.Dispatcher.Dispatch(() =>
                {
                    tableView.ContentView.ReMeasure();
                });
            });
        }

        //Add
        Add.Clicked += (sender, e) =>
        {
            var index = 2;
            (tableView.Source as Source).InsertData(index);
            tableView.InsertItems(NSIndexPath.FromRowSection(index, 0));
            r();
            tableView.ContentView.ReMeasure();
        };

        Remove.Clicked += (sender, e) =>
        {
            var index = 2;
            (tableView.Source as Source).RemoveData(index);
            tableView.RemoveItems(NSIndexPath.FromRowSection(index, 0)); r();
            tableView.ContentView.ReMeasure();
        };

        Move.Clicked += (sender, e) =>
        {
            var index = 3;
            var target = 1;
            (tableView.Source as Source).MoveData(index, target);
            tableView.MoveItem(NSIndexPath.FromRowSection(index, 0), NSIndexPath.FromRowSection(target, 0));
            r();
            tableView.ContentView.ReMeasure();
        };

        Change.Clicked += (sender, e) =>
        {
            var index = 2;
            (tableView.Source as Source).ChangeData(index);
            tableView.ChangeItem(new[] { NSIndexPath.FromRowSection(index, 0) });
            r();
            tableView.ContentView.ReMeasure();

        };

        Reload.Clicked += (sender, e) =>
        {
            tableView.ReloadData();
        };
    }
}
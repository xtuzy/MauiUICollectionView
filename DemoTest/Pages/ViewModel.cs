﻿using Bogus;
using MauiUICollectionView;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using SharpConstraintLayout.Maui.Widget;
using System.Collections.ObjectModel;
using System.Diagnostics;
using The49.Maui.ContextMenu;
using Yang.Maui.Helper.Image;

namespace DemoTest.Pages
{
    internal class ViewModel
    {
        public static Stopwatch Stopwatch = new Stopwatch();

        public static List<long> Times = new List<long>();
        public static int LimitCount = 300;
        public static void CalculateMeanMeasureTimeAsync(long time)
        {
            if (Times.Count >= LimitCount)
            {
                long count = 0;
                long max = 0;
                long min = 100;
                foreach (long t in Times)
                {
                    if (t > max)
                        max = t;
                    if (t < min)
                        min = t;
                    count += t;
                }
                Times.Clear();
                Task.Run(() =>
                {
                    Shell.Current.CurrentPage?.Dispatcher.Dispatch(async () =>
                    {
                        await Shell.Current.CurrentPage?.DisplayAlert("Alert", $" Measure {LimitCount} Items: All-{count} Mean-{count * 1.0 / LimitCount} Max-{max} Min-{min} ms", "OK");
                    });
                });
            }
            else
            {
                Times.Add(time);
            }
        }

        public List<Model> models;
        public ObservableCollection<Model> ObservableModels = new ObservableCollection<Model>();
        private Faker<Model> testModel;
        public ViewModel()
        {
            testModel = new Faker<Model>();
            testModel
                //.RuleFor(m => m.PersonIconUrl, f => f.Person.Avatar)
                .RuleFor(m => m.PersonName, f => f.Person.FullName)
                .RuleFor(m => m.PersonGender, f => f.Person.Gender.ToString())
                .RuleFor(m => m.PersonPhone, f => f.Person.Phone)
                .RuleFor(m => m.PersonTextBlogTitle, f => f.WaffleText(1, false))
                .RuleFor(m => m.PersonTextBlog, f => f.WaffleText(1, false))
                //.RuleFor(m => m.PersonImageBlogUrl, f => f.Image.PicsumUrl())
                .RuleFor(m => m.FirstComment, f => f.WaffleText(1, false))
                //.RuleFor(m => m.LikeIconUrl, f => f.Person.Avatar)
                //.RuleFor(m => m.CommentIconUrl, f => f.Person.Avatar)
                //.RuleFor(m => m.ShareIconUrl, f => f.Person.Avatar)
                ;
            models = testModel.Generate(1000);

            foreach(var m in models)
            {
                ObservableModels.Add(m);
            }
        }

        public Model Generate()
        {
            return testModel.Generate(1)[0];
        }

        public List<Model> Generate(int count)
        {
            return testModel.Generate(count);
        }
    }

    class Source : MAUICollectionViewSource
    {
        ViewModel ViewModel;
        public Source(ViewModel viewModel)
        {
            ViewModel = viewModel;

            heightForRowAtIndexPath += heightForRowAtIndexPathMethod;
            numberOfItemsInSection += numberOfRowsInSectionMethod;
            cellForRowAtIndexPath += cellForRowAtIndexPathMethod;
            numberOfSectionsInCollectionView += numberOfSectionsInTableViewMethod;
            reuseIdentifierForRowAtIndexPath += reuseIdentifierForRowAtIndexPathMethod;
            lastItemWillShow += lastItemWillShowMethod;
            willDragTo += WillDragToMethod;
        }

        private void WillDragToMethod(MAUICollectionView collectionView, NSIndexPath path1, NSIndexPath path2)
        {
            if (path1.Row == 0 || path2.Row == 0)//section的header不处理, 不然会出错
                return;
            collectionView.MoveItem(path1, path2);
            MoveData(path1.Row, path2.Row);
        }

        public void RemoveData(int index)
        {
            ViewModel.models.RemoveRange(index,3);
        }

        public void InsertData(int index)
        {
            ViewModel.models.InsertRange(index, ViewModel.Generate(3));
        }

        public void ChangeData(int index)
        {
            ViewModel.models[index] = ViewModel.Generate();
        }

        public void MoveData(int index, int toIndex)
        {
            var item = ViewModel.models[index];
            ViewModel.models.RemoveAt(index);
            ViewModel.models.Insert(toIndex, item);
        }

        public void LoadMoreOnFirst()
        {
            var models = ViewModel.Generate(20);
            ViewModel.models.InsertRange(0, models);
        }

        public void lastItemWillShowMethod(MAUICollectionView collectionView, NSIndexPath indexPath)
        {
            Task.Run(async () =>
            {
                ActivityIndicator loading = null;
                if (collectionView.FooterView.Content is VerticalStackLayout)
                {
                    loading = (collectionView.FooterView.Content as VerticalStackLayout)?.Children[0] as ActivityIndicator;
                }
                if (loading != null)
                {
                    collectionView.Dispatcher.Dispatch(() =>
                    {
                        loading.IsVisible = true;
                        loading.IsRunning = true;
                    });
                }
                await Task.Delay(2000);
                var models = ViewModel.Generate(20);
                ViewModel.models.AddRange(models);

                collectionView.ReloadDataCount();
                if (loading != null)
                {
                    collectionView.Dispatcher.Dispatch(() =>
                    {
                        loading.IsVisible = false;
                        loading.IsRunning = false;
                    });
                }
            });
        }

        public int numberOfSectionsInTableViewMethod(MAUICollectionView tableView)
        {
            return 1;
        }

        public int numberOfRowsInSectionMethod(MAUICollectionView tableView, int section)
        {
            return ViewModel.models.Count;
        }

        public string reuseIdentifierForRowAtIndexPathMethod(MAUICollectionView tableView, NSIndexPath indexPath)
        {
            if (indexPath.Row == 0)
            {
                return sectionCell;
            }
            //return itemCell;
            return itemCellSimple;
        }

        public float heightForRowAtIndexPathMethod(MAUICollectionView tableView, NSIndexPath indexPath)
        {
            var type = reuseIdentifierForRowAtIndexPathMethod(tableView, indexPath);
            switch (type)
            {
                case sectionCell:
                    return 40;
                case itemCellSimple:
                    return MAUICollectionViewViewHolder.MeasureSelf;
                default:
                    return 100;
            }
        }

        int newCellCount = 0;
        //给每个cell设置ID号（重复利用时使用）
        const string sectionCell = "sectionCell";
        const string itemCellSimple = "itemCellSimple";
        public MAUICollectionViewViewHolder cellForRowAtIndexPathMethod(MAUICollectionView tableView, NSIndexPath indexPath, MAUICollectionViewViewHolder oldViewHolder, double widthConstrain)
        {
            //从tableView的一个队列里获取一个cell
            var type = reuseIdentifierForRowAtIndexPathMethod(tableView, indexPath);
            MAUICollectionViewViewHolder cell;
            if (oldViewHolder != null)//只需局部刷新
            {
                cell = oldViewHolder;
                if (cell is ItemViewHolderSimple itemcellsimple)
                {
                    if(itemcellsimple != null) 
                        itemcellsimple.ModelView.TestButton.Text = $"Item Id={indexPath.Section}-{indexPath.Row}";
                }
            }
            else
            {
                cell = tableView.DequeueRecycledViewHolderWithIdentifier(type);

                if (type == sectionCell)
                {
                    var textCell = cell as SectionViewHolder;
                    //判断队列里面是否有这个cell 没有自己创建，有直接使用
                    if (textCell == null)
                    {
                        //没有,创建一个
                        textCell = new SectionViewHolder(new Grid(), type) { };
                    }

                    textCell.TextView.Text = $"Section={indexPath.Section} Row={indexPath.Row}";

                    cell = textCell;
                }else if (type == itemCellSimple)
                {
                    var simpleCell = cell as ItemViewHolderSimple;
                    if (simpleCell == null)
                    {
                        //没有,创建一个
                        simpleCell = new ItemViewHolderSimple(new ModelViewSimple() { }, type) { };
                        simpleCell.NewCellIndex = ++newCellCount;
                        simpleCell.ModelView.CommentCountLabel.Text = simpleCell.NewCellIndex.ToString();
                        var command = new Command<NSIndexPath>(execute: (NSIndexPath arg) =>
                        {
                            RemoveData(arg.Row);
                            tableView.NotifyItemRangeRemoved(arg);
                            tableView.ReMeasure();
                        });
                        simpleCell.InitMenu(command);
                        simpleCell.ModelView.TestButton.Clicked += async (sender, e) =>
                        {
                            await Shell.Current.CurrentPage?.DisplayAlert("Alert", $"Section={simpleCell.IndexPath.Section} Row={simpleCell.IndexPath.Row}", "OK");
                        };
                    }

                    simpleCell.ModelView.PersonName.Text = ViewModel.models[indexPath.Row].PersonName;
                    simpleCell.ModelView.PersonGender.Text = ViewModel.models[indexPath.Row].PersonGender;
                    simpleCell.ModelView.PersonPhone.Text = ViewModel.models[indexPath.Row].PersonPhone;
                    simpleCell.ModelView.PersonTextBlogTitle.Text = ViewModel.models[indexPath.Row].PersonTextBlogTitle;
                    simpleCell.ModelView.PersonImageBlog.Source = ViewModel.models[indexPath.Row].PersonImageBlogUrl;
                    simpleCell.ModelView.PersonTextBlog.Text = ViewModel.models[indexPath.Row].PersonTextBlog;
                    simpleCell.ModelView.TestButton.Text = $"Item Id={indexPath.Section}-{indexPath.Row}";

                    cell = simpleCell;
                }
            }
            cell.IndexPath = indexPath;
            if (cell.ContextMenu != null)
                cell.ContextMenu.IsEnable = tableView.CanContextMenu;
            return cell;
        }
    }

    class Model
    {
        public string PersonIconUrl { get; set; }
        public string PersonName { get; set; }
        public string PersonGender { get; set; }
        public string PersonPhone { get; set; }
        public string PersonTextBlogTitle { get; set; }
        public string PersonTextBlog { get; set; }
        public string PersonImageBlogUrl { get; set; }
        public string FirstComment { get; set; }
        public string LikeIconUrl { get; set; }
        public string CommentIconUrl { get; set; }
        public string ShareIconUrl { get; set; }
    }

    internal class SectionViewHolder : MAUICollectionViewViewHolder
    {
        public int NewCellIndex;

        public Label TextView;
        public SectionViewHolder(View itemView, string reuseIdentifier) : base(itemView, reuseIdentifier)
        {
            var grid = itemView as Grid;
            TextView = new Label();
            grid.Add(TextView);
            TextView.HorizontalOptions = LayoutOptions.Center;
            TextView.VerticalOptions = LayoutOptions.Center;
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();
            TextView.Text = string.Empty;
            UpdateSelectionState(false);
        }

        Color DefaultColor = Colors.LightYellow;
        public override void UpdateSelectionState(bool shouldHighlight)
        {
            if (DefaultColor == Colors.LightYellow)
            {
                DefaultColor = Content.BackgroundColor;
            }
            base.UpdateSelectionState(shouldHighlight);
            if (shouldHighlight)
                Content.BackgroundColor = Colors.LightGrey;
            else
                Content.BackgroundColor = DefaultColor;
        }
    }

    internal class ItemViewHolderSimple : MAUICollectionViewViewHolder
    {
        public int NewCellIndex;

        public ItemViewHolderSimple(View itemView, string reuseIdentifier) : base(itemView, reuseIdentifier)
        {
            ModelView = itemView as ModelViewSimple;
        }

        public ModelViewSimple ModelView;

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();
            ModelView.PersonIcon.Source = null;
            ModelView.PersonImageBlog.Source = null;
            //ModelView.CommentIcon.Source = null;
            //ModelView.ShareIcon.Source = null;
            UpdateSelectionState(false);
        }

        Color DefaultColor = Colors.LightYellow;
        public override void UpdateSelectionState(bool shouldHighlight)
        {
            if (DefaultColor == Colors.LightYellow)
            {
                DefaultColor = Content.BackgroundColor;
            }
            base.UpdateSelectionState(shouldHighlight);
            if (shouldHighlight)
                Content.BackgroundColor = Colors.Grey.WithAlpha(100);
            else
                Content.BackgroundColor = DefaultColor;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
#if ANDROID
            var av = this.Handler.PlatformView as Android.Views.View;
            var aContextMenu = new MauiUICollectionView.Gestures.AndroidContextMenu(av.Context, av);

            //设置PopupMenu样式, see https://learn.microsoft.com/en-us/xamarin/android/user-interface/controls/popup-menu
            aContextMenu.PlatformMenu.Inflate(Resource.Menu.popup_menu);
            aContextMenu.PlatformMenu.MenuItemClick += (s1, arg1) =>
            {
                MenuCommand.Execute(IndexPath);
            };
            ContextMenu = aContextMenu;
#endif
        }


        public Command MenuCommand;
        public void InitMenu(Command command)
        {
            MenuCommand = command;
#if IOS
            var menu = new Menu();
            var menuItem = new The49.Maui.ContextMenu.Action()
            {
                Title = "Delete",
                Command = command,
            };
            menuItem.SetBinding(The49.Maui.ContextMenu.Action.CommandParameterProperty, new Binding(nameof(IndexPath), source: this));
            menu.Children = new System.Collections.ObjectModel.ObservableCollection<MenuElement>()
            {
                menuItem
            };
            ContextMenu = new MauiUICollectionView.Gestures.iOSContextMenu(this, menu);
#elif WINDOWS || MACCATALYST
            var menu = new MenuFlyout();
            var menuItem = new MenuFlyoutItem()
            {
                Text = "Delete",
                Command = command,
                CommandParameter = IndexPath
            };
            menuItem.SetBinding(MenuFlyoutItem.CommandParameterProperty, new Binding(nameof(IndexPath), source: this));
            menu.Add(menuItem);
            ContextMenu = new MauiUICollectionView.Gestures.DesktopContextMenu(this, menu);
#endif
        }
    }

    class ModelViewSimple : ContainerLayout
    {
        public Image PersonIcon;
        public Label PersonName;
        public Label PersonGender;
        public Label PersonPhone;
        public Label PersonTextBlogTitle;
        public Label PersonTextBlog;
        public Image PersonImageBlog;
        public Button TestButton;
        public Label FirstComment;
        public Image LikeIcon;
        private Label LikeCountLabel;
        public Image CommentIcon;
        public Label CommentCountLabel;
        public Image ShareIcon;
        private Label ShareCountLabel;

        public ModelViewSimple()
        {
            this.BackgroundColor = new Color(30, 30, 30);
            var root = this;
            var layout = new Grid()
            {
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition(){ Height = GridLength.Auto },
                    new RowDefinition(){ Height = GridLength.Auto },
                    new RowDefinition(){ Height = GridLength.Auto },
                    new RowDefinition(){ Height = GridLength.Auto },
                    new RowDefinition(){ Height = GridLength.Auto },
                }
            };
            root.Add(layout);
            var PersonIconContainer = new Border() { WidthRequest = 40, HeightRequest = 40, StrokeShape = new RoundRectangle() { CornerRadius = new CornerRadius(20) } };
            PersonIcon = new Image() { BackgroundColor = Colors.AliceBlue };
            PersonIconContainer.Content = PersonIcon;
            var personInfoContainer = new HorizontalStackLayout();
            var personTextInfoContainer = new VerticalStackLayout();
             PersonName = new Label() { TextColor = Colors.White };
           var personOtherInfoContainer = new HorizontalStackLayout();
             PersonGender = new Label() { TextColor = Colors.White };
            PersonPhone = new Label() { Margin = new Thickness(5, 0, 0, 0), TextColor = Colors.White };
            personOtherInfoContainer.Add(PersonGender);
            personOtherInfoContainer.Add(PersonPhone);
            personTextInfoContainer.Add(PersonName);
            personTextInfoContainer.Add(personOtherInfoContainer);
            personInfoContainer.Add(PersonIconContainer);
            personInfoContainer.Add(personTextInfoContainer);
            PersonTextBlogTitle = new Label() { FontSize = 20, LineBreakMode = LineBreakMode.WordWrap, MaxLines = 2, TextColor = Colors.White, BackgroundColor = Colors.SlateGray };
            PersonTextBlog = new Label() { LineBreakMode = LineBreakMode.WordWrap, MaxLines = 3, TextColor = Colors.White, BackgroundColor = Colors.SlateGray };
            var imageInfoContainer = new Grid();
            PersonImageBlog = new Image() { WidthRequest = 100, HeightRequest = 100, BackgroundColor = Colors.AliceBlue, HorizontalOptions = LayoutOptions.Start };
            TestButton = new Button() { Text = "Hello", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
            imageInfoContainer.Add(PersonImageBlog);
            imageInfoContainer.Add(TestButton);
            layout.Add(personInfoContainer);
            layout.Add(PersonTextBlogTitle);
            layout.Add(imageInfoContainer);
            layout.Add(PersonTextBlog);

            Grid.SetRow(personInfoContainer, 0);
            Grid.SetRow(PersonTextBlogTitle, 1);
            Grid.SetRow(imageInfoContainer, 2);
            Grid.SetRow(PersonTextBlog, 3);

            var bottomIconBar = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition(){ Width = GridLength.Star },
                    new ColumnDefinition(){ Width = GridLength.Star },
                    new ColumnDefinition(){ Width = GridLength.Star },
                }
            };

            var likeContainer = new HorizontalStackLayout() { HorizontalOptions = LayoutOptions.Center };
            LikeIcon = new Image() { WidthRequest = 30, HeightRequest = 30, BackgroundColor = Colors.AliceBlue };
            LikeCountLabel = new Label { Text = "555", VerticalOptions = LayoutOptions.Center, TextColor = Colors.AliceBlue };
            likeContainer.Add(LikeIcon);
            likeContainer.Add(LikeCountLabel);
            var commentContainer = new HorizontalStackLayout() { HorizontalOptions = LayoutOptions.Center, BackgroundColor = Colors.Red };
            CommentIcon = new Image() { WidthRequest = 30, HeightRequest = 30, BackgroundColor = Colors.AliceBlue };
            CommentCountLabel = new Label { Text = "1000", VerticalOptions = LayoutOptions.Center, TextColor = Colors.AliceBlue };
            commentContainer.Add(CommentIcon);
            commentContainer.Add(CommentCountLabel);
            var shareContaner = new HorizontalStackLayout() { HorizontalOptions = LayoutOptions.Center };
            ShareIcon = new Image() { WidthRequest = 30, HeightRequest = 30, BackgroundColor = Colors.AliceBlue };
            ShareCountLabel = new Label { Text = "999", VerticalOptions = LayoutOptions.Center, TextColor = Colors.AliceBlue };
            shareContaner.Add(ShareIcon);
            shareContaner.Add(ShareCountLabel);

            Grid.SetColumn(likeContainer, 0);
            Grid.SetColumn(commentContainer, 1);
            Grid.SetColumn(shareContaner, 2);
            bottomIconBar.Add(likeContainer);
            bottomIconBar.Add(commentContainer);
            bottomIconBar.Add(shareContaner);

            layout.Add(bottomIconBar);
            Grid.SetRow(bottomIconBar, 4);
        }

        private void LikeIcon_Clicked(object sender, EventArgs e)
        {
            if ((LikeIcon.Source as FontImageSource)?.Color == Colors.Red)
                (LikeIcon.Source as FontImageSource).Color = Colors.White;
            else
                (LikeIcon.Source as FontImageSource).Color = Colors.Red;
            Console.WriteLine("Like Clicked");
        }

        public void BindingData()
        {
            PersonIcon.SetBinding(Image.SourceProperty, nameof(Model.PersonIconUrl));
            PersonName.SetBinding(Label.TextProperty, nameof(Model.PersonName));
            PersonGender.SetBinding(Label.TextProperty, nameof(Model.PersonGender));
            PersonPhone.SetBinding(Label.TextProperty, nameof(Model.PersonPhone));
            PersonTextBlogTitle.SetBinding(Label.TextProperty, nameof(Model.PersonTextBlogTitle));
            PersonTextBlog.SetBinding(Label.TextProperty, nameof(Model.PersonTextBlog));
            PersonImageBlog.SetBinding(Image.SourceProperty, nameof(Model.PersonImageBlogUrl));
        }
    }

    public class ContainerLayout : Layout
    {
        protected override ILayoutManager CreateLayoutManager()
        {
            return new ContainerLayoutManager(this);
        }
    }

    public class ContainerLayoutManager : LayoutManager
    {

        public ContainerLayoutManager(Microsoft.Maui.ILayout layout) : base(layout)
        {
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            var layout = Layout as Layout;
            (layout.Children[0] as IView).Arrange(bounds);
            return bounds.Size;
        }

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var layout = Layout as Layout;
            ViewModel.Stopwatch.Restart();
            var size = (layout.Children[0] as IView).Measure(widthConstraint, heightConstraint);
            ViewModel.Stopwatch.Stop();
            ViewModel.CalculateMeanMeasureTimeAsync(ViewModel.Stopwatch.ElapsedMilliseconds);
            return size;
        }
    }
}

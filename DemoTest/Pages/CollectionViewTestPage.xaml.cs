using Bogus;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using Yang.Maui.Helper.Device.Screen;

namespace DemoTest.Pages;

public partial class CollectionViewTestPage : ContentPage
{
#if WINDOWS || __ANDROID__ || __IOS__
    FrameRateCalculator fr;
#endif
    public CollectionViewTestPage()
    {
        InitializeComponent();

#if WINDOWS || __ANDROID__ || __IOS__
        if (fr == null)
        {
            fr = new FrameRateCalculator();
            fr.FrameRateUpdated += (value) =>
            {
                this.Dispatcher.Dispatch(() => fpsLabel.Text = value.Frames.ToString());
            };
            fr.Start();
        }
#endif
        var viewModel = new ViewModel();
        collectionView.BindingContext = viewModel;
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var view = new ModelViewSimple();
            view.BindingData();
            return view;
        });
        collectionView.ItemsSource = viewModel.ObservableModels;
        collectionView.SelectionMode = SelectionMode.Single;

        Add.Clicked += (sender, e) =>
        {
            var index = 2;
            viewModel.ObservableModels.Insert(index, viewModel.Generate(1)[0]);
        };

        Remove.Clicked += (sender, e) =>
        {
            var index = 2;
            viewModel.ObservableModels.RemoveAt(index);
        };

        Move.Clicked += (sender, e) =>
        {
            var index = 3;
            var target = 1;
            viewModel.ObservableModels.Move(3, 1);
        };

        Change.Clicked += (sender, e) =>
        {
            var index = 2;
            viewModel.ObservableModels[index] = viewModel.Generate(1)[0];
        };
    }
}
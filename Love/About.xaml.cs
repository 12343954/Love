using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;

namespace Love
{
    public partial class About : PhoneApplicationPage
    {
        PhotoChooserTask photoChooserTask;
        public About()
        {
            InitializeComponent();
            photoChooserTask = new PhotoChooserTask();
            
            photoChooserTask.Completed += photoChooserTask_Completed;
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                Image image = new Image();
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(e.ChosenPhoto);
                image.Source = bitmapImage;
                ContentPanel.Children.Add(image);
                MessageBox.Show(e.OriginalFileName);
                
                //MessageBox.Show(e.ChosenPhoto.Length.ToString());

                //Code to display the photo on the page in an image control named myImage.
                //System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                //bmp.SetSource(e.ChosenPhoto);
                //myImage.Source = bmp;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //photoChooserTask.Show();
            //return;
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
    }
}
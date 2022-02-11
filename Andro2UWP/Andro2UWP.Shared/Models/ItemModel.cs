// ItemModel
// Shared code

namespace Andro2UWP.Models
{
    using Microsoft.OneDrive.Sdk;
    using System.ComponentModel;
    using System.Linq;
    using Windows.UI.Xaml.Media.Imaging;

    public class ItemModel : INotifyPropertyChanged
    {
        private BitmapSource bitmap;

        public ItemModel(Item item)
        {
            this.Item = item;
        }

        public BitmapSource Bitmap
        {
            get
            {
                return this.bitmap;
            }
            set
            {
                this.bitmap = value;
                OnPropertyChanged("Bitmap");
            }
        }

        public string Icon
        {
            get
            {
                if (this.Item.Folder != null)
                {
                    return "ms-appx:///assets/app/folder.png";
                }
                else if (this.SmallThumbnail != null)
                {
                    return this.SmallThumbnail.Url;
                }

                return null;
            }
        }

        public string Id
        {
            get
            {
                return this.Item == null ? null : this.Item.Id;
            }
        }

        public Item Item { get; private set; }

        public string Name
        {
            get
            {
                return this.Item.Name;
            }
        }

        public Thumbnail SmallThumbnail
        {
            get
            {
                if (this.Item != null && this.Item.Thumbnails != null)
                {
                    var thumbnailSet = this.Item.Thumbnails.FirstOrDefault();
                    if (thumbnailSet != null)
                    {
                        return thumbnailSet.Small;
                    }
                }

                return null;
            }
        }

        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

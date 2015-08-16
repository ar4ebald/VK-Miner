using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VK_Miner.VK;
using VK_Miner.VK.Model;

namespace VK_Miner.DataModel
{
    public class PhotoDialogViewModel
    {
        public delegate RequestData CreatorDelegate(int offset);

        private int _currentPhotoIndex;
        public ObservableCollection<PhotoItem> Photos { get; }

        public PhotoDialogViewModel(CreatorDelegate creator, int offset, IEnumerable<PhotoItem> initialPhotos)
        {
            Photos = new ObservableCollection<PhotoItem>(initialPhotos);
            _currentPhotoIndex = offset;
        }
        public PhotoDialogViewModel() { }

        public class PhotoItem : Photo
        {
            public PhotoSize MaxSize { get; }

            public PhotoItem(JToken json) : base(json)
            {
                MaxSize = Sizes[0];
                for (var i = 1; i < Sizes.Length; i++)
                    if (Sizes[i].Width > MaxSize.Width)
                        MaxSize = Sizes[i];
            }
        }
    }
}

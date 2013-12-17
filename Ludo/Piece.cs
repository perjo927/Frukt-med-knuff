using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ludo
{
    /// <summary>
    /// Code representation of GUI Pieces (to be binded)
    /// </summary>
    public class Piece
    {
        #region Fields
        private int _x, _y;
        private String color;
        private bool isActive = false;
        #endregion


        #region Constructor
        public Piece(String color, int startX, int startY)
        {
            this.color = color;
            _x = startX;
            _y = startY;
        }
        #endregion



        #region Properties 
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }
        public bool IsActive
        {
            get { return isActive;}
            set { isActive = value; }
        }
        #endregion


        #region Methods
        public BitmapImage getImage()
        {
            BitmapImage image = new BitmapImage(new Uri("/images/" + color + ".png", UriKind.Relative));
            return image;
        }      
        #endregion
    }
}

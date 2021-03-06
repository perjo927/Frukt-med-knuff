﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudoRules
{
    /// <summary>
    /// The Nest collects inactive pieces
    /// </summary>
    public class Nest : BoardObject
    {
        #region Fields
        private Piece[] pieces;
        #endregion


        #region Constructor
        public Nest(Colors color, Piece[] pieces)
        {
            this.pieces = pieces;
            this.color = color;
        }
        #endregion


        #region Properties
        public Piece[] Pieces
        {
            get { return this.pieces;  }
        }
        public int Count
        {
            get { return this.pieces.Length; }
        }        
        #endregion


        #region Methods
        public Piece popPiece(int index)
        {
            Piece piece = pieces[index];
            return piece;
        }

        public override string ToString()
        {
            return String.Format("\nNest, Color: {0}", color.ToString());
        }

        #endregion  
    }
}

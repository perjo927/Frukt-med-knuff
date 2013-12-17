using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo.Engine
{
    /// <summary>
    /// Code-First Models which will generate database entries
    /// These classes are database-ready representations of the rule engine pieces & players
    /// </summary>
    public class LudoBoardTable
    {
        public LudoBoardTable()
        {
            this.Players = new List<PlayerTable>();
            this.Pieces = new List<PieceTable>();
        }

        [Key]
        public int key { get; set; }
        public bool IsActive { get; set; }
        public int PlayerTurn { get; set; }

        public virtual List<PlayerTable> Players { get; set; }
        public virtual List<PieceTable> Pieces { get; set; }
    }
    public class PlayerTable
    {
        [Key]
        public int key {get;set;}
        public int Color { get; set; }
        public bool IsActive { get; set; }
    }
    public class PieceTable
    {
        [Key]
        public int key { get; set; }
        public int Color { get; set; }
        public int ID { get; set; }
        public int Position { get; set; }
        public int Steps { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }
    }

    public class LudoBoardContext : DbContext
    {
        public DbSet<LudoBoardTable> LudoBoardTables { get; set; }
    }
}

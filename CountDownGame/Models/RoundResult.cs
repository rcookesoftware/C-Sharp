using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CountDownGame.Models;

public class RoundResult
{
    public List<char> Letters { get; set; } = new();
    public string Player1Word { get; set; } = string.Empty;
    public string Player2Word { get; set; } = string.Empty;
    public int Player1Points { get; set; }
    public int Player2Points { get; set; }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountDownGame.Models;

public class GameSettings
{
    // Brief suggests 6 rounds & 30-second clock; we’ll make these configurable. 
    public int MaxRounds { get; set; } = 6;
    public int RoundSeconds { get; set; } = 30;
}


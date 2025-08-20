using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CountDownGame.Models;

public class GameResult
{
    public DateTime PlayedAt { get; set; } = DateTime.Now;
    public string Player1Name { get; set; } = string.Empty;
    public int Player1Score { get; set; }
    public string Player2Name { get; set; } = string.Empty;
    public int Player2Score { get; set; }
    public List<RoundResult> Rounds { get; set; } = new();
}


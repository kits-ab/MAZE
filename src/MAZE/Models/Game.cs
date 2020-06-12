﻿using System.Collections.Generic;
using GameId = System.Int32;

namespace MAZE.Models
{
    public class Game
    {
        public Game(GameId id)
        {
            Id = id;
        }

        public GameId Id { get; }

        public World World { get; } = new World();

        public List<Player> Players { get; } = new List<Player>();
    }
}

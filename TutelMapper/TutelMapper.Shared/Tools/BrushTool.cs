﻿using System.Threading.Tasks;
using TutelMapper.Commands;
using TutelMapper.Util;
using TutelMapper.ViewModels;

namespace TutelMapper.Tools
{
    public class BrushTool : ITool
    {
        public bool CanUseOnDrag => true;

        public async Task Execute(TileInfo selectedTile, string[,] target, int x, int y, UndoStack undoStack)
        {
            if (target[x, y] == selectedTile.Name)
                return;
            await undoStack.Do(new PlaceTileCommand(target, x, y, selectedTile));
        }
    }
}
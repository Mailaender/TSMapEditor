﻿namespace TSMapEditor.Models
{
    public class LocalVariable
    {
        public LocalVariable(int index)
        {
            Index = index;
        }

        public int Index { get; }
        public string Name { get; set; }
        public bool InitialState { get; set; }
    }
}

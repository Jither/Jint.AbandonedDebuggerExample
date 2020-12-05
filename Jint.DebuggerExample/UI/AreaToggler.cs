using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.UI
{
    public class AreaToggler
    {
        private List<DisplayArea> areas = new List<DisplayArea>();

        public AreaToggler()
        {

        }

        public AreaToggler Add(DisplayArea area)
        {
            area.Visible = false;
            this.areas.Add(area);
            return this;
        }

        public void Show(DisplayArea showArea)
        {
            foreach (var area in areas)
            {
                area.Visible = showArea == area;
            }
        }
    }
}

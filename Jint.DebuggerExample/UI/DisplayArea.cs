using Jint.DebuggerExample.Utilities;
using System.Diagnostics;

namespace Jint.DebuggerExample.UI
{
    public abstract class DisplayArea
    {
        protected Display display;
        protected Bounds bounds;

        private bool invalidated;
        private bool visible = true;

        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    Invalidate();
                }
            }
        }

        protected DisplayArea(Display display, Bounds bounds)
        {
            this.display = display;
            this.bounds = bounds;
        }

        public abstract void Redraw();

        public void Update(bool force = false)
        {
            if (visible && (force || invalidated))
            {
                Redraw();
                invalidated = false;
            }
        }

        public void Invalidate()
        {
            invalidated = true;
        }
    }
}

﻿using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Manina.Windows.Forms
{
    [ToolboxBitmap(typeof(PagedControl))]
    [Designer(typeof(PagedControlDesigner))]
    [Docking(DockingBehavior.Ask)]
    [DefaultEvent("PageChanged")]
    [DefaultProperty("SelectedPage")]
    public partial class PagedControl : Control
    {
        #region Events
        /// <summary>
        /// Contains event data for events related to a single page.
        /// </summary>
        public class PageEventArgs : EventArgs
        {
            /// <summary>
            /// The page causing the event.
            /// </summary>
            public Page Page { get; private set; }

            public PageEventArgs(Page page)
            {
                Page = page;
            }
        }

        /// <summary>
        /// Contains event data for the <see cref="PageChanging"/> event.
        /// </summary>
        public class PageChangingEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Current page.
            /// </summary>
            public Page CurrentPage { get; private set; }
            /// <summary>
            /// The page that will become the current page after the event.
            /// </summary>
            public Page NewPage { get; set; }

            public PageChangingEventArgs(Page currentPage, Page newPage) : base(false)
            {
                CurrentPage = currentPage;
                NewPage = newPage;
            }
        }

        /// <summary>
        /// Contains event data for the <see cref="PageChanged"/> event.
        /// </summary>
        public class PageChangedEventArgs : EventArgs
        {
            /// <summary>
            /// The page that was the current page before the event.
            /// </summary>
            public Page OldPage { get; private set; }
            /// <summary>
            /// Current page.
            /// </summary>
            public Page CurrentPage { get; private set; }

            public PageChangedEventArgs(Page oldPage, Page currentPage)
            {
                OldPage = oldPage;
                CurrentPage = currentPage;
            }
        }

        /// <summary>
        /// Contains event data for the <see cref="PageValidating"/> event.
        /// </summary>
        public class PageValidatingEventArgs : CancelEventArgs
        {
            /// <summary>
            /// The page causing the event.
            /// </summary>
            public Page Page { get; private set; }

            public PageValidatingEventArgs(Page page)
            {
                Page = page;
            }
        }

        /// <summary>
        /// Contains event data for the <see cref="PagePaint"/> event.
        /// </summary>
        public class PagePaintEventArgs : PageEventArgs
        {
            /// <summary>
            /// Gets the graphics used to paint.
            /// </summary>
            public Graphics Graphics { get; private set; }

            public PagePaintEventArgs(Graphics graphics, Page page) : base(page)
            {
                Graphics = graphics;
            }
        }

        public delegate void PageEventHandler(object sender, PageEventArgs e);
        public delegate void PageChangingEventHandler(object sender, PageChangingEventArgs e);
        public delegate void PageChangedEventHandler(object sender, PageChangedEventArgs e);
        public delegate void PageValidatingEventHandler(object sender, PageValidatingEventArgs e);
        public delegate void PagePaintEventHandler(object sender, PagePaintEventArgs e);

        protected internal virtual void OnPageAdded(PageEventArgs e) { PageAdded?.Invoke(this, e); }
        protected internal virtual void OnPageRemoved(PageEventArgs e) { PageRemoved?.Invoke(this, e); }
        protected internal virtual void OnCurrentPageChanging(PageChangingEventArgs e) { PageChanging?.Invoke(this, e); }
        protected internal virtual void OnCurrentPageChanged(PageChangedEventArgs e) { PageChanged?.Invoke(this, e); }
        protected internal virtual void OnPageValidating(PageValidatingEventArgs e) { PageValidating?.Invoke(this, e); }
        protected internal virtual void OnPageValidated(PageEventArgs e) { PageValidated?.Invoke(this, e); }
        protected internal virtual void OnPageHidden(PageEventArgs e) { PageHidden?.Invoke(this, e); }
        protected internal virtual void OnPageShown(PageEventArgs e) { PageShown?.Invoke(this, e); }
        protected internal virtual void OnPagePaint(PagePaintEventArgs e) { PagePaint?.Invoke(this, e); }
        protected internal virtual void OnUpdateUIControls(EventArgs e) { UpdateUIControls?.Invoke(this, e); }

        /// <summary>
        /// Occurs when a new page is added.
        /// </summary>
        [Category("Behavior"), Description("Occurs when a new page is added.")]
        public event PageEventHandler PageAdded;
        /// <summary>
        /// Occurs when a page is removed.
        /// </summary>
        [Category("Behavior"), Description("Occurs when a page is removed.")]
        public event PageEventHandler PageRemoved;
        /// <summary>
        /// Occurs before the current page is changed.
        /// </summary>
        [Category("Behavior"), Description("Occurs before the current page is changed.")]
        public event PageChangingEventHandler PageChanging;
        /// <summary>
        /// Occurs after the current page is changed.
        /// </summary>
        [Category("Behavior"), Description("Occurs after the current page is changed.")]
        public event PageChangedEventHandler PageChanged;
        /// <summary>
        /// Occurs when the current page is validating.
        /// </summary>
        [Category("Behavior"), Description("Occurs when the current page is validating.")]
        public event PageValidatingEventHandler PageValidating;
        /// <summary>
        /// Occurs after the current page is successfully validated.
        /// </summary>
        [Category("Behavior"), Description("Occurs after the current page is successfully validated.")]
        public event PageEventHandler PageValidated;
        /// <summary>
        /// Occurs while the current page is changing and the previous current page is hidden.
        /// </summary>
        [Category("Behavior"), Description("Occurs when the current page is changing and the previous current page is hidden.")]
        public event PageEventHandler PageHidden;
        /// <summary>
        /// Occurs while the current page is changing and the new current page is shown.
        /// </summary>
        [Category("Behavior"), Description("Occurs while the current page is changing and the new current page is shown.")]
        public event PageEventHandler PageShown;
        /// <summary>
        /// Occurs when a page is painted.
        /// </summary>
        [Category("Appearance"), Description("Occurs when a page is painted.")]
        public event PagePaintEventHandler PagePaint;
        /// <summary>
        /// Occurs when UI controls need to be updated.
        /// </summary>
        [Category("Appearance"), Description("Occurs when UI controls need to be updated.")]
        public event EventHandler UpdateUIControls;
        #endregion

        #region Member Variables
        private int selectedIndex;
        private Page lastSelectedPage;
        private Page selectedPage;
        private BorderStyle borderStyle;
        private bool creatingUIControls;
        private int uiControlCount;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the index of the first page in the controls collection.
        /// </summary>
        internal int FirstPageIndex => uiControlCount;
        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        internal int PageCount => Controls.Count - uiControlCount;

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        [Editor(typeof(PagedControlEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Gets or sets the current page.")]
        public virtual Page SelectedPage
        {
            get => selectedPage;
            set => ChangePage(value, true);
        }

        /// <summary>
        /// Gets or sets the zero-based index of the current page.
        /// </summary>
        [Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Gets or sets the zero-based index of the current page.")]
        public virtual int SelectedIndex
        {
            get => selectedIndex;
            set => ChangePage(value == -1 ? null : Pages[value], true);
        }

        /// <summary>
        /// Gets or sets the collection of pages.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets the collection of pages.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual PageCollection Pages { get; }

        /// <summary>
        /// Gets or sets the drawing mode for the control.
        /// </summary>
        [Category("Behavior"), DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Description("Gets or sets the drawing mode for the control.")]
        public bool OwnerDraw { get; set; }

        /// <summary>
        /// Gets the rectangle that represents the client area of the control.
        /// </summary>
        [Browsable(false)]
        public new Rectangle ClientRectangle
        {
            get
            {
                var rect = base.ClientRectangle;
                if (BorderStyle != BorderStyle.None)
                    rect.Inflate(-1, -1);
                return rect;
            }
        }

        /// <summary>
        /// Gets or sets the border style of the control.
        /// </summary>
        [Category("Appearance"), DefaultValue(BorderStyle.FixedSingle)]
        [Description("Gets or sets the border style of the control.")]
        public BorderStyle BorderStyle { get => borderStyle; set { borderStyle = value; Invalidate(); } }

        /// <summary>
        /// Gets the size of the control when it is initially created.
        /// </summary>
        protected override Size DefaultSize => new Size(300, 200);

        /// <summary>
        /// Determines whether the control can navigate to the previous page.
        /// </summary>
        [Browsable(false)]
        public virtual bool CanGoBack => (Pages.Count != 0) && (SelectedIndex != -1) && !(ReferenceEquals(SelectedPage, Pages[0]));

        /// <summary>
        /// Determines whether the control can navigate to the next page.
        /// </summary>
        [Browsable(false)]
        public virtual bool CanGoNext => (Pages.Count != 0) && (SelectedIndex != -1) && !(ReferenceEquals(SelectedPage, Pages[Pages.Count - 1]));

        /// <summary>
        /// Gets the client rectangle where pages are located.
        /// </summary>
        [Browsable(false)]
        public override Rectangle DisplayRectangle => new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width, ClientRectangle.Height);

        /// <summary>
        /// Gets an array of UI controls hosted on the PagedControl.
        /// </summary>
        public virtual Control[] UIControls => new Control[0];
        #endregion

        #region Unused Methods - Hide From User
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override string Text { get => base.Text; set => base.Text = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new ControlCollection Controls => base.Controls;

#pragma warning disable CS0067
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler TextChanged;
#pragma warning restore CS0067
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPagedControl"/> class.
        /// </summary>
        public PagedControl()
        {
            creatingUIControls = false;
            uiControlCount = 0;

            Pages = new PageCollection(this);

            selectedIndex = -1;
            lastSelectedPage = null;
            selectedPage = null;

            borderStyle = BorderStyle.FixedSingle;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, true);

            CreateChildControls();

            OnUpdateUIControls(new EventArgs());
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Navigates to the previous page.
        /// </summary>
        public void GoBack()
        {
            if (CanGoBack)
                SelectedIndex = SelectedIndex - 1;
        }

        /// <summary>
        /// Navigates to the next page.
        /// </summary>
        public void GoNext()
        {
            if (CanGoNext)
                SelectedIndex = SelectedIndex + 1;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Create the UI controls.
        /// </summary>
        private void CreateChildControls()
        {
            creatingUIControls = true;

            foreach (Control control in UIControls)
            {
                Controls.Add(control);
            }
            uiControlCount = UIControls.Length;

            creatingUIControls = false;
        }

        /// <summary>
        /// Updates the display bounds and visibility of pages.
        /// </summary>
        internal void UpdatePages()
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                var page = Pages[i];

                if (i == SelectedIndex)
                {
                    page.Bounds = DisplayRectangle;
                    page.Invalidate();

                    page.Visible = true;
                }
                else
                {
                    page.Visible = false;
                }
            }
        }

        /// <summary>
        /// Changes the currently selected page to the given page.
        /// </summary>
        /// <param name="page">The page to make current.</param>
        /// <param name="raisePageEvents">Whether to raise page events while changing the page.</param>
        internal void ChangePage(Page page, bool raisePageEvents)
        {
            int index = (page == null) ? -1 : Pages.IndexOf(page);

            if (page != null && index == -1)
                throw new ArgumentException("Page is not found in the page collection.");

            if (selectedPage != null && page != null && selectedPage == page)
                return;

            if (raisePageEvents && selectedPage != null && selectedPage.CausesValidation)
            {
                PageValidatingEventArgs pve = new PageValidatingEventArgs(selectedPage);
                OnPageValidating(pve);
                if (pve.Cancel) return;

                OnPageValidated(new PageEventArgs(selectedPage));
            }

            if (raisePageEvents)
            {
                PageChangingEventArgs pce = new PageChangingEventArgs(selectedPage, page);
                OnCurrentPageChanging(pce);
                if (pce.Cancel) return;

                page = pce.NewPage;
                index = (page == null) ? -1 : Pages.IndexOf(page);
            }

            lastSelectedPage = selectedPage;
            selectedPage = page;
            selectedIndex = index;

            OnUpdateUIControls(new EventArgs());
            UpdatePages();

            if (raisePageEvents && lastSelectedPage != null)
                OnPageHidden(new PageEventArgs(lastSelectedPage));

            if (raisePageEvents && selectedPage != null)
                OnPageShown(new PageEventArgs(selectedPage));

            if (raisePageEvents)
                OnCurrentPageChanged(new PageChangedEventArgs(lastSelectedPage, selectedPage));
        }
        #endregion

        #region Overriden Methods
        protected override ControlCollection CreateControlsInstance()
        {
            return new PagedControlControlCollection(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            if (BorderStyle == BorderStyle.FixedSingle)
                ControlPaint.DrawBorder(e.Graphics, base.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
            else if (BorderStyle == BorderStyle.Fixed3D)
                ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, Border3DStyle.SunkenOuter);

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdatePages();
        }
        #endregion

        #region ControlCollection
        internal class PagedControlControlCollection : ControlCollection
        {
            private readonly PagedControl owner;

            public bool RaisePageEvents { get; set; }

            public PagedControlControlCollection(PagedControl ownerControl) : base(ownerControl)
            {
                RaisePageEvents = true;

                owner = ownerControl;
            }

            public override void Add(Control value)
            {
                if (owner.creatingUIControls)
                {
                    base.Add(value);
                    return;
                }

                if (!(value is Page page))
                {
                    throw new ArgumentException(string.Format("Only a Page can be added to a PagedControl. Expected type {0}, supplied type {1}.", typeof(Page).AssemblyQualifiedName, value.GetType().AssemblyQualifiedName));
                }

                base.Add(page);

                // site the page
                ISite site = owner.Site;
                if (site != null && page.Site == null)
                {
                    IContainer container = site.Container;
                    if (container != null)
                    {
                        container.Add(page);
                    }
                }

                if (RaisePageEvents)
                {
                    owner.OnPageAdded(new PageEventArgs(page));

                    if (owner.PageCount == 1) owner.ChangePage(page, true);

                    owner.OnUpdateUIControls(new EventArgs());
                    owner.UpdatePages();
                }
            }

            public override void Remove(Control value)
            {
                if (owner.creatingUIControls)
                {
                    base.Remove(value);
                    return;
                }

                if (!(value is Page page))
                {
                    throw new ArgumentException(string.Format("Only a Page can be removed from a PagedControl. Expected type {0}, supplied type {1}.", typeof(Page).AssemblyQualifiedName, value.GetType().AssemblyQualifiedName));
                }

                int index = owner.Pages.IndexOf(page);
                if (index == -1)
                {
                    throw new ArgumentException("Page not found in collection.");
                }

                if (RaisePageEvents)
                {
                    if (owner.PageCount == 1) // removing last page
                        owner.ChangePage(null, true);
                    else if (ReferenceEquals(owner.SelectedPage, page)) // removing selected page
                        owner.ChangePage(owner.Pages[index == 0 ? 1 : index - 1], true);
                }

                base.Remove(page);
                if (owner.PageCount == 0) // removed last page
                    owner.selectedIndex = -1;
                else if (owner.selectedIndex > owner.Pages.Count - 1)
                    owner.selectedIndex = owner.Pages.Count - 1;

                if (RaisePageEvents)
                {
                    owner.OnPageRemoved(new PageEventArgs(page));

                    owner.OnUpdateUIControls(new EventArgs());
                    owner.UpdatePages();
                }
            }

            public override Control this[int index] => base[index];
        }
        #endregion

        #region UITypeEditor
        internal class PagedControlEditor : ObjectSelectorEditor
        {
            protected override void FillTreeWithData(Selector selector, ITypeDescriptorContext context, IServiceProvider provider)
            {
                base.FillTreeWithData(selector, context, provider);

                var control = (PagedControl)context.Instance;

                foreach (var page in control.Pages)
                {
                    SelectorNode node = new SelectorNode(page.Name, page);
                    selector.Nodes.Add(node);

                    if (page == control.SelectedPage)
                        selector.SelectedNode = node;
                }
            }
        }
        #endregion
    }
}

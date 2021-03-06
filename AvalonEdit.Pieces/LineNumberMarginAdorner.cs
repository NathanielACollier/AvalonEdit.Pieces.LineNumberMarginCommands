﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AvalonEdit.Pieces
{
    public class LineNumberMarginAdorner : Adorner
    {

        public LineNumberMarginAdorner(LineNumberMarginWithCommands marginElement)
            : base(marginElement)
        {
            this.listView = new LineNumbersListView();
            this.AddVisualChild(this.listView); // this has to be there for events and interaction to work

            this.listView.SizeChanged += (_sender, _args) =>
            {
                trackListViewWidth();
            };

            // update the adorner layer
            AdornerLayer.GetAdornerLayer(marginElement).Update();

            // setup events that we will need to use to modify our list of line numbers
            marginElement.LineNumbersChangedDelayedEvent += MarginElement_LineNumbersChangedDelayedEvent;

            // need to initially populate line numbers that are already there
            populateLineNumbers(marginElement.uiLineInfoList, this.listView.LineNumbers);
        }


        private void populateLineNumbers(List<LineInfo> textLineInfoList,
                                        ObservableCollection<LineNumberDisplayModel> visualLines)
        {
            // determine what needs to be created and what needs hidden
            var nonExistantTextLines = from t in textLineInfoList
                                       where !visualLines.Any(v => v.LineNumber == t.Number)
                                       select t;

            var visualsToHide = from v in visualLines
                                where !textLineInfoList.Any(t => t.Number == v.LineNumber)
                                select v;

            var visualsToShow = from v in visualLines
                                where textLineInfoList.Any(t => t.Number == v.LineNumber)
                                select v;

            // create 
            foreach (var t in nonExistantTextLines)
            {
                visualLines.Add(new LineNumberDisplayModel
                {
                    IsInView = true,
                    LineNumber = t.Number,
                    ControlHeight = t.LineHeight
                });
            }

            // hide
            foreach (var v in visualsToHide)
            {
                v.IsInView = false;
            }

            // show
            foreach (var v in visualsToShow)
            {
                v.IsInView = true;
            }
        }

        private void MarginElement_LineNumbersChangedDelayedEvent(object sendor, EventArgs args)
        {
            var margin = sendor as LineNumberMarginWithCommands;
            populateLineNumbers(margin.uiLineInfoList, this.listView.LineNumbers);
        }


        private LineNumbersListView listView;



        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return listView;
        }


        double previousListViewWidth = 0;

        public event Action<object, LineNumberMarginListViewWidthChangedEventArgs> LineNumberListViewWidthChanged;

        public class LineNumberMarginListViewWidthChangedEventArgs : EventArgs
        {
            public double Width { get; set; }
        }


        private void trackListViewWidth()
        {
            if (listView.DesiredSize.Width != previousListViewWidth)
            {
                previousListViewWidth = listView.DesiredSize.Width;
                // fire off that width of listview changed
                if (this.LineNumberListViewWidthChanged != null)
                {
                    this.LineNumberListViewWidthChanged(this, new LineNumberMarginListViewWidthChangedEventArgs
                    {
                        Width = previousListViewWidth
                    });
                }
            }
        }


        protected override Size MeasureOverride(Size constraint)
        {
            listView.Measure(constraint);
            return listView.DesiredSize;
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            listView.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(listView.ActualWidth, listView.ActualHeight);
        }

    }
}

﻿using G_coder.Model;
using System;
using System.IO;
using System.Windows.Forms;

namespace G_coder.DxfConverter
{
    public delegate void OnCreatedLinesHandler(Fields fields);

    public class DxfConverter : IDxfConverter
    {
        private const string XStartMarker = " 10";
        private const string XEndMarker = " 11";
        private const string YStartMarker = " 20";
        private const string YEndMarker = " 21";
        private const byte FrameLength = 13;

        private string _layerName = "silikon";

        private string[] _dxfContent;

        private Fields _fields;

        public OnCreatedLinesHandler CreatedLinesHandler;

        public Fields Fields { get; set; } = new Fields();

        private void Calculate()
        {
            _fields = FindLines();
            _fields.SetNearestToCenterAsP0();
            _fields.CalculateDistances();
            RaiseCreatedLinesHandler(_fields);
        }

        public void ConvertToFields(string pathToFile)
        {
            //TODO add exception when pathToFile string is incorrect
            _dxfContent = File.ReadAllLines(pathToFile);
            ChangeDotToComma();
            Calculate();
        }

        public Fields GetFields()
        {
            return _fields;
        }

        private void ChangeDotToComma()
        {
            for (var i = 0; i < _dxfContent.Length; i++)
            {
                _dxfContent[i] = Convert.ToString(_dxfContent[i].Replace('.', ','));
            }
        }

        private Fields FindLines()
        {
            var fields = new Fields();
            double xStart = 0,
                yStart = 0,
                xEnd = 0,
                yEnd = 0;
            var fieldId = 0;

            try
            {
                for (var i = 0; i < _dxfContent.Length; i++)
                {
                    if (string.Equals(_dxfContent[i], _layerName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        for (var j = i; j < i + FrameLength; j++)
                        {
                            if (_dxfContent[j] == XStartMarker)
                            {
                                xStart = Math.Round(Convert.ToDouble(_dxfContent[j + 1]), 1);
                            }
                            if (_dxfContent[j] == XEndMarker)
                            {
                                xEnd = Math.Round(Convert.ToDouble(_dxfContent[j + 1]), 1);
                            }
                            if (_dxfContent[j] == YStartMarker)
                            {
                                yStart = Math.Round(Convert.ToDouble(_dxfContent[j + 1]), 1);
                            }
                            if (_dxfContent[j] == YEndMarker)
                            {
                                yEnd = Math.Round(Convert.ToDouble(_dxfContent[j + 1]), 1);
                                fields.Add(new Field(fieldId, xStart, xEnd, yStart, yEnd));
                                fieldId++;
                            }
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show(@"Open file first.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex.Message));
            }
            return fields;
        }

        public Fields GetRandomFields(int countOfFields, int graphWidth, int graphHeight)
        {
            //TODO change bufferToFrame using
            var randomLines = new Fields();
            const int bufferToFrame = 30;
            var rand = new Random();

            for (var i = 0; i < countOfFields; i++)
            {
                var startX = rand.Next(0, graphWidth - bufferToFrame);
                var endX = rand.Next(0, graphWidth - bufferToFrame);
                var startY = rand.Next(5, graphHeight - 10);
                var endY = rand.Next(5, graphHeight - 10);
                randomLines.Add(new Field(i, startX, endX, startY, endY));
            }
            randomLines.SetNearestToCenterAsP0();
            randomLines.CalculateDistances();

            RaiseCreatedLinesHandler(randomLines);

            return randomLines;
        }

        private void RaiseCreatedLinesHandler(Fields randomFields)
        {
            if (CreatedLinesHandler != null)
                CreatedLinesHandler(randomFields);
        }
    }
}
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Linq;
using System.IO;
using System;

namespace WPF_RtfTextEditor
{
    public partial class MainWindow : Window
    {
        string filename; // default name of file

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // initially the filename is empty
            filename = string.Empty;

            // filling comboboxes with values
            TextFontFamily.ItemsSource = Fonts.SystemFontFamilies.
                OrderBy(fontFamily => fontFamily.Source);
            FontSize.ItemsSource = new List<double>()
            { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            IndentSize.ItemsSource = new List<string>()
            { "0,5", "0,75", "1", "1,25", "1,5", "1,75", "2", "2,5" };

            // initializing default values
            TextFontFamily.Text = "Arial";
            FontSize.Text = "24"; 
            IndentSize.Text = "1"; 
        }

        // action when activated with the left mouse button
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }

        /* work with file */
        // opening file
        private void Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opening = new OpenFileDialog
            { Filter = "Rich Text|*.rtf" };
            if (opening.ShowDialog() == true)
            {
                filename = opening.FileName;
                this.Title = filename;
                FileStream fileStream = new FileStream(filename, FileMode.Open);
                TextRange textRange = new TextRange(MainText.Document.ContentStart,
                    MainText.Document.ContentEnd);
                textRange.Load(fileStream, DataFormats.Rtf);
                fileStream.Close();
            }
        }
        // saving file
        private void Save(object sender, RoutedEventArgs e)
        {
            if (filename != string.Empty)
            {
                this.Title = filename;
                File.WriteAllText(filename, string.Empty);
                FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate);
                TextRange fileRanges = new TextRange(MainText.Document.ContentStart,
                    MainText.Document.ContentEnd);
                fileRanges.Save(fileStream, DataFormats.Rtf);
                fileStream.Close();
            }
            else SaveAs(sender, e);
        }
        // saving (as) file
        private void SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saving = new SaveFileDialog();
            // creating filter - only rtf
            saving.Filter = "Rich Text|*.rtf";
            if (saving.ShowDialog() == true)
            {
                filename = saving.FileName;
                Save(sender, e);
            }
        }
        // saving file and exit from program
        private void SaveExit(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
            Application.Current.Shutdown();
        }
        // printing file 
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printing = new PrintDialog();
            if ((printing.ShowDialog() == true))
            {
                printing.PrintVisual(MainText as Visual, "printing as visual");
                printing.PrintDocument((((IDocumentPaginatorSource)MainText.Document).
                    DocumentPaginator), "printing as paginator");
            }
        }

        // just exit from program 
        private void Exit(object sender, RoutedEventArgs e)
        {
            // suggest to save the file when exiting the program
            MessageBoxResult result =
                MessageBox.Show("Would you like to save file before exiting?",
                "Exit?", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveExit(sender, e);
                    break;
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
            }
        }

        /* work with text */
        // changed of text font
        private void ChangedOfFontFamily(object sender, SelectionChangedEventArgs e)
        {
            if (TextFontFamily.SelectedItem != null)
                MainText.Selection.ApplyPropertyValue(Inline.FontFamilyProperty,
                    TextFontFamily.SelectedItem);
        }
        // choice of indent size
        private void ChangedOfIndentSize(object sender, SelectionChangedEventArgs e)
        {
            if (IndentSize.SelectedItem != null)
                MainText.Document.LineHeight = Convert.ToDouble(IndentSize.SelectedItem);
        }
        // choice of font size
        private void ChangedOfFontSize(object sender, SelectionChangedEventArgs e)
        {
            TextSelection selectedText = MainText.Selection;
            object currentFont = selectedText.GetPropertyValue(Inline.FontSizeProperty);
            if (selectedText.Start.GetOffsetToPosition(selectedText.End) == 0) return;
            if (float.TryParse(FontSize.SelectedItem.ToString(), out _) &&
                currentFont.ToString() != FontSize.SelectedItem.ToString())
                selectedText.ApplyPropertyValue(Inline.FontSizeProperty,
                    FontSize.SelectedItem.ToString());
        }
        // choice of foreground color
        private void SelectedColorOfForegroundChanged(object sender,
            RoutedPropertyChangedEventArgs<Color?> e)
        {
            TextSelection selectedText = MainText.Selection;
            Color currentColor = (Color)e.NewValue;
            if (currentColor != null)
                selectedText.ApplyPropertyValue(TextElement.ForegroundProperty,
                    (SolidColorBrush)(new BrushConverter().ConvertFrom(currentColor.ToString())));
        }
        // choice of background color
        private void SelectedColorOfBackgroundChanged(object sender,
            RoutedPropertyChangedEventArgs<Color?> e)
        {
            TextSelection selectedText = MainText.Selection;
            Color currentColor = (Color)e.NewValue;
            if (currentColor != null)
                selectedText.ApplyPropertyValue(TextElement.BackgroundProperty,
                    (SolidColorBrush)(new BrushConverter().ConvertFrom(currentColor.ToString())));
        }

        // also closing the program (through the button)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // suggest to save the file when exiting the program
            MessageBoxResult result =
                MessageBox.Show("Would you like to save file before exiting?",
                "Exit?", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveExit(sender, e);
                    break;
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
            }
        }
    }
}

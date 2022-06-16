using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using System.Windows;
namespace asu
{
    internal class WordSolver
    {
        private FileInfo _fileinfo;

        public WordSolver(string filename)
        {
            if (File.Exists(filename))
            {
                _fileinfo = new FileInfo(filename);
            }
            else
            {
                Exception ex = new Exception("Файл шаблона не найден");
                MessageBox.Show(ex.Message);
                return;
            }
        }

        internal bool EditRows(int rowCount)
        {

        //    Word.Application App = null;
        //    try
        //    {
        //        App = new Word.Application();
        //        Object file = _fileinfo.FullName;

        //        Word.Application wordApp = null;

        //        object missing = System.Reflection.Missing.Value;

        //        Word.Document wordDoc = null;
        //        if (File.Exists(_fileinfo.ToString()) && wordApp != null)
        //        {
        //            object readOnly = ;
        //            object isVisible = true;
        //            wordDoc = wordApp.Documents.Open(ref _fileinfo, ref missing,
        //                                             ref readOnly, ref missing, ref missing, ref missing,
        //                                             ref missing, ref missing, ref missing, ref missing,
        //                                             ref missing, ref isVisible, ref missing, ref missing, ref missing,
        //                                             ref missing);
        //        }
        //        if (tables.Count > 0)
        //        {
        //            //Get the first table in the document
        //            Table table = tables[1];

        //            int rowsCount = table.Rows.Count;
        //            int coulmnsCount = table.Columns.Count;

        //            for (int i = 0; i < 25; i++)
        //            {
        //                Word.Row row = table.Rows.Add(ref missing);

        //                for (int j = 1; j <= coulmnsCount; j++)
        //                {
        //                    row.Cells[j].Range.Text = string.Format(@"{ 0}
        //                    { 1}", i, j);
        //            row.Cells[j].WordWrap = true;
        //            row.Cells[j].Range.Underline = WdUnderline.wdUnderlineNone;
        //            row.Cells[j].Range.Bold = 0;
        //        }
        //    }
        

        //Object NewFileName = Path.Combine(_fileinfo.DirectoryName, Path.GetFileNameWithoutExtension(_fileinfo.Name)
        //                 + "edited.docx");
        //        App.ActiveDocument.SaveAs2(NewFileName);
        //        App.Quit();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return false;
        //    }
        //    finally
        //    {
        //        if (App != null)
        //            App.ActiveDocument.Close();
        //    }

            return true;
        }

        internal void Saver(string dir, int changer)
        {
            Word.Application App = null;
            try
            {
                App = new Word.Application();
                Object file = _fileinfo.FullName;
                Object missing = Type.Missing;

                App.Documents.Open(file);

                
                Object NewFileName =null;
                if (changer != 3)
                {
                    NewFileName = Path.Combine(dir);
                    App.ActiveDocument.SaveAs2(NewFileName);
                }
                else
                {
                    NewFileName = Path.Combine(dir.Substring(0, dir.Length-4) + ".xps");
                    App.ActiveDocument.SaveAs2(NewFileName, FileFormat: Word.WdSaveFormat.wdFormatXPS);
               

                }
                App.ActiveDocument.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ;
            }
            finally
            {
                if (App != null)
                    App.Quit();
            }
            return;
        }
    

    internal string Process(Dictionary<string, string> items)
        {
            Word.Application App = null;
            Object NewFileName2 = null;
            try
            {
                App = new Word.Application();
                Object file = _fileinfo.FullName;
                Object missing = Type.Missing;

                App.Documents.Open(file);

                foreach (var item in items)
                {
                    Word.Find find = App.Selection.Find;
                    find.Text = item.Key;
                    find.Replacement.Text = item.Value;

                    Object wrap = Word.WdFindWrap.wdFindContinue;
                    Object replace = Word.WdReplace.wdReplaceAll;
                    find.Execute(FindText: Type.Missing,
                        MatchCase: false,
                        MatchWholeWord: false,
                        MatchWildcards: false,
                        MatchSoundsLike: missing,
                        MatchAllWordForms: false,
                        Forward: true,
                        Wrap: wrap,
                        Format: false,
                        ReplaceWith: missing, Replace: replace
                        );
                }
                Object NewFileName = Path.Combine(_fileinfo.DirectoryName, Path.GetFileNameWithoutExtension(_fileinfo.Name)
                        + DateTime.Now.ToString("dd-MM-yyyy") + ".docx");
                App.ActiveDocument.SaveAs2(NewFileName);
                 NewFileName2 = Path.Combine(_fileinfo.DirectoryName, Path.GetFileNameWithoutExtension(_fileinfo.Name)
        + DateTime.Now.ToString("dd-MM-yyyy") + ".XPS");
                App.ActiveDocument.SaveAs2(NewFileName2, FileFormat: Word.WdSaveFormat.wdFormatXPS);
                App.ActiveDocument.Close();
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message); 
                return "";
            }
            finally
            {
                if (App != null)
                App.Quit();
            }
            return NewFileName2.ToString();
        }
    }
}
